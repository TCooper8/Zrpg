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
          xp = 0.0
          finalXp = 1000.0
          groundTravelSpeed = 100.0
        }

    let raceStatMod race =
      match race with
      | Human ->
        { strength = 1.0
          stamina = 1.1
          xp = 0.0
          finalXp = 1.0
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
          ownedRegions = [| startingZone.regionId |]
          ownedZones = [| startingZone.id |]
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

        let hero:Hero = {
          id = uuid()
          clientId = msg.clientId
          state = Idle
          zoneId = startingZoneId
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
          let panes =
            [ for i in 0 .. 2 do
                let slots =
                  [
                    for i in 0 .. 9 do
                      let slot = {
                        position = i
                        itemRecordId = GameNone
                      }
                      yield slot
                  ] |> Array.ofList

                let pane = {
                  position = i
                  slots = slots
                }
                yield pane
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

    let heroBeginQuest (state:GameState) (heroId:string) (questId:string) =
      let mutable state = state
      match state.heroes.TryFind heroId with
      | None ->
        state, HeroBeginQuestReply.HeroDoesNotExist
      | Some hero when hero.state = HeroState.Questing ->
        state, HeroBeginQuestReply.HeroIsQuesting
      | Some hero when hero.state = HeroState.Idle ->
        // Make sure the hero is in the same zone as the quest.
        //match state.zoneQuests.TryFind hero.zoneId with
        //| None -> sprintf "Hero zone %s does not have any quests" hero.zoneId |> failwith
        //| Some quests ->
        //  // Make sure the quest is in this array.
        //  quests |> Array.tryFind questId.Equals |> fun m ->
        //    match m with
        //    | None -> sprintf "Zone %s does not contain quest %s" hero.zoneId questId |> failwith
        //    | _ -> ()

        // Get the quest from the id.
        match state.quests.TryFind questId with
        | None ->
          state, HeroBeginQuestReply.QuestDoesNotExist
        | Some quest ->
          // Create a record for the quest and set the hero state.
          let record = {
            QuestRecord.id = uuid()
            questId = questId
            startTime = state.gameTime
          }

          let hero = {
            hero with
              state = HeroState.Questing
              zoneId = quest.zoneId // Move the hero.
          }
          let state = {
            state with
              questRecords = state.questRecords.Add(record.id, record)
              heroes = state.heroes.Add(heroId, hero)
              heroQuestRecords = state.heroQuestRecords.Add(heroId, record.id)
          }
          state, HeroBeginQuestReply.Success record.id

    let resolveTick (state:GameState) =
      // Go through all of the heroes and run them for one cycle.
      let mutable state = state

      let heroes =
        state.heroes |> Map.map (fun id hero ->
          log.Debug <| sprintf "Resolving quest for hero %s" hero.id

          state.heroQuestRecords.TryFind id
          |> Option.map (fun id ->
            log.Debug <| sprintf "Hero %s has quest record id %s" hero.id id
            id
          )
          |> Option.bind state.questRecords.TryFind
          |> Option.map (fun record ->
            log.Debug <| sprintf "Hero %s has quest record %A" hero.id record

            match state.quests.TryFind record.questId with
            | None -> failwith <| sprintf "Hero quest record %s does not link to an actual quest." record.id
            | Some quest ->
              record, quest
          )
          |> Option.map (fun (record, quest) ->
            log.Debug <| sprintf "Hero record %A quest %A" record quest
            match quest.objective with
            | TimeObjective objective ->
              let tf = record.startTime + objective.timeDelta
              let dt = tf - state.gameTime
              log.Debug <| sprintf "Hero %s has %i ticks until completing quest %s" hero.id dt quest.id

              if tf <= state.gameTime then
                // Finish the quest.
                log.Debug <| sprintf "Hero %s finished quest %A" hero.id quest
                state <- state.CompleteQuest hero record quest
                log.Debug <| "Updated "
              // Quest is not finished
            ()
          )
        )

      state <- {
        state with
          gameTime = state.gameTime + 1
      }

      state, TickReply

    let handleMsg msg state =
      match msg with
      | AddItem info ->
        let item = {
          id = uuid()
          info = info
        }
        let state = {
          state with
            items = state.items.Add(item.id, item)
        }
        state, item.id |> AddItemReply

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
                regions = state.regions.Add(region.id, region)
            }
            state, zone.id |> AddZoneReply.Success |> AddZoneReply

      | AddZoneAssetPositionInfo info ->
        let state = {
          state with
            zoneAssetPositionInfo = state.zoneAssetPositionInfo.Add(info.id, info)
        }
        state, AddZoneAssetPositionInfoReply

      | AddQuest addQuest ->
        // Make sure the zone exists.
        match state.zones.TryFind addQuest.zoneId with
        | None -> sprintf "Zone %s does not exist" addQuest.zoneId |> failwith
        | _ -> ()

        let quest = {
          id = uuid()
          zoneId = addQuest.zoneId
          title = addQuest.title
          body = addQuest.body
          rewards = addQuest.rewards |> List.ofArray
          objective = addQuest.objective
        }

        let zoneQuests =
          state.zoneQuests.TryFind quest.zoneId
          |> defaultArg <| Array.empty
          |> Array.append [| quest.id |]

        let state = {
          state with
            quests = state.quests.Add(quest.id, quest)
            zoneQuests = state.zoneQuests.Add(quest.zoneId, zoneQuests)
        }
        state, AddQuestReply.Success quest.id |> AddQuestReply

      | GetClientGarrison clientId ->
        let r =
          match state.clientGarrisons.TryFind clientId with
          | None -> GetClientGarrisonReply.Empty
          | Some garrisonId ->
            match state.garrisons.TryFind garrisonId with
            | None -> GetClientGarrisonReply.Empty
            | Some garrison -> GetClientGarrisonReply.Success garrison
        (state, GetClientGarrisonReply r)

      | GetGameTime ->
        state, state.gameTime |> GetGameTimeReply

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

      | GetHeroQuest heroId ->
        let res r = GetHeroQuestReply r
        match state.heroQuestRecords.TryFind heroId with
        | None -> state, GetHeroQuestReply.Empty |> res
        | Some recordId ->
          match state.questRecords.TryFind recordId with
          | None -> failwith <| sprintf "Hero %s has record id %s, but no quest record exists" heroId recordId
          | Some record ->
            // Now, try and get the quest.
            match state.quests.TryFind record.questId with
            | None -> failwith <| sprintf "Hero %s has record %s, but quest %s does not exist" heroId recordId record.questId
            | Some quest ->
              state, GetHeroQuestReply.Success(record, quest) |> res

      | GetItem recordId ->
        match state.itemRecords.TryFind recordId with
        | None -> failwith "Item record does not exist"
        | Some record ->
          match state.items.TryFind record.itemId with
          | None -> failwith "Item record found, but item does not exist"
          | Some item ->
            state, GetItemReply(record, item)

      | GetRegion regionId ->
        match state.regions.TryFind regionId with
        | None -> sprintf "Region %s does not exist" regionId |> failwith
        | Some region ->
          state, GetRegionReply.Success region |> GetRegionReply

      | GetZone zoneId ->
        match state.zones.TryFind zoneId with
        | None -> sprintf "Zone %s does not eixst" zoneId |> failwith
        | Some zone ->
          state, GetZoneReply.Success zone |> GetZoneReply

      | GetZoneAssetPositionInfo zoneId ->
        match state.zoneAssetPositionInfo.TryFind zoneId with
        | None -> failwith "Zone asset info does not exist"
        | Some info ->
          state, info |> GetZoneAssetPositionInfoReply

      | GetZoneQuests zoneId ->
        match state.zoneQuests.TryFind zoneId with
        | None -> state, Array.empty |> GetZoneQuestsReply
        | Some ids ->
          ids |> Array.map (fun id ->
            match state.quests.TryFind id with
            | None -> sprintf "Zone contains unmapped quest id %s" id |> failwith
            | Some quest -> quest
          )
          |> GetZoneQuestsReply
          |> fun r -> state, r

      | HeroBeginQuest (heroId, questId) ->
        heroBeginQuest state heroId questId
        |> fun (state, reply) -> state, reply |> HeroBeginQuestReply

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
        resolveTick state

    let agent = MailboxProcessor<Cmd>.Start(fun inbox ->
      log.Debug <| "Starting server..."

      let rec loop state = async {
        try
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

          //do! Async.Sleep(512)
          return! loop state
        with e ->
          log.Error <| sprintf "Agent error: %A" e
          do! Async.Sleep(5000)
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
      printfn "Error: %A" e
      log.Warn <| sprintf "Error: %A" e

    override this.Receive (msg, sender) =
      printfn "Received msg %A" msg
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
      printfn "Sending msg %A" msg
      let chan = Chan<'a>()
      gameBundle.Send (msg, chan)
      printfn "Waiting for reply..."
      let! res = chan.Await()
      printfn "Reply = %A" res
      let reply = fromChoice res
      return reply
    }

    member this.AddItem itemInfo = async {
      let! res = itemInfo |> AddItem |> send
      return
        match res with
        | AddItemReply itemId -> itemId
        | reply -> sprintf "Expected AddItem reply but got %A" reply |> failwith
    }

    member this.AddRegion addRegion = async {
      let! res = addRegion |> AddRegion |> send
      return
        match res with
        | AddRegionReply reply -> reply
        | reply -> sprintf "Expected AddRegion reply but got %A" reply |> failwith
    }
    member this.AddRegionSync addRegion =
      this.AddRegion addRegion
      |> Async.RunSynchronously

    member this.AddQuest addQuest = async {
      let! res = addQuest |> AddQuest |> send
      return
        match res with
        | AddQuestReply reply -> reply
        | reply -> sprintf "Expected AddZone reply but got %A" reply |> failwith
    }

    member this.AddZone addZone = async {
      let! res = addZone |> AddZone |> send
      return
        match res with
        | AddZoneReply reply -> reply
        | reply -> sprintf "Expected AddZone reply but got %A" reply |> failwith
    }

    member this.AddZoneAssetPositionInfo info = async {
      let! res = info |> AddZoneAssetPositionInfo |> send
      return
        match res with
        | AddZoneAssetPositionInfoReply -> ()
        | reply -> sprintf "Expected AddZoneAssetPositionInfo reply but got %A" reply |> failwith
    }

    member this.GetZoneAssetPositionInfo zoneId = async {
      let! res = zoneId |> GetZoneAssetPositionInfo |> send
      return
        match res with
        | GetZoneAssetPositionInfoReply info -> info
        | reply -> sprintf "Expected GetZoneAssetPositionInfoReply but got %A" reply |> failwith
    }

    member this.SetStartingZone (race, zoneId) = async {
      let! res = SetStartingZone(race, zoneId) |> send
      return
        match res with
        | SetStartingZoneReply reply -> reply
        | reply -> sprintf "Expected SetStartingZone reply but got %A" reply |> failwith
    }

    member this.GetApiHandler (): Pario.WebServer.Handler Async = async {
      printfn "Getting api handler..."
      let! res = send GetApiHandler
      return res
    }

    member this.GetSocketHandler (): Pario.WebServer.WsHandler Async = async {
      let! res = send GetWsHandler
      return res
    }

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