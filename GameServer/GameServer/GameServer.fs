namespace Zrpg.Game

open System
open System.IO
open System.Text
open System.Net.WebSockets
open System.Threading
open System.Threading.Tasks
open Newtonsoft.Json
open System.Diagnostics
open Zrpg.Game
open Zrpg.Commons
open Zrpg.Commons.Bundle

open Logging

module GameServer =
  let uuid () = Guid.NewGuid().ToString()

  type Token = string

  type Cmd = Msg * AsyncReplyChannel<Reply>

  [<Interface>]
  type IGameServer =
    abstract ApiHandler: Pario.WebServer.Handler
    abstract WebSocketHandler: Net.HttpListenerContext -> bool Async

  type private GameServer (log:LogBundle.Log) =
    //let game = GameRunner.Game(gameState, log)
    //let webServer = Pario.WebServer.Server(log)
    let discovery = Zrpg.Discovery.createLocal() |> Async.RunSynchronously

    let baseClassStats heroClass =
      match heroClass with
      | Warrior -> 
        { strength = 10.0
          stamina = 10.0
          groundTravelSpeed = 100.0
        }

    let raceStatMod race =
      match race with
      | Human ->
        { strength = 1.0
          stamina = 1.1
          groundTravelSpeed = 1.0
        }

    let addGarrison (msg:AddGarrison) (state:GameState): GameState * AddGarrisonReply =
      log.Debug <| sprintf "Adding garrison %s" msg.name

      // Check if the client has a world already.
      if state.clientWorlds.ContainsKey msg.clientId then
        (state, ClientHasWorld)
      else if state.clientGarrisons.ContainsKey msg.clientId then
        (state, ClientHasGarrison)
      else
        let stats = {
          goldIncome = 10
          heroes = Array.empty
        }

        let race = msg.race
        let raceId = race.ToString()

        // Get the starting zone for the race.
        let startingZone =
          match state.startingZones.TryFind raceId with
          | None -> sprintf "Starting zone not defined for race %A" race |> failwith
          | Some zoneId ->
            match state.zones.TryFind zoneId with
            | None -> sprintf "Starting zone defined for race %A, but does not map to an actual zone" race |> failwith
            | Some zone -> zone

        // Lookup the starting zone connections.
        let zoneConnections =
          match state.zoneConnections.TryFind startingZone.id with
          | None -> Map.empty
          | Some ls -> Map.add startingZone.id ls Map.empty

        // Give ownership of the startingZone to the client.
        let zoneOwners = Map.add startingZone.id msg.clientId Map.empty
        // Discover the starting zone
        let discoveredZones = [| startingZone.id |]
        // Make the world.
        let world = {
          id = uuid()
          clientId = msg.clientId
          zoneConnections = zoneConnections
          zoneOwners = zoneOwners
          discoveredZones = discoveredZones
          discoveredRegions = Array.empty
        }

        let garrison = {
          id = uuid()
          clientId = msg.clientId
          worldId = world.id
          name = msg.name
          race = msg.race
          faction = msg.faction
          ownedRegions = Set []
          ownedZones = Set [ startingZone.id ]
          vaultId = ""
          stats = stats
        }

        let state = {
          state with
            clientGarrisons = state.clientGarrisons.Add(msg.clientId, garrison.id)
            garrisons = state.garrisons.Add(garrison.id, garrison)
            worlds = state.worlds.Add(msg.clientId, world)
            clientWorlds = state.clientWorlds.Add(world.id, world.id)
        }

        (state, AddGarrisonReply.Success)

    let addHero (msg:AddHero) (state:GameState): GameState * AddHeroReply =
      // Check and make sure there's not a duplicate hero name.
      if state.heroNames.Contains msg.name then
        state, AddHeroReply.NameTaken
      else
        let stats =
          let classStats = baseClassStats msg.heroClass
          let raceStats = raceStatMod msg.race
          classStats * raceStats

        let startingZoneId =
          match state.startingZones.TryFind (msg.race.ToString()) with
          | None -> sprintf "Starting zone is not defined for %A" msg.race |> failwith
          | Some zoneId -> zoneId

        let hero = {
          id = uuid()
          zoneId = startingZoneId
          clientId = msg.clientId
          name = msg.name
          race = msg.race
          faction = msg.faction
          gender = msg.gender
          heroClass = msg.heroClass
          level = 1
          stats = stats
        }

        // Create the inventory for the hero.
        let inventory =
          let slots = 
            [
              for i in 1 .. 10 do
                let slot = {
                  position = i
                  itemRecordId = None
                }
                yield slot
            ] |> Array.ofList

          let panes =
            [ { position = 0
                slots = slots
              }
            ] |> Array.ofList

          let inventory = {
            HeroInventory.id = hero.id
            heroId = hero.id
            panes = panes
          }
          inventory

        // Lookup the client's garrison, and add the hero to it.
        let garrison = 
          match state.clientGarrisons.TryFind msg.clientId with
          | None -> sprintf "Client has no garrison" |> failwith
          | Some id ->
            match state.garrisons.TryFind id with
            | None -> sprintf "Client has a garrison, but garrison %s doesn't contain any data" id |> failwith
            | Some garrison -> garrison

        let stats = {
          garrison.stats with
            heroes = garrison.stats.heroes |> Array.append [| hero.id |]
        }
        let garrison = {
          garrison with
            stats = stats
        }

        let state = {
          state with
            heroInventories = state.heroInventories.Add (inventory.id, inventory)
            heroNames = state.heroNames.Add hero.name
            heroes = state.heroes.Add(hero.id, hero)
            garrisons = state.garrisons.Add (garrison.id, garrison)
        }
        (state, AddHeroReply.Success hero.id)

    let handleMsg msg state =
      match msg with
      | AddGarrison msg ->
        addGarrison msg state
        |> fun (s, r) -> (s, AddGarrisonReply r)

      | AddHero msg ->
        addHero msg state
        |> fun (s, r) -> (s, AddHeroReply r)

      | AddRegion msg ->
        match state.regionNames.Contains msg.name with
        | true -> state, AddRegionReply.RegionExists |> AddRegionReply
        | false ->
          let region = {
            id = uuid()
            name = msg.name
            zones = Array.empty
          }
          let state = {
            state with
              regionNames = state.regionNames.Add region.name
              regions = state.regions.Add(region.id, region)
          }

          state, AddRegionReply.Success region.id |> AddRegionReply

      | AddZone msg ->
        match state.zoneNames.Contains msg.name with
        | true -> state, AddZoneReply.ZoneExists |> AddZoneReply
        | false ->
          match state.regions.TryFind msg.regionId with
          | None -> state, AddZoneReply.RegionDoesNotExist |> AddZoneReply
          | Some region ->
            let zone: Zone = {
              id = uuid()
              regionId = msg.regionId
              name = msg.name
              terrain = msg.terrain
            }
            let region = {
              region with
                zones = region.zones |> Array.append [| zone.id |]
            }
            let state = {
              state with
                zoneNames = state.zoneNames.Add zone.name
                zones = state.zones.Add(zone.id, zone)
            }
            state, zone.id |> AddZoneReply.Success |> AddZoneReply

      | GetClientGarrison clientId ->
        let r =
          match state.clientGarrisons.TryFind clientId with
          | None -> GetClientGarrisonReply.Empty
          | Some garrisonId ->
            match state.garrisons.TryFind garrisonId with
            | None -> GetClientGarrisonReply.Empty
            | Some garrison -> GetClientGarrisonReply.Success garrison
        (state, GetClientGarrisonReply r)

      | GetHero heroId ->
        match state.heroes.TryFind heroId with
        | None -> GetHeroReply.Empty
        | Some hero -> GetHeroReply.Success hero
        |> fun r -> (state, GetHeroReply r)

      | GetHeroArray heroIds ->
        heroIds |> Array.map (fun id ->
          match state.heroes.TryFind id with
          | None -> sprintf "Hero %s does not map to a hero" id |> failwith
          | Some hero -> hero
        )
        |> fun heroes -> (state, heroes |> GetHeroArrayReply.Success |> GetHeroArrayReply)

      | GetHeroInventory id ->
        match state.heroInventories.TryFind id with
        | None -> GetHeroInventoryReply.Empty
        | Some value -> GetHeroInventoryReply.Success value
        |> fun reply -> state, GetHeroInventoryReply reply

      | RemGarrison garrisonId ->
        let r = 
          match state.garrisons.TryFind garrisonId with
          | None -> RemGarrisonReply.Success
          | Some garrison ->
            // Remove all of the heroes in this garrison.
            let toRemove = garrison.stats.heroes |> Set
            let heroes = state.heroes |> Map.filter (fun id _ -> id |> toRemove.Contains |> not)

            let toRemoveNames =
              garrison.stats.heroes |> Array.collect (fun id ->
                match state.heroes.TryFind id with
                | None -> [||]
                | Some hero -> [| hero.name |]
              ) |> Set
            let heroNames = state.heroNames |> Set.filter (fun name ->
              name |> toRemoveNames.Contains |> not
            )

            let state = {
              state with
                garrisons = state.garrisons.Remove garrisonId
                heroes = heroes
                worlds = state.worlds.Remove garrison.worldId
                clientGarrisons = state.clientGarrisons.Remove garrison.clientId
                clientWorlds = state.clientWorlds.Remove garrison.clientId
                heroNames = heroNames
            }
            RemGarrisonReply.Success
        (state, RemGarrisonReply r)

      | SetStartingZone (race, zoneId) ->
        let raceId = race.ToString()

        match state.zones.ContainsKey zoneId with
        | false ->
          state, SetStartingZoneReply.ZoneDoesNotExist |> SetStartingZoneReply
        | true ->
          let state = {
            state with
              startingZones = state.startingZones.Add (raceId, zoneId)
          }
          state, SetStartingZoneReply.Success |> SetStartingZoneReply

      | Tick ->
        (state, TickReply)

    let agent = MailboxProcessor<Cmd>.Start(fun inbox ->
      log.Debug <| "Starting server..."

      let rec loop state = async {
        let! cmd = inbox.Receive()
        let msg, chan = cmd

        let state, reply =
          try
            log.Debug <| sprintf "Received \n\t%A" msg
            handleMsg msg state
          with e ->
            state, ExnReply e.Message

        log.Debug <| sprintf "Reply with %A" reply
        chan.Reply(reply)

        return! loop state
      }

      let gameState = GameState.Default

      loop gameState
    )

    let enc = Encoding.UTF8

    let handleWs (ws:WebSocket) = async {
      use ws = ws
      use tokSource = new CancellationTokenSource()
      let tok = tokSource.Token

      use inStream = new MemoryStream(1024)

      let rec receive () = async {
        let inSegment = ArraySegment(Array.zeroCreate 1024)
        let! n = ws.ReceiveAsync(inSegment, tok) |> Async.AwaitTask
        do! inStream.AsyncWrite inSegment.Array
        
        if n.EndOfMessage then
          let res = inStream.ToArray() |> enc.GetString
          inStream.Position <- 0L

          return (n, res.Substring(0, res.Length - (1024 - n.Count)))
        else
          return! receive()
      }

      let send msg frameType = async {
        let outBuffer = JsonConvert.SerializeObject(msg) |> Encoding.ASCII.GetBytes
        let outSegment = ArraySegment(outBuffer)
        //match frameType with
        //| WebSocketMessageType.Text ->
          //do! ws.SendAsync(outSegment, WebSocketMessageType.Text, true, CancellationToken.None) |> Async.AwaitIAsyncResult |> Async.Ignore
        //| WebSocketMessageType.Binary ->
        do! ws.SendAsync(outSegment, WebSocketMessageType.Binary, true, CancellationToken.None) |> Async.AwaitIAsyncResult |> Async.Ignore
      }

      let rec loop () = async {

        try
          log.Info "Receiving data..."
          let! (res, json) = receive()
          log.Info "Received."

          let asyncMsg = JsonConvert.DeserializeObject<AsyncMsg>(json)
          let msg = asyncMsg.msg

          log.Info "Deserialized msg"
          let! reply = agent.PostAndAsyncReply(fun reply -> (msg, reply))
          let reply = {
            reply = reply
            id = asyncMsg.id
          }
          log.Info <| sprintf "Got reply %A" reply
          let replyJson = JsonConvert.SerializeObject(reply)

          log.Info <| "Serialized reply"
          do! send replyJson res.MessageType
          log.Info <| sprintf "Sent %s" replyJson

          return! loop()
        with e ->
          log.Warn <| sprintf "Socket %A closed for %A" ws.State e
          do! ws.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, e.Message, tok) |> Async.AwaitTask
          return ()
      }

      do! loop()
      
      return ()
    }

    member this.Agent = agent

    interface IGameServer with
      member this.ApiHandler: Pario.WebServer.Handler = fun req resp -> async {
        let path = req.Url.AbsolutePath

        if path <> "/api" then
          return false
        else
        try
          // This is an addCharacterRequest.
          use input = req.InputStream
          use reader = new StreamReader(input)

          let! json = reader.ReadToEndAsync() |> Async.AwaitTask

          log.Debug <| sprintf "Got json %s" json

          let msg = JsonConvert.DeserializeObject<Msg>(json)

          if obj.ReferenceEquals(msg, null) then
            failwith "Unable to deserialize message"
            return true
          else
          // Now, issue the command to the server.
          let! res = agent.PostAndAsyncReply(fun reply -> (msg, reply))

          use resp = resp.OutputStream
          let json = JsonConvert.SerializeObject(res) |> enc.GetBytes
          do! resp.WriteAsync(json, 0, json.Length) |> Async.AwaitIAsyncResult |> Async.Ignore

          return true

        with e ->
          log.Debug <| sprintf "Error: %A" e
          resp.StatusCode <- 400
          resp.StatusDescription <- e.Message
          return true
      }

      member this.WebSocketHandler (context: Net.HttpListenerContext) = async {
      //do webServer.handleSocket <| fun context -> async {
        try
          let! wsCtx = context.AcceptWebSocketAsync(null) |> Async.AwaitTask

          let ws = wsCtx.WebSocket
          handleWs ws |> Async.Start

          return true
        with e ->
          log.Warn <| sprintf "Encountered a WebSocket error: %A" e
          return false
      }

  type Msg =
    | GetApiHandler
    | GetWsHandler

  type private GameServerBundle (log, id) =
    inherit IBundle()

    let server = GameServer(log)
    let agent = server.Agent
    let mutable context: IContext option = None

    override this.Id = id

    override this.Start _context =
      context <- Some _context

    override this.PreRestart (e, context) =
      log.Warn <| sprintf "Error: %A" e

    override this.Receive (msg, sender) =
      match msg with
      | :? Zrpg.Game.Msg as msg ->
        agent.PostAndAsyncReply(fun reply -> (msg, reply))
        |> Async.RunSynchronously
        |> fun reply ->
          match sender with
          | Some sender ->
            sender.Send reply
          | None -> ()
      | :? Msg as msg ->
        match msg with
        | GetApiHandler ->
          sender |> Option.iter (fun sender ->
            let handler = (server :> IGameServer).ApiHandler
            sender.Send handler
          )

        | GetWsHandler ->
          sender |> Option.iter (fun sender ->
            let handler = (server :> IGameServer).WebSocketHandler
            sender.Send handler
          )

  type GameServerChan(gameBundle:IBundleRef) =
    let fromChoice res =
      match res with
      | Choice1Of2 reply -> reply
      | Choice2Of2 e -> sprintf "Error: %A" e |> failwith

    let send msg = async {
      let chan = Chan<'a>()
      gameBundle.Send (msg, chan)
      let! res = chan.Await()
      let reply = fromChoice res
      return reply
    }

    member this.AddRegion addRegion = async {
      let! res = addRegion |> AddRegion |> send
      return
        match res with
        | AddRegionReply reply -> reply
        | reply -> sprintf "Expected AddRegion reply but got %A" reply |> failwith
    }

    member this.AddZone addZone = async {
      let! res = addZone |> AddZone |> send
      return
        match res with
        | AddZoneReply reply -> reply
        | reply -> sprintf "Expected AddZone reply but got %A" reply |> failwith
    }

    member this.SetStartingZone (race, zoneId) = async {
      let! res = SetStartingZone(race, zoneId) |> send
      return
        match res with
        | SetStartingZoneReply reply -> reply
        | reply -> sprintf "Expected SetStartingZone reply but got %A" reply |> failwith
    }

    member this.GetApiHandler (): Pario.WebServer.Handler Async = async {
      let! res = send GetApiHandler
      return res
    }

    member this.GetSocketHandler (): Pario.WebServer.WsHandler Async = async {
      let! res = send GetWsHandler
      return res
    }

    member this.AddRegionSync addRegion =
      this.AddRegion addRegion
      |> Async.RunSynchronously

  let server (platform:IPlatform) id =
    let log = LogBundle.log platform (id + ":log")
    match platform.Lookup id with
    | None ->
      let bundle = GameServerBundle (log, id)
      platform.Register bundle
      platform.Lookup id
      |> Option.get
      |> GameServerChan
    | Some game ->
      game
      |> GameServerChan