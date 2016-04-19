namespace Zrpg.GameCommons

open System.Threading.Tasks

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
  | ExnReply of exn

module GameClient =
  [<Interface>]
  type IGameClient =
    abstract member AddGarrison : string * string * Race * Faction -> Reply Task