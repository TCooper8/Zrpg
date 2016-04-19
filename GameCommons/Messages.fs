namespace Zrpg.Game

type Msg =
  | AddGarrison of AddGarrison
and AddGarrison = {
  clientId: string
  name: string
  race: Race
  faction: Faction
}

type Reply =
  | EmptyReply
  | MsgReply of string
  | ExnReply of string