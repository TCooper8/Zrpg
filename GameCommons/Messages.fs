namespace Zrpg.Game

type ClientId = string

type Msg =
  | AddGarrison of AddGarrison
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

type Reply =
  | AddGarrisonReply of AddGarrisonReply
  | ExnReply of string
  | GetClientGarrisonReply of GetClientGarrisonReply
  | GetHeroReply of GetHeroReply
  | GetHeroArrayReply of GetHeroArrayReply
  | RemGarrisonReply of RemGarrisonReply

and AddGarrisonReply =
  | Success
  | ClientHasGarrison

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