namespace Zrpg.Game

type ClientId = string

type Msg =
  | AddGarrison of AddGarrison
  | AddHero of AddHero
  | AddRegion of AddRegion
  | AddZone of AddZone
  | GetClientGarrison of string
  | GetHero of string
  | GetHeroArray of string array
  | GetHeroInventory of heroId:string
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

type Reply =
  | AddGarrisonReply of AddGarrisonReply
  | AddHeroReply of AddHeroReply
  | AddRegionReply of AddRegionReply
  | AddZoneReply of AddZoneReply
  | AddWorldReply of AddWorldReply
  | ExnReply of string
  | GetClientGarrisonReply of GetClientGarrisonReply
  | GetHeroReply of GetHeroReply
  | GetHeroArrayReply of GetHeroArrayReply
  | GetHeroInventoryReply of GetHeroInventoryReply
  | GetStartingZoneReply of GetStartingZoneReply
  | GetZoneReply of GetZoneReply
  | GetZoneConnectionsReply of GetZoneConnectionsReply
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

and GetStartingZoneReply =
  | Success of string
  | Empty

and GetZoneReply =
  | Success of Zone
  | Empty

and GetZoneConnectionsReply =
  | Success of ZoneConnection array
  | Empty

and RemGarrisonReply =
  | Success

and SetStartingZoneReply =
  | Success
  | ZoneDoesNotExist