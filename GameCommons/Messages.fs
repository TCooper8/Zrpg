namespace Zrpg.Game

type ClientId = string

type Msg =
  | AddGarrison of AddGarrison
  | AddHero of AddHero
  | GetClientGarrison of string
  | GetHero of string
  | GetHeroArray of string array
  | RemGarrison of string

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

type Reply =
  | AddGarrisonReply of AddGarrisonReply
  | AddHeroReply of AddHeroReply
  | ExnReply of string
  | GetClientGarrisonReply of GetClientGarrisonReply
  | GetHeroReply of GetHeroReply
  | GetHeroArrayReply of GetHeroArrayReply
  | RemGarrisonReply of RemGarrisonReply

and AddGarrisonReply =
  | Success
  | ClientHasGarrison

and AddHeroReply =
  | Success of string
  | NameTaken
  | InvalidRaceForFaction
  | InvalidClassForRace

and GetClientGarrisonReply =
  | Success of Garrison
  | Empty

and GetHeroReply =
  | Success of Hero
  | Empty

and GetHeroArrayReply =
  | Success of Hero array
  | Empty

and RemGarrisonReply =
  | Success