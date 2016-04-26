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

  member this.addAnyRegion () =
    let id = uuid()
    Test.client.AddRegion {
      name = id
    }
    |> sync()
    |> fun m -> 
      match m with
      | AddRegionReply.Success id -> id
      | reply ->
        failwith <| sprintf "Expected AddRegionReply.Success but got %A" reply

  member this.addAnyZone regionId =
    let id = uuid()
    Test.client.AddZone {
      name = id
      regionId = regionId
      terrain = Forest
    }
    |> sync()
    |> fun m -> 
      match m with
      | AddZoneReply.Success id -> id
      | reply ->
        failwith <| sprintf "Expected AddZoneReply.Success but got %A" reply

  member this.setStartingZone (race, zoneId) =
    Test.client.SetStartingZone(race, zoneId)
    |> sync()
    |> fun m ->
      match m with
      | SetStartingZoneReply.Success -> ()
      | reply ->
        failwith <| sprintf "Expected SetStartingZone.Success but got %A" reply

  member this.addAnyGarrison clientId race faction =
    this.addAnyRegion ()
    |> this.addAnyZone
    |> fun zoneId -> this.setStartingZone(race, zoneId)

    Test.client.AddGarrison(
      clientId,
      uuid(),
      race,
      faction
    )
    |> sync()
    |> fun reply ->
      match reply with
      | AddGarrisonReply.Success -> ()
      | reply -> sprintf "Expected AddGarrisonReply.Success but got %A" reply |> failwith

  member this.getClientGarrison clientId =
    Test.client.GetClientGarrison(clientId)
    |> sync()
    |> fun reply ->
      match reply with
      | GetClientGarrisonReply.Success garrison -> garrison
      | reply -> sprintf "Expected GetClientGarrisonReply.Success but got %A" reply |> failwith
      

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
  member this.testNewGarrisonN () =
    for i in 0 .. 10 do
      uuid() |> this.addAnyGarrison
      <| Human
      <| Alliance

  [<TestMethod>]
  member this.testNewRegion () =
    this.addAnyRegion () |> ignore

  [<TestMethod>]
  member this.TestNewZone () =
    this.addAnyRegion ()
    |> this.addAnyZone
    |> ignore

  [<TestMethod>]
  member this.testNewGarrison () =
    uuid() |> this.addAnyGarrison

  [<TestMethod>]
  member this.testClientFirstGarrison () =
    let clientId = uuid()

    try clientId |> this.getClientGarrison |> Some
    with e -> None
    |> Option.iter (fun garrison ->
      sprintf "Client should not have a garrison but got %A" garrison |> failwith
    )

    // No garrison, let's create one.
    this.addAnyGarrison clientId Human Alliance
    this.getClientGarrison clientId
    |> ignore

  [<TestMethod>]
  member this.testClientHasGarrison () =
    let clientId = uuid()

    this.addAnyGarrison clientId Human Alliance
    this.getClientGarrison clientId
    |> ignore

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