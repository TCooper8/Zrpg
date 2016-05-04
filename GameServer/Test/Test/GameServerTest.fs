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
open Zrpg.Commons
open Zrpg.Commons.Bundle
open Logging

[<TestClass>]
type TestClass () =
  let sync () = Async.AwaitTask >> Async.RunSynchronously
  let uuid () = Guid.NewGuid().ToString()

  let testId = "test"
  let httpHost = "localhost"
  let httpPort = (new Random()).Next(1 <<< 10, 1 <<< 15) |> uint16
  let httpEndPoint = sprintf "http://%s:%i" httpHost httpPort

  let gameId = testId + "game"
  let client = GameClient.RESTClient httpEndPoint
  let platform = Platform.create testId
  let game = GameServer.server platform gameId

  [<TestInitialize>]
  member this.Init () =
    let web = Pario.WebServer.create platform (testId + "web")

    async {
      printfn "Getting api handler..."
      try
        let! handler = game.GetApiHandler()
        printfn "Handler = %A" handler
        web.Send <| Pario.WebServer.AddHandler (handler, 100)

        let chan = Chan<Pario.WebServer.Reply>()
        Pario.WebServer.Listen (httpHost, httpPort) |> fun msg -> web.Send(msg, chan)
        let! res = chan.Await()
        match res with
        | Choice1Of2 reply -> printfn "Web reply = %A" reply
        | Choice2Of2 e -> raise e

        let regionId = this.addAnyRegion()
        let zoneId = this.addAnyZone(regionId)
        this.setStartingZone(Human, zoneId)
      with e ->
        printfn "Error: %A" e

      return ()
    }
    |> Async.RunSynchronously
    
    ()

  member this.addAnyRegion () =
    let id = uuid()
    client.AddRegion {
      name = id
    }
    |> sync()
    |> fun m -> 
      match m with
      | AddRegionReply.Success id -> id
      | reply ->
        failwith <| sprintf "Expected AddRegionReply.Success but got %A" reply

  member this.addAnyQuest () =
    let rewards = [|
      XpReward 10.0
    |]
    let objective = TimeObjective {
      timeDelta = 5
    }

    client.AddQuest {
      title = sprintf "Test title %s" <| uuid()
      body = sprintf "Test body %s" <| uuid()
      rewards = rewards
      objective = objective
    }
    |> sync()
    |> fun m ->
      match m with
      | AddQuestReply.Success id -> id
      | reply ->
        failwith <| sprintf "Expected AddQuestReply.Success but got %A" reply

  member this.addAnyZone regionId =
    let id = uuid()
    client.AddZone {
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
    client.SetStartingZone(race, zoneId)
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

    client.AddGarrison(
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
    client.GetClientGarrison(clientId)
    |> sync()
    |> fun reply ->
      match reply with
      | GetClientGarrisonReply.Success garrison -> garrison
      | reply -> sprintf "Expected GetClientGarrisonReply.Success but got %A" reply |> failwith

  [<TestMethod>]
  member this.testNewGarrisonN () =
    for i in 0 .. 1000 do
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

    match client.AddGarrison(clientId, "My garrison", Human, Alliance) |> sync() with
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
      match client.AddHero msg |> sync() with
      | AddHeroReply.Success id -> id
      | msg -> failwith <| sprintf "Expected Success but got %A" msg

    let garrison =
      match client.GetClientGarrison clientId |> sync() with
      | GetClientGarrisonReply.Success garrison -> garrison
      | msg -> failwith <| sprintf "Expected Success but got %A" msg

    // Check and make sure the hero.id is in the garrison stats.
    let heroes = garrison.stats.heroes
    heroes |> Array.tryFind (fun id -> id = heroId)
    |> fun res ->
      match res with
      | None -> failwith "Hero was not in the garrison heroes"
      | _ -> ()

  [<TestMethod>]
  member this.testHeroQuest () =
    let clientId = uuid()

    match client.AddGarrison(clientId, "My garrison", Human, Alliance) |> sync() with
    | AddGarrisonReply.Success -> ()
    | msg -> failwith <| sprintf "Expected Success but got %A" msg

    let msg:AddHero = { 
      clientId = clientId
      name = "hero" + uuid()
      race = Human
      faction = Alliance
      gender = Male
      heroClass = Warrior
    }

    let heroId =
      match client.AddHero msg |> sync() with
      | AddHeroReply.Success id -> id
      | msg -> failwith <| sprintf "Expected Success but got %A" msg

    // Get the hero before the quest.
    let heroI = client.GetHero heroId |> sync() |> fun m ->
      match m with
      | GetHeroReply.Success hero -> hero
      | msg -> failwith <| sprintf "Expected Success but got %A" msg

    // Add a quest.
    let questId = this.addAnyQuest()
    client.HeroBeginQuest(heroId, questId)
    |> sync()
    |> fun m ->
      match m with
      | HeroBeginQuestReply.Success recordId -> recordId
      | reply -> sprintf "Expected GetClientGarrisonReply.Success but got %A" reply |> failwith
    |> ignore

    // Make sure the hero does the quest.
    let _game = platform.Lookup gameId |> Option.get
    for i in 1 .. 5 do
      let chan = Chan<Reply>()
      _game.Send(Tick, chan)
      chan.Await() |> Async.RunSynchronously |> ignore

    let quest, record =
      client.GetHeroQuest heroId
      |> sync()
      |> fun m ->
        match m with
        | GetHeroQuestReply.Success (record, quest) ->
          // Got em!
          quest, record
        | reply -> sprintf "Expected GetHeroQuestReply.Success but got %A" reply |> failwith
    printfn "Got quest %A record %A" quest record

    do
      let chan = Chan<Reply>()
      _game.Send(Tick, chan)
      chan.Await() |> Async.RunSynchronously |> ignore

    client.GetHeroQuest heroId |> sync() |> fun m ->
      match m with
      | GetHeroQuestReply.Empty -> ()
      | reply -> sprintf "Expected Empty but got %A" m |> failwith

    // Get the hero after the quest.
    let heroF = client.GetHero heroId |> sync() |> fun m ->
      match m with
      | GetHeroReply.Success hero -> hero
      | msg -> failwith <| sprintf "Expected Success but got %A" msg

    // The hero should have the rewards from the quest.
    quest.rewards |> List.iter (fun reward ->
      match reward with
      | XpReward xp ->
        // The hero should have the xp reward from the quest.
        let finalXp = heroI.stats.xp + xp
        if heroF.stats.xp <> finalXp then
          sprintf "Expected hero to have %f xp after quest, but hero has %f xp" finalXp heroF.stats.xp
          |> failwith
        else ()
    )
    ()