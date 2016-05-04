﻿// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System
open System.IO
open System.Text
open System.Diagnostics
open System.Reflection

open Zrpg
open Logging
open Pario

open Zrpg.Commons
open Zrpg.Commons.Bundle
open Zrpg.Game

let httpHost = "localhost"
let httpPort = 8080us
let httpEndPoint = sprintf "http://%s:%i" httpHost httpPort

[<EntryPoint>]
let main argv =
  let platform = Platform.create "Main"
  let web = WebServer.create platform "web"

  //log
  let game = Game.GameServer.server platform "Zrpg.GameServer"

  // Have the web server listen.
  async {
    let! reply = game.AddRegion ({ name = "Elwyn Forest" })
    let regionId =
      match reply with
      | AddRegionReply.Success id -> id
      | RegionExists -> failwith "Region exists"

    let! reply = game.AddZone {
      name = "Northshire"
      regionId = regionId
      terrain = Plains
    }
    let zoneId =
      match reply with
      | AddZoneReply.Success id -> id
      | _ -> failwith <| sprintf "Expected AddZoneReply.Success but got %A" reply

    let! reply = game.AddZone {
      name = "Goldshire"
      regionId = regionId
      terrain = Plains
    }
    let zoneId =
      match reply with
      | AddZoneReply.Success id -> id
      | _ -> failwith <| sprintf "Expected AddZoneReply.Success but got %A" reply

    let! reply = game.SetStartingZone (Human, zoneId)
    match reply with
    | SetStartingZoneReply.Success -> ()
    | _ -> failwith <| sprintf "Expected AddZoneReply.Success but got %A" reply

    let! handler = game.GetApiHandler()
    web.Send <| WebServer.AddHandler (handler, 100)

    let! handler = game.GetSocketHandler()
    web.Send <| WebServer.AddWsHandler handler
    
    let chan = Chan<WebServer.Reply>()
    WebServer.Listen (httpHost, httpPort) |> fun msg -> web.Send(msg, chan)
    let! res = chan.Await()
    match res with
    | Choice1Of2 reply -> printfn "Web reply = %s"
    | Choice2Of2 e -> raise e
  }
  |> Async.Catch
  |> Async.RunSynchronously
  |> printfn "Result = %A"

  // Run the REPL
  //let repl = CommandLine.create platform "repl"
  //repl.LoadConsole ()

  async {
    while true do
      do! Async.Sleep(1024)
      game
  } |> Async.RunSynchronously

  0
