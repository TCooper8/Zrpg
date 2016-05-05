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

    // Item data
    itemRecords: Map<string, ItemRecord>
    items: Map<string, Item>

    // Zone data
    zoneQuests: Map<string, string array>
    zoneAssetPositionInfo: Map<string, AssetPositionInfo>

    quests: Map<string, Quest>
    questRecords: Map<string, QuestRecord>

    clientGarrisons: Map<string, string>
    clientWorlds: Map<string, string>
    clientNotifications: Map<string, ClientNotification list>

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

      itemRecords = Map.empty
      items = Map.empty

      zoneQuests = Map.empty
      zoneAssetPositionInfo = Map.empty

      quests = Map.empty
      questRecords = Map.empty

      clientGarrisons = Map.empty
      clientWorlds = Map.empty
      clientNotifications = Map.empty

      heroNames = Set.empty
      regionNames = Set.empty
      zoneNames = Set.empty

      startingZones = Map.empty
      zoneConnections = Map.empty
    }

    member this.CompleteQuest (hero:Hero) (record:QuestRecord) (quest:Quest) =
      let messageTitle =
        sprintf "Hero %s completed quest \"%s\"" hero.name quest.title

      let messageBody =
        quest.rewards
        |> List.map (fun reward ->
          match reward with
          | XpReward xp ->
            sprintf "XP : %f" xp
        )
        |> fun rewards -> String.Join("\n\t", rewards)
        |> sprintf "Rewards = \n\t%s"

      let notify = NotifyQuestCompleted {
        questId = quest.id
        finishTime = this.gameTime
        messageTitle = messageTitle
        messageBody = messageBody
      }

      let notifications =
        this.clientNotifications.TryFind hero.clientId
        |> defaultArg <| []

      let stats =
        quest.rewards |> List.fold (fun stats reward ->
          match reward with
          | XpReward xp ->
            { stats with xp = stats.xp + xp }
        ) hero.stats

      let hero = {
        hero with
          state = HeroState.Idle
          stats = stats
      }

      let state = {
        this with
          questRecords = this.questRecords.Remove record.id
          heroes = this.heroes.Add(hero.id, hero)
          heroQuestRecords = this.heroQuestRecords.Remove hero.id
          clientNotifications = this.clientNotifications.Add(hero.clientId, notifications)
      }

      state