namespace Zrpg.Game

type Stats = {
  strength: float
  agility: float
  stamina: float
  intellect: float
  spirit: float
  charisma: float
  insight: float
  history: float
  survival: float
} with
  static member (*) (a: Stats, b: Stats) =
    { strength = a.strength * b.strength
      agility = a.agility * b.agility
      stamina = a.stamina * b.stamina
      intellect = a.intellect * b.intellect
      spirit = a.spirit * b.spirit
      charisma = a.charisma * b.charisma
      insight = a.insight * b.insight
      history = a.history * b.history
      survival = a.survival * b.survival
    }
  static member (*) (a: Stats, scale: float) =
    { strength = a.strength * scale
      agility = a.agility * scale
      stamina = a.stamina * scale
      intellect = a.intellect * scale
      spirit = a.spirit * scale
      charisma = a.charisma * scale
      insight = a.insight * scale
      history = a.history * scale
      survival = a.survival * scale
    }

  static member (+) (a: Stats, b: Stats) =
    { strength = a.strength + b.strength
      agility = a.agility + b.agility
      stamina = a.stamina + b.stamina
      intellect = a.intellect + b.intellect
      spirit = a.spirit + b.spirit
      charisma = a.charisma + b.charisma
      insight = a.insight + b.insight
      history = a.history + b.history
      survival = a.survival + b.survival
    }