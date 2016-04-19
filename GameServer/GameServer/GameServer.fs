namespace Zrpg.Game

open System
open System.IO
open System.Text
open System.Net.WebSockets
open System.Threading
open Newtonsoft.Json
open System.Diagnostics

open Logging

module GameServer =
  let uuid () = Guid.NewGuid().ToString()

  type Token = string

  type Msg =
    | AddGarrison of AddGarrison
  and AddGarrison = {
    clientId: string
    name: string
    race: Race
    faction: Faction
  }

  type Reply =
    | EmptyReply
    | MsgReply of string
    | ExnReply of exn

  type Cmd = Msg * AsyncReplyChannel<Reply>

  type GameServer () =
    let log = StreamLogger(
      "GameServer",
      LogLevel.Debug,
      Console.OpenStandardOutput()
    )

    let gameState = GameState()
    let game = GameRunner.Game(gameState, log)
    let webServer = Pario.WebServer.Server(log)
    let discovery = Zrpg.Discovery.createLocal() |> Async.RunSynchronously

    let agent = MailboxProcessor<Cmd>.Start(fun inbox ->
      log.Debug <| "Starting server..."

      let rec loop () = async {
        let! cmd = inbox.Receive()
        let msg, reply = cmd

        log.Debug <| sprintf "Received \n\t%A" cmd

        match msg with
        | AddGarrison msg ->
          let stats = {
            goldIncome = 10
            heroes = []
          }

          let garrison = {
            id = uuid()
            clientId = "tmpClientId"
            name = msg.name
            race = msg.race
            faction = msg.faction
            ownedRegions = Set []
            ownedZones = Set []
            vaultId = ""
            stats = stats
          }

          let res = match gameState.addGarrison garrison with
          | Failure e -> ExnReply e
          | Success () -> MsgReply "Garrison created!"

          reply.Reply(res)

        return! loop ()
      }
      loop()
    )

    let enc = Encoding.UTF8

    do webServer.handle {
      priority = 100
      handler = fun req resp -> async {
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

          // Now, issue the command to the server.
          let! res = agent.PostAndAsyncReply(fun reply -> (msg, reply))

          use resp = resp.OutputStream
          let json = JsonConvert.SerializeObject(res) |> enc.GetBytes
          do! resp.WriteAsync(json, 0, json.Length) |> Async.AwaitIAsyncResult |> Async.Ignore

          return true

        with e ->
          resp.StatusCode <- 400
          resp.StatusDescription <- e.Message
          return true
      }
    }

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



          return res.Substring(0, res.Length - (1024 - n.Count))
        else
          return! receive()
      }

      let send msg = async {
        let outBuffer = JsonConvert.SerializeObject(msg) |> enc.GetBytes
        let outSegment = ArraySegment(outBuffer)
        do! ws.SendAsync(outSegment, WebSocketMessageType.Text, true, tok) |> Async.AwaitIAsyncResult |> Async.Ignore
      }

      let rec loop () = async {
        let! msg = receive()
        log.Info <| sprintf ">>> %s END" msg

        do! send "Hello"

        return! loop()
      }

      do! loop()
      
      return ()
    }

    do webServer.handleSocket <| fun context -> async {
      try
        let! wsCtx = context.AcceptWebSocketAsync(null) |> Async.AwaitTask

        let ws = wsCtx.WebSocket
        handleWs ws |> Async.Start

        return true
      with e ->
        log.Warn("Encountered a WebSocket error: {0}", e)
        return false
    }

    let createServer (log:Logger) = async {
    }

    member this.Listen host port = async {
      let! res = discovery.addServiceHost "gameserver" {
        ipAddress = host
        port = port
        hostType = "http"
        status = "good"
      }

      Option.iter (fun e -> raise e) res

      do! webServer.listen host port
    }