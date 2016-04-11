// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open Zrpg
open Zrpg.Game
open System

let libuuid () = 
  Guid.NewGuid().ToString()

let game = GameState()

let Change =
  | Msg of string
  | 

let elwynnForest =
  let region = {
    Region.id = libuuid()
    name = "Elwynn Forest"
    zones = Set.empty
  }
  game.AddRegion region

  let northshire =
    let zone = {
      Zone.id = libuuid()
      name = "Northshire"
      npcs = Set.empty
      }
    game.AddZone region.id zone

    let blacksmith =
      let npc = {
        Npc.id = libuuid()
        name = "Smith Argus"
        gender = Male
        race = Human
        faction = Alliance
        events = Set.empty
      }
      game.AddNpc zone.id npc
      
      let firstJob =
        // Start of the quest chain.
        let start = {
          Quest.id = libuuid()
          message = "The town blacksmith asked me if I wanted to help him out with a quest."
          responses = Set.empty
        }
        game.AddQuest npc.id start

        let id = libuuid()
        game.AddQuestLink start.id {
          Quest.id = id
          message = "Argus says that I need to kill some wolves. I'll just add that to my todo list."
          responses = Set.empty
        }

        //let prevId = id
        //let id = libuuid()
        //// Add the wolves objective.
        //game.AddQuestLink prevId {
        //  Objective.id = id
        //  links = Set.empty
        //}

        game.AddQuestLink start.id {
          Quest.id = libuuid()
          message = "I have to go out and kill 10 wolves in the woods. Wish me luck!"
          responses = Set.empty
        }

        game.AddQuestLink start.id {
          Quest.id = libuuid()
          message = "Fuck that quest! That old Smith Argus is lazy."
          responses = Set.empty
        }

        start
      npc

    let tailor =
      let npc = {
        Npc.id = libuuid()
        name = "Jaina"
        gender = Female
        race = Human
        faction = Alliance
        events = Set.empty
      }
      game.AddNpc zone.id npc
    zone

  // Northshire is the starting zone for human characters.
  game.SetStartingZone Human northshire.id
  region

[<EntryPoint>]
let main argv = 
  let clientId = libuuid()

  game.AddCharacter clientId "Goclsmacl" Human Male Warrior

  let server = Game(game)

  let rec loop () = async {
    do! Async.Sleep 5000
    server.Tick ()

    return! loop()
  }
  loop () |> Async.Start

  Console.ReadLine() |> ignore
  server.Stop()

  printfn "%A" argv
  0 // return an integer exit code