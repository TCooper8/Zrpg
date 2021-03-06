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

  artisans: Map<string, Artisan>
  recipes: Map<string, Recipe>

  // Zone data
  zoneQuests: Map<string, string array>
  zoneAssetPositionInfo: Map<string, AssetPositionInfo>

  quests: Map<string, Quest>
  questRecords: Map<string, QuestRecord>

  clientArtisans: Map<string, string array>
  clientGarrisons: Map<string, string>
  clientWorlds: Map<string, string>
  clientNotifications: Map<string, NotifyRecord list>
  clientClosedQuests: Map<string, string Set>

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

    artisans = Map.empty
    recipes = Map.empty

    zoneQuests = Map.empty
    zoneAssetPositionInfo = Map.empty

    quests = Map.empty
    questRecords = Map.empty

    clientArtisans = Map.empty
    clientGarrisons = Map.empty
    clientWorlds = Map.empty
    clientNotifications = Map.empty
    clientClosedQuests = Map.empty

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
        | ZoneUnlockReward zoneId ->
          let zone = this.zones.[zoneId]
          sprintf "Discovered %s" zone.name
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
      messageTitle = messageTitle
      messageBody = messageBody
    }
    let record = {
      id = Util.uuid()
      item = notify
      timestamp = DateTime.Now
    }

    let notifications =
      this.clientNotifications.TryFind hero.clientId
      |> defaultArg <| []
      |> List.append [ record ]

    let mutable state = this

    let mutable inventory =
      match state.heroInventories.TryFind hero.id with
      | None -> sprintf "Hero %A has no inventory" hero |> failwith
      | Some res -> res

    let stats: HeroStats =
      quest.rewards |> List.fold (fun stats reward ->
        match reward with
        | ZoneUnlockReward zoneId ->
          let garrisonId = state.clientGarrisons.[hero.clientId]
          let garrison = state.garrisons.[garrisonId]
          let zones = garrison.ownedZones |> Array.append [| zoneId |]
          let garrison = {
            garrison with
              ownedZones = zones
          }
          state <- {
            state with
              garrisons = state.garrisons.Add(garrisonId, garrison)
          }
          stats
        | XpReward xp ->
          { stats with xp = stats.xp + xp }
        | ItemReward reward ->
          // Lookup the item from the rewards and give it to the player.
          let item =
            match state.items.TryFind reward.itemId with
            | None -> sprintf "Item reward from quest %A refers to an invalid item id %s" quest reward.itemId |> failwith
            | Some item -> item
          let quantity = reward.quantity

          // Find a pane and slot to fill.
          //let slot =
          //  inventory.panes |> Array.tryPick (fun pane ->
          //    pane.slots |> Array.tryPick (fun slot ->
          //      match slot.itemRecordId with
          //      | GameNone -> Some (pane.position, slot)
          //      | _ -> None
          //    )
          //  )

          let mutable added = false
          inventory <- {
            inventory with
              panes = inventory.panes |> Array.map (fun pane ->
                if added then pane else

                pane.slots |> Array.map (fun slot ->
                  if added then slot else

                  match slot.itemRecordId with
                  | GameNone ->
                    // Empty slot, create a record.
                    added <- true
                    let record = {
                      id = Util.uuid()
                      itemId = item.id
                      quantity = quantity
                    }
                    state <- { state with itemRecords = state.itemRecords.Add(record.id, record) }
                    { slot with itemRecordId = GameSome record.id }
                  | GameSome recordId ->
                    match state.itemRecords.TryFind recordId with
                    | None ->
                      // The record doesn't actually exist.
                      added <- true
                      let record = {
                        id = Util.uuid()
                        itemId = item.id
                        quantity = quantity
                      }
                      state <- { state with itemRecords = state.itemRecords.Add(record.id, record) }
                      { slot with itemRecordId = GameSome record.id }
                    | Some record ->
                      if record.itemId = item.id then
                        // We can stack this item.
                        added <- true
                        let record = { record with quantity = record.quantity + quantity }
                        state <- { state with itemRecords = state.itemRecords.Add(record.id, record) }
                        slot
                      else
                        slot
                )
                |> fun slots -> { pane with slots = slots }
              )
          }

          if not added then
            // Need to mail the item to the player.
            failwith "Item cannot be added to inventory"
          else
            // Add the inventory to the state
            state <- {
              state with
                heroInventories = state.heroInventories.Add(inventory.id, inventory)
            }
            stats
      ) hero.stats

    let hero = {
      hero with
        state = HeroState.Idle
        stats = stats
    }

    let closedQuests =
      state.clientClosedQuests.TryFind hero.clientId
      |> defaultArg <| Set []
      |> Set.add quest.id

    let state = {
      state with
        heroInventories = state.heroInventories.Add(hero.id, inventory)
        questRecords = state.questRecords.Remove record.id
        heroes = state.heroes.Add(hero.id, hero)
        heroQuestRecords = state.heroQuestRecords.Remove hero.id
        clientNotifications = state.clientNotifications.Add(hero.clientId, notifications)
        clientClosedQuests = state.clientClosedQuests.Add(hero.clientId, closedQuests)
    }

    state