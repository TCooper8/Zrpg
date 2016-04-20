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

  type Cmd = Msg * AsyncReplyChannel<Reply>

  [<Interface>]
  type IGameServer =
    abstract ApiHandler: Pario.WebServer.Handler
    abstract WebSocketHandler: Net.HttpListenerContext -> bool Async

  type private GameServer (log:Logger) =
    let gameState = GameState()
    let game = GameRunner.Game(gameState, log)
    let webServer = Pario.WebServer.Server(log)
    let discovery = Zrpg.Discovery.createLocal() |> Async.RunSynchronously

    let agent = MailboxProcessor<Cmd>.Start(fun inbox ->
      log.Debug <| "Starting server..."

      let rec loop () = async {
        let! cmd = inbox.Receive()
        let msg, reply = cmd

        let res: Reply =
          try
            log.Debug <| sprintf "Received \n\t%A" msg
            match msg with
            | AddGarrison msg ->
              log.Debug <| sprintf "Adding garrison %s" msg.name

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

              gameState.addGarrison garrison

            | GetClientGarrison clientId ->
              gameState.getClientGarrison clientId

            | GetGarrison id ->
              gameState.getGarrison id

            | RemGarrison id ->
              gameState.remGarrison id
          with e ->
            ExnReply e.Message

        log.Debug <| sprintf "Reply with %A" res
        reply.Reply(res)

        return! loop ()
      }
      loop()
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
          log.Warn("Encountered a WebSocket error: {0}", e)
          return false
      }

  let server log =
    GameServer(log) :> IGameServer