namespace Zrpg.Game

type Msg =
  | AddArtisan of AddArtisan
  | AddItem of AddItem
  | AddGarrison of AddGarrison
  | AddHero of AddHero
  | AddRecipe of AddRecipe
  | AddRegion of AddRegion
  | AddZone of AddZone
  | AddZoneAssetPositionInfo of AssetPositionInfo
  | AddQuest of AddQuest
  | GetClientArtisans of clientId:string
  | GetClientGarrison of clientId:string
  | GetClientNotifications of clientId:string
  | GetGameTime
  | GetHero of string
  | GetHeroArray of string array
  | GetHeroInventory of heroId:string
  | GetHeroQuest of heroId:string
  | GetItem of recordId:string
  | GetRegion of regionId:string
  | GetQuestsInZone of clientId:string * zoneId:string
  | GetZone of zoneId:string
  | GetZoneAssetPositionInfo of zoneId:string
  | GetZoneQuests of zoneId:string
  | HeroBeginQuest of heroId:string * questId:string
  | RemGarrison of string
  | RemNotification of clientId:string * notifyId:string
  | SetStartingZone of Race * string
  | Tick

and AddArtisan = {
  clientId:string
  name: string
  profession: Profession
}

and AddItem = {
  assetId: string
  info: ItemInfo
}

and AddGarrison = {
  clientId: string
  name: string
  race: Race
  faction: Faction
}

and AddHero = {
  clientId: string
  name: string
  race: Race
  faction: Faction
  gender: Gender
  heroClass: HeroClass
}

and AddRecipe = {
  name:string
  craftedItemId: string
  xpReward: float
  materialCosts: MaterialCost array
  requirements: RecipeRequirement array
  tags: string array
}

and AddRegion = {
  name: string
}

and AddZone = {
  name: string
  regionId: string
  terrain: Terrain
}

and AddQuest = {
  zoneId: string
  title: string
  body: string
  rewards: QuestReward array
  objective: Objective
  childQuests: string array
}

type AsyncMsg = {
  msg: Msg
  id: string
}

type Reply =
  | AddArtisanReply of artisanId:string
  | AddItemReply of itemId:string
  | AddGarrisonReply of AddGarrisonReply
  | AddHeroReply of AddHeroReply
  | AddRecipeReply of recipeId:string
  | AddRegionReply of AddRegionReply
  | AddZoneReply of AddZoneReply
  | AddZoneAssetPositionInfoReply
  | AddQuestReply of AddQuestReply
  | AddWorldReply of AddWorldReply
  | ExnReply of string
  | GetClientArtisansReply of Artisan array
  | GetClientGarrisonReply of GetClientGarrisonReply
  | GetClientNotificationsReply of NotifyRecord array
  | GetGameTimeReply of int
  | GetHeroReply of GetHeroReply
  | GetHeroArrayReply of GetHeroArrayReply
  | GetHeroInventoryReply of GetHeroInventoryReply
  | GetHeroQuestReply of GetHeroQuestReply
  | GetItemReply of record:ItemRecord * item:Item
  | GetRegionReply of GetRegionReply
  | GetStartingZoneReply of GetStartingZoneReply
  | GetQuestsInZoneReply of Quest array
  | GetZoneReply of GetZoneReply
  | GetZoneAssetPositionInfoReply of AssetPositionInfo
  | GetZoneConnectionsReply of GetZoneConnectionsReply
  | GetZoneQuestsReply of Quest array
  | HeroBeginQuestReply of HeroBeginQuestReply
  | RemGarrisonReply of RemGarrisonReply
  | RemNotificationReply
  | SetStartingZoneReply of SetStartingZoneReply
  | TickReply

and AddGarrisonReply =
  | Success
  | ClientHasGarrison
  | ClientHasWorld

and AddHeroReply =
  | Success of string
  | NameTaken
  | ClientHasNoGarrison
  | InvalidRaceForFaction
  | InvalidClassForRace

and AddRegionReply =
  | Success of string
  | RegionExists

and AddWorldReply =
  | Success of string
  | ClientHasWorld

and AddQuestReply =
  | Success of questId:string

and AddZoneReply =
  | Success of zoneId:string
  | RegionDoesNotExist
  | ZoneExists

and GetClientGarrisonReply =
  | Success of Garrison
  | Empty

and GetHeroReply =
  | Success of Hero
  | Empty

and GetHeroArrayReply =
  | Success of Hero array
  | Empty

and GetHeroInventoryReply =
  | Success of HeroInventory
  | Empty

and GetHeroQuestReply =
  | Success of QuestRecord * Quest
  | Empty

and GetRegionReply =
  | Success of Region
  | Empty

and GetStartingZoneReply =
  | Success of string
  | Empty

and GetZoneReply =
  | Success of Zone
  | Empty

and GetZoneConnectionsReply =
  | Success of ZoneConnection array
  | Empty

and HeroBeginQuestReply =
  | Success of questRecordId:string
  | HeroDoesNotExist
  | HeroIsQuesting
  | QuestDoesNotExist

and RemGarrisonReply =
  | Success

and SetStartingZoneReply =
  | Success
  | ZoneDoesNotExist

type AsyncReply = {
  reply: Reply
  id: string
}