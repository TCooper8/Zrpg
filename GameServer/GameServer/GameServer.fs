namespace Zrpg.Game

open System
open System.IO
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
    | AddCharacterMsg of AddCharacter * AsyncReplyChannel<exn option>

  type GameServer () =
    let gameState = GameState()
    let game = GameRunner.Game(gameState)
    let webServer = Pario.WebServer.Server()

    let agent = MailboxProcessor.Start(fun inbox ->
      let rec loop () = async {
        let! msg = inbox.Receive()
        match msg with
        | AddCharacterMsg(cmd, reply) ->
          let res =
            try
              gameState.AddCharacter
              <| cmd.clientId
              <| cmd.name
              <| cmd.race
              <| cmd.gender
              <| cmd.classType
              None
            with e -> Some e
          reply.Reply(res)
      }
      loop()
    )

    do webServer.handle {
      priority = 100
      handler = fun req resp -> async {
        let path = req.Url.AbsolutePath

        if path <> "/addCharacter" then
          return false
        else
        try
          // This is an addCharacterRequest.
          use input = req.InputStream
          use reader = new StreamReader(input)

          let! json = reader.ReadToEndAsync() |> Async.AwaitTask
          let cmd = JsonConvert.DeserializeObject<AddCharacter>(json)

          // Now, issue the command to the server.
          let! res = agent.PostAndAsyncReply(fun reply -> AddCharacterMsg(cmd, reply))
          match res with
          | Some e ->
            Debug.WriteLine(sprintf "%A" e)
            resp.StatusCode <- 500
            resp.StatusDescription <- e.Message
            return true
          | None ->
            // The add was successful.
            use resp = resp.OutputStream
            let msg = "Character created!"B
            do! resp.WriteAsync(msg, 0, msg.Length) |> Async.AwaitIAsyncResult |> Async.Ignore

            return true

        with e ->
          resp.StatusCode <- 400
          resp.StatusDescription <- e.Message
          return true
      }
    }

    member this.Listen host port =
      webServer.listen host port