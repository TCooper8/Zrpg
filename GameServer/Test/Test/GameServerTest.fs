namespace Zrpg.Game

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Diagnostics
open System.IO
open System.Text
open System.Net
open Newtonsoft.Json

open Zrpg.Game.GameServer
open Zrpg.Game
open Logging

[<TestClass>]
type Test () =
  let sync = Async.AwaitTask >> Async.RunSynchronously
  let uuid () = Guid.NewGuid().ToString()

  static member log = new Logging.StreamLogger("Test", LogLevel.Debug, Console.OpenStandardOutput())
  static member server = new Pario.WebServer.Server(Test.log)
  static member client = GameClient.RESTClient "http://localhost:8080"

  [<ClassInitialize>]
  static member init (ctx:TestContext) =
    async {
      let server = Test.server
      let log = Test.log
      let gameServer = GameServer.server log

      do server.handle {
        priority = 100
        handler = gameServer.ApiHandler
      }

      do server.handleSocket gameServer.WebSocketHandler

      let! res = server.listen "localhost" 8080us |> Async.Catch
      match res with
      | Choice2Of2 e ->
        Debug.WriteLine(sprintf "%A" e)
      | _ -> ()
    } |> Async.Start

  [<ClassCleanup>]
  static member cleanup () =
    Test.server.stop()

  [<TestMethod>]
  member this.testNewGarrison () =
    let reply = Test.client.AddGarrison(uuid(), "My garrison", Human, Alliance) |> sync
    match reply with
    | Success -> failwith e
    | _ -> ()

  [<TestMethod>]
  member this.testClientFirstGarrison () =
    let clientId = uuid()

    Test.client.GetClientGarrison(clientId) |> sync
    |> fun m ->
      match m with
      | EmptyReply -> () // good.
      | reply ->
        failwith <| sprintf "Expected client not to have a garrison, but got %A" reply

    // No garrison, let's create one.
    Test.client.AddGarrison(clientId, "My garrison", Human, Alliance) |> sync
    |> fun m ->
      match m with
      | AddGarrisonReply GarrisonAdded ->
        () // good.
      | reply ->
        failwith <| sprintf "Expected AddGarrisonReply of GarrisonAdded but got %A" reply

    match Test.client.GetClientGarrison(clientId) |> sync with
    | GetClientGarrisonReply id ->
      match Test.client.GetGarrison id |> sync with
      | GetGarrisonReply garrison ->
        () // Got the garrison, good.
      | reply ->
        failwith <| sprintf "Expceted GetGarrisonReply but got %A" reply
    | reply ->
      failwith <| sprintf "Expceted GetClientGarrisonReply but got %A" reply

  [<TestMethod>]
  member this.testClientHasGarrison () =
    let clientId = uuid()
    match Test.client.AddGarrison(clientId, "My garrison", Human, Alliance) |> sync with
    | AddGarrisonReply id -> ()
    | msg -> failwith <| sprintf "Expected AddGarrisonReply with id, but got %A" msg

    let garrisonId =
      match Test.client.GetClientGarrison(clientId) |> sync with
      | GetClientGarrisonReply id -> id
      | msg -> failwith <| sprintf "Expected GetClientGarrisonReply with id, but got %A" msg

    let garrison =
      match Test.client.GetGarrison garrisonId |> sync with
      | GetGarrisonReply garrison -> garrison
      | msg -> failwith <| sprintf "Expected GetClientGarrisonReply with id, but got %A" msg

    // Got the garrison
    ()