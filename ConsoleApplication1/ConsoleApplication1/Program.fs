// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System
open System.IO
open System.Text

open Zrpg
open Logging
open Pario

open Newtonsoft.Json

module Main =
  let log = new StreamLogger("main", LogLevel.Debug, Console.OpenStandardOutput())
  let server = WebServer.Server <| log.Fork("Web", log.Level)
  let enc = Encoding.UTF8

  let gameServer = Zrpg.Game.GameServer.server log

  do server.handle {
    priority = 100
    handler = gameServer.ApiHandler
  }

[<EntryPoint>]
let main argv =
  Main.server.listen "localhost" 8080us |> Async.RunSynchronously
  0
