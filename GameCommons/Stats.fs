namespace Zrpg.Game

type HeroStats = {
  strength: float
  stamina: float
  groundTravelSpeed: float
} with
  static member (*) (a: HeroStats, b: HeroStats) =
    { strength = a.strength * b.strength
      stamina = a.stamina * b.stamina
      groundTravelSpeed = a.groundTravelSpeed * b.groundTravelSpeed
    }
  static member (*) (a: HeroStats, scale: float) =
    { strength = a.strength * scale
      stamina = a.stamina * scale
      groundTravelSpeed = a.groundTravelSpeed * scale
    }

  static member (+) (a: HeroStats, b: HeroStats) =
    { strength = a.strength + b.strength
      stamina = a.stamina + b.stamina
      groundTravelSpeed = a.groundTravelSpeed + b.groundTravelSpeed
    }