namespace Zrpg.Game

type ClientId = string

type Msg =
  | AddGarrison of AddGarrison
  | GetClientGarrison of string
  | GetGarrison of string
  | RemGarrison of string
and AddGarrison = {
  clientId: string
  name: string
  race: Race
  faction: Faction
}

type Reply =
  | EmptyReply
  | AddGarrisonReply of AddGarrisonReply
  | GetClientGarrisonReply of string
  | GetGarrisonReply of Garrison
  | MsgReply of string
  | ExnReply of string
and AddGarrisonReply =
  | ClientHasGarrison
  | GarrisonAdded