// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System
open System.IO
open System.Text

open Zrpg
open Logging
open Pario

open Zrpg.Commons
open Zrpg.Commons.Bundle
open Zrpg.Game

let httpHost = "localhost"
let httpPort = 8080us
let httpEndPoint = sprintf "http://%s:%i" httpHost httpPort

let logStreamInner = new MemoryStream(1 <<< 20)
let logStream = new StreamReader(logStreamInner)

[<EntryPoint>]
let main argv =
  let log = new StreamLogger("main", LogLevel.Debug, logStreamInner)
  let platform = Platform.createPlatform()

  let repl = platform.Lookup "REPL" |> Option.get
  repl.Send <| CommandLine.LoadConsole

  let web = WebServer.create platform "web"

  //log
  let game = Game.GameServer.server "Zrpg.GameServer"
  game.AddRegion ({ name = "bob" })
  |> Async.RunSynchronously
  |> printfn "Result = %A"

  async {
    while true do
      do! Async.Sleep(Int32.MaxValue)
  } |> Async.RunSynchronously

  // Add the game's API to the server.
  //do server.handle {
  //  priority = 100
  //  handler = game.ApiHandler
  //}

  //server.listen httpHost httpPort |> Async.RunSynchronously

  //Main.server.listen "localhost" 8080us |> Async.RunSynchronously
  0
