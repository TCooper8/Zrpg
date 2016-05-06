﻿namespace Zrpg.Game

open System
module Util =
  let uuid () = Guid.NewGuid().ToString()

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
        | ItemReward reward ->
          let item =
            match this.items.TryFind reward.itemId with
            | None -> sprintf "Item reward %A references invalid item %s" reward reward.itemId |> failwith
            | Some item -> item
          
          match item.info with
          | TradeGood tradeGood ->
            sprintf "[ TradeGood ] %s (%A)" tradeGood.name tradeGood.rarity
          | PoorItem item ->
            sprintf "[ PoorItem ] %s" item.name
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

    let mutable state = this

    let mutable inventory =
      match state.heroInventories.TryFind hero.id with
      | None -> sprintf "Hero %A has no inventory" hero |> failwith
      | Some res -> res

    let stats =
      quest.rewards |> List.fold (fun stats reward ->
        match reward with
        | XpReward xp ->
          { stats with xp = stats.xp + xp }
        | ItemReward reward ->
          // Lookup the item from the rewards and give it to the player.
          let item =
            match state.items.TryFind reward.itemId with
            | None -> sprintf "Item reward from quest %A refers to an invalid item id %s" quest reward.itemId |> failwith
            | Some item -> item
          let quantity = reward.quantity

          let record: ItemRecord = {
            id = Util.uuid()
            itemId = item.id
          }
          state <- {
            state with
              itemRecords = state.itemRecords.Add(record.id, record)
          }

          // Find a pane and slot to fill.
          let slot =
            inventory.panes |> Array.tryPick (fun pane ->
              pane.slots |> Array.tryPick (fun slot ->
                match slot.itemRecordId with
                | GameNone -> Some slot
                | _ -> None
              )
            )
          match slot with
          | None -> failwith "Hero has no empty slots"
          | Some slot ->
            let slot = { slot with itemRecordId = GameSome record.id }
            inventory <- {
              inventory with
                panes = inventory.panes |> Array.map (fun pane ->
                  { pane with
                      slots = pane.slots |> Array.map (fun _slot ->
                        if _slot.position = slot.position then slot
                        else _slot
                      )
                  }
                )
            }
            stats

          // Give the hero quantity * items.
      ) hero.stats

    let hero = {
      hero with
        state = HeroState.Idle
        stats = stats
    }

    let state = {
      state with
        heroInventories = state.heroInventories.Add(hero.id, inventory)
        questRecords = state.questRecords.Remove record.id
        heroes = state.heroes.Add(hero.id, hero)
        heroQuestRecords = state.heroQuestRecords.Remove hero.id
        clientNotifications = state.clientNotifications.Add(hero.clientId, notifications)
    }

    state