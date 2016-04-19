namespace Zrpg.Game

open System

type GameState () =
  let mutable gameTime = 0
  let mutable garrisons = Map.empty<string, Garrison>
  let mutable clientGarrisons = Map.empty<string, string>

  member this.addGarrison (garrison:Garrison) =
    if clientGarrisons.ContainsKey garrison.clientId then
      Try.failwith <| sprintf "Client %s already has a garrison" garrison.clientId
    else
    garrisons <- garrisons.Add(garrison.id, garrison)
    clientGarrisons <- clientGarrisons.Add(garrison.clientId, garrison.id)
    Success ()

  member this.getClientGarrison clientId =
    match clientGarrisons.TryFind clientId with
    | None -> Try.failwith <| sprintf "Garrison for client %s does not exist" clientId
    | Some id ->
      this.getGarrison id

  member this.getGarrison id =
    match garrisons.TryFind id with
    | None -> Try.failwith <| sprintf "Garrison %s does not exist" id
    | Some garrison -> Success garrison

  member this.remGarrison id =
    garrisons <- garrisons.Remove id

  member this.tick () =
    gameTime <- gameTime + 1