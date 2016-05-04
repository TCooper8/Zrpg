namespace Zrpg.Game

type HeroStats = {
  strength: float
  stamina: float
  xp: float
  finalXp: float
  groundTravelSpeed: float
} with
  static member (*) (a: HeroStats, b: HeroStats) =
    { strength = a.strength * b.strength
      stamina = a.stamina * b.stamina
      xp = a.xp * b.xp
      finalXp = a.finalXp * b.finalXp
      groundTravelSpeed = a.groundTravelSpeed * b.groundTravelSpeed
    }

  static member (+) (a: HeroStats, b: HeroStats) =
    { strength = a.strength + b.strength
      stamina = a.stamina + b.stamina
      xp = a.xp + b.xp
      finalXp = a.finalXp + b.finalXp
      groundTravelSpeed = a.groundTravelSpeed + b.groundTravelSpeed
    }