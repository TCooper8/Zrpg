namespace Zrpg.Game

open System
open System.IO
open System.Text
open Newtonsoft.Json
open System.Diagnostics

module GameServer =
  type AddCharacter = {
    clientId:  string
    name:      string
    race:      Race
    gender:    Gender
    classType: ClassType
  }

  type ChainEvent = {
    id: string
    events: Event list
  }
  and EventNode = string
  and EventBranch = {
    id: string
    activation: Stats option
    events: Event list
  }
  and Event =
    | ChainEvent of ChainEvent
    | EventNode of EventNode
    | EventBranch of EventBranch

  type Msg =
    | AddCharacter of AddCharacter
    | AddRegion of Region
    | AddZone of string * Zone
    | SetStartingZone of Race * string

  type Reply =
    | EmptyReply
    | MsgReply of string
    | ExnReply of string

  type Cmd = Msg * AsyncReplyChannel<Choice<Reply, exn>>

  type GameServer () =
    let log = Logging.FileLogger("GameServer", Logging.LogLevel.Debug, "out.log")
    let gameState = GameState()
    let game = GameRunner.Game(gameState)
    let webServer = Pario.WebServer.Server()

    do
      for i in 0 .. 100 do
        log.Info <| sprintf "%i" i

    let agent = MailboxProcessor<Cmd>.Start(fun inbox ->
      log.Info <| "Starting server..."

      let rec loop () = async {
        let! cmd = inbox.Receive()
        let (msg, reply) = cmd

        log.Info <| sprintf "Received %A" msg

        match msg with
        | AddCharacter char ->
          let res =
            try
              gameState.AddCharacter
              <| char.clientId
              <| char.name
              <| char.race
              <| char.gender
              <| char.classType
              MsgReply "Added character" |> Choice1Of2
            with e -> Choice2Of2 e
          reply.Reply(res)

        | AddRegion region ->
          let res =
            try
              gameState.AddRegion region
              MsgReply "Added region" |> Choice1Of2
            with e -> Choice2Of2 e
          reply.Reply(res)

        | AddZone(regionId, zone) ->
          let res =
            try
              gameState.AddZone regionId zone
              MsgReply "Added zone" |> Choice1Of2
            with e -> Choice2Of2 e
          reply.Reply(res)

        | SetStartingZone(race, zoneId) ->
          let res =
            try
              gameState.SetStartingZone race zoneId
              MsgReply "Set starting zone" |> Choice1Of2
            with e -> Choice2Of2 e
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

          log.Info <| sprintf "Got json %s" json

          let msg = JsonConvert.DeserializeObject<Msg>(json)

          // Now, issue the command to the server.
          let! res = agent.PostAndAsyncReply(fun reply -> (msg, reply))
          let reply = match res with
          | Choice2Of2 e -> ExnReply e.Message
          | Choice1Of2 reply -> reply

          use resp = resp.OutputStream
          let json = JsonConvert.SerializeObject(reply) |> enc.GetBytes
          do! resp.WriteAsync(json, 0, json.Length) |> Async.AwaitIAsyncResult |> Async.Ignore

          return true

        with e ->
          resp.StatusCode <- 400
          resp.StatusDescription <- e.Message
          return true
      }
    }

    member this.Listen host port =
      webServer.listen host port