namespace Zrpg.Game

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Diagnostics
open System.IO
open System.Text
open System.Net
open System.Threading.Tasks
open Newtonsoft.Json

open Zrpg.Game.GameServer
open Zrpg.Game
open Logging

[<TestClass>]
type Test () =
  let sync () = Async.AwaitTask >> Async.RunSynchronously
  let uuid () = Guid.NewGuid().ToString()

  static let port = (new Random()).Next(1 <<< 10, 1 <<< 15) |> uint16
  static let endPoint = sprintf "http://localhost:%i" port

  static member log = new Logging.StreamLogger("Test", LogLevel.Debug, Console.OpenStandardOutput())
  static member server = new Pario.WebServer.Server(Test.log)
  static member client = GameClient.RESTClient endPoint

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

      let! res = server.listen "localhost" port |> Async.Catch
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
    let reply = Test.client.AddGarrison(uuid(), "My garrison", Human, Alliance) |> sync()
    match reply with
    | AddGarrisonReply.Success -> ()
    | AddGarrisonReply.ClientHasGarrison -> failwith "Client should not have a garrison at this point"

  [<TestMethod>]
  member this.testClientFirstGarrison () =
    let clientId = uuid()

    Test.client.GetClientGarrison(clientId) |> sync()
    |> fun m ->
      match m with
      | GetClientGarrisonReply.Empty -> ()
      | reply ->
        failwith <| sprintf "Expected client not to have a garrison, but got %A" reply

    // No garrison, let's create one.
    Test.client.AddGarrison(clientId, "My garrison", Human, Alliance) |> sync()
    |> fun m ->
      match m with
      | AddGarrisonReply.Success ->
        () // good.
      | reply ->
        failwith <| sprintf "Expected AddGarrisonReply of GarrisonAdded but got %A" reply

    match Test.client.GetClientGarrison(clientId) |> sync() with
    | GetClientGarrisonReply.Success garrison ->
      () // Got the garrison, good.
    | reply ->
      failwith <| sprintf "Expceted GetClientGarrisonReply but got %A" reply

  [<TestMethod>]
  member this.testClientHasGarrison () =
    let clientId = uuid()
    match Test.client.AddGarrison(clientId, "My garrison", Human, Alliance) |> sync() with
    | AddGarrisonReply.Success -> ()
    | msg -> failwith <| sprintf "Expected AddGarrisonReply with id, but got %A" msg

    match Test.client.GetClientGarrison(clientId) |> sync() with
    | GetClientGarrisonReply.Success garrison -> ()
    | msg -> failwith <| sprintf "Expected GetClientGarrisonReply with id, but got %A" msg

    // Got the garrison
    ()

  [<TestMethod>]
  member this.testAddHero () =
    let clientId = uuid()

    match Test.client.AddGarrison(clientId, "My garrison", Human, Alliance) |> sync() with
    | AddGarrisonReply.Success -> ()
    | msg -> failwith <| sprintf "Expected Success but got %A" msg

    let msg:AddHero = { 
      clientId = clientId
      name = "bob"
      race = Human
      faction = Alliance
      gender = Male
      heroClass = Warrior
    }

    let heroId =
      match Test.client.AddHero msg |> sync() with
      | AddHeroReply.Success id -> id
      | msg -> failwith <| sprintf "Expected Success but got %A" msg

    let garrison =
      match Test.client.GetClientGarrison clientId |> sync() with
      | GetClientGarrisonReply.Success garrison -> garrison
      | msg -> failwith <| sprintf "Expected Success but got %A" msg

    // Check and make sure the hero.id is in the garrison stats.
    let heroes = garrison.stats.heroes
    heroes |> Array.tryFind (fun id -> id = heroId)
    |> fun res ->
      match res with
      | None -> failwith "Hero was not in the garrison heroes"
      | _ -> ()