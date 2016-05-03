namespace Zrpg.Game

open System

  type GameState = {
    gameTime: int
    garrisons: Map<string, Garrison>
    zones: Map<string, Zone>
    regions: Map<string, Region>
    heroes: Map<string, Hero>
    worlds: Map<string, ClientWorld>

    // Hero data
    heroQuestRecords: Map<string, string>
    heroInventories: Map<string, HeroInventory>

    // Zone data
    zoneQuests: Map<string, string array>

    quests: Map<string, Quest>
    questRecords: Map<string, QuestRecord>

    clientGarrisons: Map<string, string>
    clientWorlds: Map<string, string>

    heroNames: string Set
    regionNames: string Set
    zoneNames: string Set

    startingZones: Map<string, string>
    zoneConnections: Map<string, ZoneConnection array>
  } with
    static member Default = {
      gameTime = 0
      garrisons = Map.empty
      zones = Map.empty
      regions = Map.empty
      heroes = Map.empty
      worlds = Map.empty
      heroQuestRecords = Map.empty
      heroInventories = Map.empty
      zoneQuests = Map.empty
      quests = Map.empty
      questRecords = Map.empty
      clientGarrisons = Map.empty
      clientWorlds = Map.empty

      heroNames = Set.empty
      regionNames = Set.empty
      zoneNames = Set.empty

      startingZones = Map.empty
      zoneConnections = Map.empty
    }