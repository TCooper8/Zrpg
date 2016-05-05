namespace Zrpg.Game

type Race =
  | Human
  with
    override this.ToString() =
      match this with
      | Human -> "Human"

type Gender =
  | Male
  | Female
  with
    override this.ToString() =
      match this with
      | Male -> "Male"
      | Female -> "Female"

type HeroClass =
  | Warrior
  with
    override this.ToString() =
      match this with
      | Warrior -> "Warrior"

type Faction =
  | Alliance
  | Horde
  with
    override this.ToString() =
      match this with
      | Alliance -> "Alliance"
      | Horde -> "Horde"

type 'a GameOption =
  | GameSome of 'a
  | GameNone

type ItemSlot = {
  position: int
  itemRecordId: string GameOption
}

type InventoryPane = {
  position: int
  slots: ItemSlot array
}

type HeroInventory = {
  id: string
  heroId: string
  panes: InventoryPane array
}

type HeroState =
  | Questing
  | Idle

type Hero = {
  id: string
  clientId: string
  state: HeroState
  zoneId: string // Shows what zone the hero is in.
  name: string
  race: Race
  faction: Faction
  gender: Gender
  heroClass: HeroClass
  level: int
  stats: HeroStats
} with
  override this.ToString() =
    sprintf "%s\nLevel %i %A" this.name this.level this.heroClass

type GarrisonStats = {
  goldIncome: int
  heroes: string array
}

type Garrison = {
  id: string
  clientId: string
  worldId: string
  name: string
  race: Race
  faction: Faction
  ownedRegions: string array
  ownedZones: string array
  vaultId: string
  stats: GarrisonStats
}

type Terrain =
  | Plains
  | Forest
  with
    override this.ToString() =
      sprintf "%A" this

type AssetPositionInfo = {
  id: string
  assetId: string
  left: float
  right: float
  top: float
  bottom: float
}

type Zone = {
  id: string
  name: string
  regionId: string
  terrain: Terrain
}

type Region = {
  id: string
  name: string
  zones: string array
}

type ZoneConnection =
  | GroundConnection of GroundConnection
  | FlightConnection of FlightConnection

and GroundConnection = {
  srcZoneId: string
  dstZoneId: string
  terrain: Terrain
  distance: float
}

and FlightConnection = {
  srcZoneId: string
  dstZoneId: string
  terrain: Terrain
  distance: float
}

type ClientWorld = {
  id: string
  clientId: string
  zoneConnections: Map<string, ZoneConnection array>
  zoneOwners: Map<string, string>
  discoveredZones: string array
  discoveredRegions: string array
}

type Quest = {
  id: string
  zoneId: string
  title: string
  body: string
  rewards: QuestReward list
  objective: Objective
}

and Objective =
  | TimeObjective of TimeObjective

and QuestReward =
  | ItemReward of ItemReward
  | XpReward of float

and TimeObjective = {
  timeDelta: int
}

and ItemReward = {
  itemId: string
  quantity: int
}

type QuestRecord = {
  id: string
  questId: string
  startTime: int
}

type QuestCompleted = {
  id: string
  questId: string
}

type ItemRecord = {
  id: string
  itemId: string
  stackSize: int
  maxStackSize: int
}

type Rarity =
  | Common
  | Uncommon
  | Rare
  | Epic
  | Legendary

type Item = {
  id:string
  info:ItemInfo
}

and ItemInfo =
  | TradeGood of TradeGood
  | PoorItem of PoorItem

and TradeGood = {
  name: string
  rarity: Rarity
}

and PoorItem = {
  name: string
}