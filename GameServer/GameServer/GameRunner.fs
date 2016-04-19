namespace Zrpg.Game

open System
open Logging

module GameRunner =
  let uuid () = Guid().ToString()

  type Msg =
    | Tick
    | Kill

  type Game (state, log:Logger) =
    let rand = Random()
    let state:GameState = state

    // This will advance the state of the game.
    let rec handleTick () =
      ()

    let agent = MailboxProcessor<Msg>.Start(fun inbox ->
      let rec loop () = async {
        let! msg = inbox.Receive()

        try
          match msg with
          | Tick ->
            handleTick ()
          | Kill ->
            return () // Will stop the task
        with e ->
          printfn "%A" e

        return! loop ()
      }
      loop()
    )

    member this.Stop () =
      agent.Post Kill

    member this.Tick () =
      agent.Post Tick

