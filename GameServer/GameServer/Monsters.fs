namespace Zrpg.Game

type MonsterType = {
  id: string
  name: string
  stats: Stats
  statsLevelMod: Stats
}

type Monster = {
  id: string
  level: int
  typeId: string
  stats: Stats
}