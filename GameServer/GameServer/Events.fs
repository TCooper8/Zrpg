namespace Zrpg.Game

type Dialogue = {
  id: string
  message: string
  responses: string Set
  requirements: string Set
}

type Quest = {
  id: string
  message: string
  responses: string Set
}

type Combat = {
  id: string
  enemies: string Set
}

type Objective = {
  id: string
  links: string Set
}

type Event =
  | DialogueEvent of Dialogue
  | QuestEvent of Quest
  | ObjectiveEvent of Objective