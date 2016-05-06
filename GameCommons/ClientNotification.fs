namespace Zrpg.Game

open System

type NotifyItem =
  | NotifyQuestCompleted of NotifyQuestCompleted

and NotifyQuestCompleted = {
  questId: string
  messageTitle: string
  messageBody: string
}

type NotifyRecord = {
  id: string
  item: NotifyItem
  timestamp: DateTime
}