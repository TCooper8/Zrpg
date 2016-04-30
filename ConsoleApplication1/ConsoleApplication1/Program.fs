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

[<EntryPoint>]
let main argv =
  let platform = Platform.create "Main"

  let web = WebServer.create platform "web"

  //log
  let game = Game.GameServer.server platform "Zrpg.GameServer"
  game.AddRegion ({ name = "bob" })
  |> Async.RunSynchronously
  |> printfn "Result = %A"

  // Have the web server listen.
  async {
    let! handler = game.GetApiHandler()
    web.Send <| WebServer.AddHandler (handler, 100)
    
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

  async {
    while true do
      do! Async.Sleep(Int32.MaxValue)
  } |> Async.RunSynchronously

  0
