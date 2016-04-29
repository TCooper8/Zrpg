// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System
open System.IO
open System.Text
open System.Diagnostics

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

type Msg =
  | Report

type TestBundle () =
  inherit IBundle()

  let mutable i = 0

  override this.Id = "Test"

  override this.Receive (msg, sender) =
    i <- i + 1
    printfn "%i" i
    match msg with
    | :? Msg as msg ->
      printfn "%i" i
    | _ -> ()

[<EntryPoint>]
let main argv =
  let log = new StreamLogger("main", LogLevel.Debug, logStreamInner)
  let platform = Platform.create "Main"

  //let repl = platform.Lookup "REPL" |> Option.get
  //repl.Send <| CommandLine.LoadConsole

  let bundle = TestBundle()
  platform.Register bundle
  let test = platform.Lookup bundle.Id |> Option.get

  let msgs = 1000
  let runners = 4

  let watch = new Stopwatch()
  watch.Start()
  [ for i in 1 .. runners do
    yield
      async {
        for i in 1 .. msgs do
          test.Send 1
      }
  ]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore

  watch.Stop()

  let msgs = msgs * runners |> float
  let dt = watch.Elapsed.TotalSeconds
  let mps = msgs / dt
  printfn "%f mps" mps

  let web = WebServer.create platform "web"

  //log
  let game = Game.GameServer.server platform "Zrpg.GameServer"
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
