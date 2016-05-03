namespace Zrpg.Game

open System

  type GameState = {
    gameTime: int
    garrisons: Map<string, Garrison>
    zones: Map<string, Zone>
    regions: Map<string, Region>
    heroes: Map<string, Hero>
    worlds: Map<string, ClientWorld>

    clientGarrisons: Map<string, string>
    clientWorlds: Map<string, string>

    heroNames: string Set
    regionNames: string Set
    zoneNames: string Set

    startingZones: Map<string, string>
    zoneConnections: Map<string, ZoneConnection array>
  }

//type GameState () =
//  let mutable gameTime = 0
//  let mutable garrisons = Map.empty<string, Garrison>
//  let mutable zones = Map.empty<string, Zone>
//  let mutable regions = Map.empty<string, Region>
//  let mutable heroes = Map.empty<string, Hero>
//  let mutable worlds = Map.empty<string, ClientWorld>

//  let mutable clientGarrisons = Map.empty<string, string>
//  let mutable clientWorlds = Map.empty<string, string>

//  let mutable heroNames = Set.empty<string>
//  let mutable regionNames = Set.empty<string>

//  let mutable startingZones = Map.empty<string, string>
//  let mutable worldZoneConnections = Map.empty<string, ZoneConnection array>

//  member this.addWorld (world:ClientWorld) =
//    if clientWorlds.ContainsKey world.clientId then
//      AddWorldReply.ClientHasWorld
//    else
//    clientWorlds <- clientWorlds.Add(world.clientId, world.id)
//    worlds <- worlds.Add(world.id, world)

//    AddWorldReply.Success

//  member this.addGarrison (garrison:Garrison) =
//    if clientGarrisons.ContainsKey garrison.clientId then
//      ClientHasGarrison
//    else
//    garrisons <- garrisons.Add(garrison.id, garrison)
//    clientGarrisons <- clientGarrisons.Add(garrison.clientId, garrison.id)

//    Success

//  member this.addHero (hero:Hero): AddHeroReply =
//    if heroNames.Contains hero.name then
//      NameTaken
//    else

//    // Lookup the client's garrison.
//    clientGarrisons.TryFind hero.clientId
//    |> Option.bind (fun garrisonId ->
//      garrisons.TryFind garrisonId
//    )
//    |> Option.iter (fun garrison ->
//      let heroes = garrison.stats.heroes |> Array.append [| hero.id |]

//      let stats = { garrison.stats with
//        heroes = heroes
//      }
//      garrisons <- garrisons.Add(garrison.id, {
//        garrison with
//          stats = stats
//      })
//    )

//    heroes <- heroes.Add(hero.id, hero)
//    heroNames <- heroNames.Add(hero.name)
//    AddHeroReply.Success hero.id

//  member this.addRegion (region:Region) =
//    match regionNames.Contains region.name with
//    | true -> 
//      AddRegionReply.RegionExists
//    | false ->
//      regionNames <- regionNames.Add region.name
//      regions <- regions.Add(region.id, region)
//      AddRegionReply.Success

//  member this.getClientGarrison clientId =
//    match clientGarrisons.TryFind clientId with
//    | None -> Empty
//    | Some id ->
//      match garrisons.TryFind id with
//      | None -> Empty
//      | Some garrison -> GetClientGarrisonReply.Success garrison

//  member this.getHero id =
//    match heroes.TryFind id with
//    | None -> GetHeroReply.Empty
//    | Some hero -> GetHeroReply.Success hero

//  member this.getStartingZone (race:Race) =
//    let id = race.ToString()
//    match startingZones.TryFind id with
//    | None -> GetStartingZoneReply.Empty
//    | Some id -> GetStartingZoneReply.Success id

//  member this.getZone id =
//    match zones.TryFind id with
//    | None -> GetZoneReply.Empty
//    | Some zone -> GetZoneReply.Success zone

//  member this.getZoneConnections zoneId =
//    worldZoneConnections.TryFind zoneId
//    |> fun option ->
//      match option with
//      | None -> GetZoneConnectionsReply.Empty
//      | Some ls -> GetZoneConnectionsReply.Success ls

//  member this.getClientZoneConnections worldId zoneId =
//    worlds.TryFind worldId
//    |> Option.map (fun world ->
//      world.zoneConnections.TryFind zoneId
//      |> defaultArg <| Array.empty
//    )
//    |> defaultArg <| Array.empty
//    |> Array.append
//    |> fun append ->
//      worldZoneConnections.TryFind zoneId
//      |> defaultArg <| Array.empty
//      |> append
//    |> fun ls ->
//      if Array.length ls = 0 then GetZoneConnectionsReply.Empty
//      else GetZoneConnectionsReply.Success ls

//  member this.remGarrison id =
//    garrisons <- garrisons.Remove id
//    RemGarrisonReply.Success

//  member this.tick () =
//    gameTime <- gameTime + 1