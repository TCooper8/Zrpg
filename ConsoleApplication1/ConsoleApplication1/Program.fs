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

let mutable itemIds = Map.empty<string, string>
let mutable questIds = Map.empty<string, string>
let mutable regionIds = Map.empty<string, string>
let mutable zoneIds = Map.empty<string, string>

let items () = [
  fun () -> ("Copper Ore", {
    assetId = "inv_fabric_linen.jpg"
    info = TradeGood {
      name = "Copper Ore"
      rarity = Common
    }
  })
  fun () -> ("Copper Bar", {
    assetId = "inv_fabric_linen.jpg"
    info = TradeGood {
      name = "Copper Bar"
      rarity = Common
    }
  })
]

let regions: (unit -> AddRegion) list = [
  fun () ->
    { name = "Elwynn Forest"
    }
]

let zones: (unit -> AddZone) list = [
  fun () ->
    { name = "Northshire" 
      regionId = regionIds.["Elwynn Forest"]
      terrain = Plains
    }
  fun () ->
    { name = "Goldshire"
      regionId = regionIds.["Elwynn Forest"]
      terrain = Plains
    }
]

let quests = [
  fun () -> 
    { zoneId = zoneIds.["Northshire"]
      title = "Join the battle!"
      body = "Report to Sergeant Williams behind Northshire Abbey."
      rewards =
        [|  XpReward 100.0
        |]
      objective = TimeObjective {
        timeDelta = 5
      }
      childQuests = Array.empty
    }
  fun () ->
    { zoneId = zoneIds.["Northshire"]
      title = "Beating Them Back!"
      body = "Kill 6 Blackrock Worgs."
      rewards =
        [|  XpReward 100.0
            ZoneUnlockReward zoneIds.["Goldshire"]
        |]
      objective = TimeObjective {
        timeDelta = 5
      }
      childQuests = [| questIds.["Join the battle!"] |]
    }
]

let recipes (): AddRecipe list = [
  { name = "Copper Bar"
    craftedItemId = itemIds.["Copper Bar"]
    xpReward = 100.0
    materialCosts =
      [| { itemId = itemIds.Item "Copper Ore"; quantity = 5 }
      |]
    requirements =
      [|  ProfessionRequirement Blacksmith
      |]
    tags = [| "Materials" |]
  }
]

let addRegions (game:Game.GameServer.GameServerChan) = async { 
  for f in regions do
    let region = f()
    let! reply = game.AddRegion region
    match reply with
    | AddRegionReply.RegionExists -> ()
    | AddRegionReply.Success id ->
      regionIds <- regionIds.Add(region.name, id)
}

let addZones (game:Game.GameServer.GameServerChan) = async {
  for f in zones do
    let zone = f()
    let! reply = game.AddZone zone
    match reply with
    | AddZoneReply.RegionDoesNotExist -> sprintf "%A" reply |> failwith
    | AddZoneReply.Success id -> zoneIds <- zoneIds.Add(zone.name, id)
    | AddZoneReply.ZoneExists -> ()
}

let addItems (game:Game.GameServer.GameServerChan) = async {
  for f in items() do
    let name, msg = f()
    let! id = game.AddItem(msg)
    itemIds <- itemIds.Add(name, id)
}

let addRecipes (game:IGameClient) = async {
  for msg in recipes() do
    let! id = game.AddRecipe msg |> Async.AwaitTask
    ()
}

let addQuests (game:Game.GameServer.GameServerChan) = async {
  for f in quests do
    let quest = f()
    let! reply = game.AddQuest quest
    let id =
      match reply with
      | AddQuestReply.Success id -> id
    questIds <- questIds.Add(quest.title, id)
}

let platform = Platform.create "Main"
let web = WebServer.create platform "web"

//log
let game = Game.GameServer.server platform "Zrpg.GameServer"

let client = GameClient.RESTClient httpEndPoint

// Have the web server listen.
async {
  let! handler = game.GetApiHandler()
  web.Send <| WebServer.AddHandler (handler, 100)

  let! handler = game.GetSocketHandler()
  web.Send <| WebServer.AddWsHandler handler
  
  let chan = Chan<WebServer.Reply>()
  WebServer.Listen (httpHost, httpPort) |> fun msg -> web.Send(msg, chan)
  let! res = chan.Await()
  match res with
  | Choice1Of2 reply -> printfn "Web reply = %A" reply
  | Choice2Of2 e -> raise e

  do! addRegions game
  do! addZones game
  do! addItems game
  do! addRecipes client
  do! addQuests game

  // Load the asset info
  do! game.AddZoneAssetPositionInfo {
    id = zoneIds.["Northshire"]
    assetId = "elwynnForestMap.jpg"
    left = 420.0 / 930.0
    right = 490.0 / 930.0
    top = 175.0 / 510.0
    bottom = 250.0 / 510.0
  }
  do! game.AddZoneAssetPositionInfo {
    id = zoneIds.["Goldshire"]
    assetId = "elwynnForestMap.jpg"
    left = 346.0 / 930.0
    right = 441.0 / 930.0
    top = 302.0 / 510.0
    bottom = 344.0 / 510.0
  }

  let! reply = game.SetStartingZone (Human, zoneIds.["Northshire"])
  match reply with
  | SetStartingZoneReply.Success -> ()
  | _ -> failwith <| sprintf "Expected AddZoneReply.Success but got %A" reply
}
|> Async.Catch
|> Async.RunSynchronously
|> printfn "Result = %A"

// Run the REPL
//let repl = CommandLine.create platform "repl"
//repl.LoadConsole ()

async {
  let game = platform.Lookup "Zrpg.GameServer" |> Option.get
  while true do
    do! Async.Sleep(1024)
    game.Send Tick
} |> Async.RunSynchronously