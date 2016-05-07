namespace Zrpg.Game

type MaterialCost = {
  itemId: string
  quantity: int
}

type Profession =
  | Blacksmith

type ProfessionTier =
  | Novice
  | Journeyman
  | Expert
  | Artisan
  | Master

type RecipeRequirement =
  | LevelRecipeRequirement of level:int
  | ParentRecipeRequirement of recipeId:string
  | ProfessionRequirement of Profession
  | TierRequirement of ProfessionTier

type Recipe = {
  id: string
  skillReward: int
  materialCosts: MaterialCost array
  requirements: RecipeRequirement array
  tags: string array
}

type ArtisanStats = {
  xp: float
  finalXp: float
}

type Artisan = {
  id: string
  name: string
  profession: Profession
  level: int
  tier: ProfessionTier
  recipes: Recipe array
  stats: ArtisanStats
}
