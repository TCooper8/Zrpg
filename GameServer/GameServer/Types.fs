namespace Zrpg.Game

type Gender =
  | Male 
  | Female

type Race =
  | Human

type ClassType =
  | Warrior

type Faction =
  | Horde
  | Alliance

type Npc = {
  id: string
  name: string
  gender: Gender
  race: Race
  faction: Faction
  events: string Set
}

type Zone = {
  id: string
  name: string
  npcs: string Set
}

type Region = {
  id: string
  name: string
  zones: string Set
}

type Character = {
  id: string
  clientId: string
  zoneId: string
  eventId: string option
  name: string
  race: Race
  gender: Gender
  classType: ClassType
  stats: Stats
}