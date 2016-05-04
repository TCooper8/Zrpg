namespace Zrpg.Game

type ClientNotification =
  | NotifyQuestCompleted of NotifyQuestCompleted

and NotifyQuestCompleted = {
  questId: string
  finishTime: int
  messageTitle: string
  messageBody: string
}