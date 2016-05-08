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
  with
    member this.Next () =
      ProfessionTier.Next this

    static member Next tier =
      match tier with
      | Novice -> Journeyman
      | Journeyman -> Expert
      | Expert -> Artisan
      | Artisan -> Master
      | Master -> Master

type RecipeRequirement =
  | LevelRecipeRequirement of level:int
  | ParentRecipeRequirement of recipeId:string
  | ProfessionRequirement of Profession
  | TierRequirement of ProfessionTier

type Recipe = {
  id: string
  name: string
  craftedItemId: string
  xpReward: float
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
  levelMax: int
  tier: ProfessionTier
  recipes: Recipe array
  stats: ArtisanStats
}
