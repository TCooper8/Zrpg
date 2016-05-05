namespace Zrpg.Game

type Msg =
  | AddGarrison of AddGarrison
  | AddHero of AddHero
  | AddRegion of AddRegion
  | AddZone of AddZone
  | AddZoneAssetPositionInfo of AssetPositionInfo
  | AddQuest of AddQuest
  | GetClientGarrison of string
  | GetHero of string
  | GetHeroArray of string array
  | GetHeroInventory of heroId:string
  | GetHeroQuest of heroId:string
  | GetRegion of regionId:string
  | GetZone of zoneId:string
  | GetZoneAssetPositionInfo of zoneId:string
  | HeroBeginQuest of heroId:string * questId:string
  | RemGarrison of string
  | SetStartingZone of Race * string
  | Tick

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
}

type AsyncMsg = {
  msg: Msg
  id: string
}

type Reply =
  | AddGarrisonReply of AddGarrisonReply
  | AddHeroReply of AddHeroReply
  | AddRegionReply of AddRegionReply
  | AddZoneReply of AddZoneReply
  | AddZoneAssetPositionInfoReply
  | AddQuestReply of AddQuestReply
  | AddWorldReply of AddWorldReply
  | ExnReply of string
  | GetClientGarrisonReply of GetClientGarrisonReply
  | GetHeroReply of GetHeroReply
  | GetHeroArrayReply of GetHeroArrayReply
  | GetHeroInventoryReply of GetHeroInventoryReply
  | GetHeroQuestReply of GetHeroQuestReply
  | GetRegionReply of GetRegionReply
  | GetStartingZoneReply of GetStartingZoneReply
  | GetZoneReply of GetZoneReply
  | GetZoneAssetPositionInfoReply of AssetPositionInfo
  | GetZoneConnectionsReply of GetZoneConnectionsReply
  | HeroBeginQuestReply of HeroBeginQuestReply
  | RemGarrisonReply of RemGarrisonReply
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