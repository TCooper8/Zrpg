﻿namespace Zrpg.Game

type Race =
  | Human

type Gender =
  | Male
  | Female

type HeroClass =
  | Warrior

type Faction =
  | Alliance
  | Horde

type Hero = {
  id: string
  clientId: string
  name: string
  race: Race
  faction: Faction
  gender: Gender
  heroClass: HeroClass
  level: int
  stats: HeroStats
} with
  override this.ToString() =
    sprintf "%s\nLevel 4 %A" this.name this.heroClass

type GarrisonStats = {
  goldIncome: int
  heroes: string array
}

type Garrison = {
  id: string
  clientId: string
  name: string
  race: Race
  faction: Faction
  ownedRegions: string Set
  ownedZones: string Set
  vaultId: string
  stats: GarrisonStats
}

type Objective =
  | MenialObjective of MenialObjective
and MenialObjective = {
  id: string
  timeDelta: int
}

type Quest = {
  id: string
  title: string
  body: string
  rewards: string array
  objectives: string array
}

type QuestRecord = {
  id: string
  questId: string
  startTime: string
}