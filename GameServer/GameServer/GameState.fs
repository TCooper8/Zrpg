namespace Zrpg.Game

open System

type GameState () =
  let mutable gameTime = 0
  let mutable garrisons = Map.empty<string, Garrison>
  let mutable clientGarrisons = Map.empty<string, string>
  let mutable heroes = Map.empty<string, Hero>
  let mutable heroNames = Set.empty<string>

  member this.addGarrison (garrison:Garrison) =
    if clientGarrisons.ContainsKey garrison.clientId then
      ClientHasGarrison
    else
    garrisons <- garrisons.Add(garrison.id, garrison)
    clientGarrisons <- clientGarrisons.Add(garrison.clientId, garrison.id)
    Success

  member this.addHero (hero:Hero): AddHeroReply =
    if heroNames.Contains hero.name then
      NameTaken
    else
    heroes <- heroes.Add(hero.id, hero)
    heroNames <- heroNames.Add(hero.name)
    AddHeroReply.Success hero.id

  member this.getClientGarrison clientId =
    match clientGarrisons.TryFind clientId with
    | None -> Empty
    | Some id ->
      match garrisons.TryFind id with
      | None -> Empty
      | Some garrison -> GetClientGarrisonReply.Success garrison

  member this.getHero id =
    match heroes.TryFind id with
    | None -> GetHeroReply.Empty
    | Some hero -> GetHeroReply.Success hero

  member this.remGarrison id =
    garrisons <- garrisons.Remove id
    RemGarrisonReply.Success

  member this.tick () =
    gameTime <- gameTime + 1