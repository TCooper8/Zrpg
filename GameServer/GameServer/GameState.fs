namespace Zrpg.Game

open System

type GameState () =
  let mutable gameTime = 0
  let mutable garrisons = Map.empty<string, Garrison>
  let mutable clientGarrisons = Map.empty<string, string>

  member this.addGarrison (garrison:Garrison) =
    if clientGarrisons.ContainsKey garrison.clientId then
      AddGarrisonReply ClientHasGarrison
    else
    garrisons <- garrisons.Add(garrison.id, garrison)
    clientGarrisons <- clientGarrisons.Add(garrison.clientId, garrison.id)
    AddGarrisonReply GarrisonAdded

  member this.getClientGarrison clientId =
    match clientGarrisons.TryFind clientId with
    | None -> EmptyReply
    | Some id ->
      GetClientGarrisonReply id

  member this.getGarrison id =
    match garrisons.TryFind id with
    | None -> EmptyReply
    | Some garrison -> GetGarrisonReply garrison

  member this.remGarrison id =
    garrisons <- garrisons.Remove id
    EmptyReply

  member this.tick () =
    gameTime <- gameTime + 1