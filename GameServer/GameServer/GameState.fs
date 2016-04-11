namespace Zrpg.Game

open System

type GameState () =
  let mutable quests = Map.empty<string, Quest>
  let mutable dialogues = Map.empty<string, Dialogue>
  let mutable npcs = Map.empty<string, Npc>
  let mutable zones = Map.empty<string, Zone>
  let mutable regions = Map.empty<string, Region>
  let mutable characters = Map.empty<string, Character>
  let mutable startingZones = Map.empty<string, string>

  let raceStats race =
    match race with
    | Human ->
      { strength = 1.1
        agility = 1.0
        stamina = 1.0
        intellect = 1.0
        spirit = 1.0
        charisma = 1.0
        insight = 1.0
        history = 1.0
        survival = 1.0
      }

  let classStats classType =
    match classType with
    | Warrior ->
      { strength = 2.0
        agility = 1.5
        stamina = 1.5
        intellect = 1.0
        spirit = 1.0
        charisma = 1.0
        insight = 1.0
        history = 1.0
        survival = 1.0
      }

  member this.AddRegion (region: Region) =
    regions <- regions.Add(region.id, region)

  member this.AddZone regionId (zone:Zone) =
    match regions.TryFind regionId with
    | None ->
      failwith
      <| sprintf "Region with id = %s does not exist." regionId
    | Some region ->
      let region = { region with zones = region.zones.Add(zone.id) }
      regions <- regions.Add(regionId, region)
      zones <- zones.Add(zone.id, zone)

  member this.AddNpc zoneId (npc:Npc) =
    match zones.TryFind zoneId with
    | None ->
      failwith
      <| sprintf "Zone with id = %s does not exist." zoneId
    | Some zone ->
      let zone = { zone with npcs = zone.npcs.Add npc.id }
      zones <- zones.Add(zoneId, zone)
      npcs <- npcs.Add(npc.id, npc)

  member this.AddQuest npcId (quest:Quest) =
    match npcs.TryFind npcId with
    | None ->
      failwith
      <| sprintf "Npc with id = %s does not exist." npcId
    | Some npc ->
      let npc = { npc with events = npc.events.Add quest.id }
      npcs <- npcs.Add(npcId, npc)
      quests <- quests.Add(quest.id, quest)

  member this.AddQuestLink questId (quest:Quest) =
    match quests.TryFind questId with
    | None ->
      failwith
      <| sprintf "Quest with id = %s does not exist." questId
    | Some head ->
      let head = { head with responses = head.responses.Add quest.id }
      quests <- quests.Add(questId, head)
      quests <- quests.Add(quest.id, quest)

  member this.AddDialogue npcId (dialogue:Dialogue) =
    match npcs.TryFind npcId with
    | None ->
      failwith
      <| sprintf "Npc with id = %s does not exist." npcId
    | Some npc ->
      let npc = { npc with events = npc.events.Add dialogue.id }
      npcs <- npcs.Add(npcId, npc)
      dialogues <- dialogues.Add(dialogue.id, dialogue)

  member this.GetStartingZoneId race =
    match startingZones.TryFind (race.ToString()) with
    | None ->
      failwith
      <| sprintf "Starting zone for %A has not been defined." race
    | Some zoneId ->
      zoneId

  member this.AddCharacter clientId name race gender classType =
    let stats = (classStats classType) * (raceStats race)

    let startingZone = this.GetStartingZoneId race

    let char = {
      id = Guid().ToString()
      clientId = clientId
      zoneId = startingZone
      eventId = None
      name = name
      race = race
      gender = gender
      classType = classType
      stats = stats
    }
    characters <- characters.Add(char.id, char)

  member this.SetCharacter char =
    characters <- characters.Add(char.id, char)

  member this.SetStartingZone (race:Race) zoneId =
    match zones.TryFind zoneId with
    | None ->
      failwith
      <| sprintf "Cannot set starting zone for %A, Zone %s does not exist." race zoneId
    | _ -> ()
    startingZones <- startingZones.Add(race.ToString(), zoneId)

  member this.GetCharacters () = characters
  member this.GetZone zoneId =
    match zones.TryFind zoneId with
    | None ->
      failwith
      <| sprintf "Zone with id = %s does not exist." zoneId
    | Some zone ->
      zone

  member this.GetQuest questId =
    quests.[questId]

  member this.GetDialogue dialogueId =
    dialogues.[dialogueId]

  member this.GetEvent eventId =
    match quests.TryFind eventId, dialogues.TryFind eventId with
    | Some quest, Some dialogue ->
      failwith
      <| sprintf "Event.id %s is mapped to quest %A and dialogue %A." eventId quest dialogue
    | Some quest, None ->
      QuestEvent quest
    | None, Some dialogue ->
      DialogueEvent dialogue
    | None, None ->
      failwith
      <| sprintf "Event.id %s is not mapped to a quest or an event." eventId

  member this.GetChildEvents eventId =
    let childQuests =
      quests.TryFind eventId
      |> Option.map (fun quest ->
        quest.responses 
        |> Set.map (this.GetQuest >> QuestEvent)
        |> Set.toList
      )
      |> defaultArg <| []

    let childDialogues =
      dialogues.TryFind eventId
      |> Option.map (fun dialogue ->
        dialogue.responses
        |> Set.map (this.GetDialogue >> DialogueEvent)
        |> Set.toList
      )
      |> defaultArg <| []

    childQuests@childDialogues

  member this.GetNpc npcId =
    npcs.[npcId]

  member this.CharLog (char:Character) msg =
    printfn "%A Journal %s : %s" DateTime.Now char.name msg

  member this.CharLogQuest (char:Character) (quest:Quest) =
    printfn "%A Journal %s : %s" DateTime.Now char.name quest.message

  member this.CharLogDialogue (char:Character) (dialogue:Dialogue) =
    printfn "%A Journal %s : %s" DateTime.Now char.name dialogue.message
