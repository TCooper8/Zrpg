namespace Zrpg.Game

open System.Threading.Tasks

[<Interface>]
type IGameClient =
  abstract member AddGarrison :
    clientId:string *
    garrisonName:string *
    race:Race *
    faction:Faction
      -> AddGarrisonReply Task

    abstract member AddHero : AddHero -> AddHeroReply Task
    abstract member AddRegion : AddRegion -> AddRegionReply Task
    abstract member AddQuest : AddQuest -> AddQuestReply Task
    abstract member AddZone : AddZone -> AddZoneReply Task
    abstract member AddZoneAssetPositionInfo : AssetPositionInfo -> unit Task
    abstract member GetClientGarrison : clientId:string -> GetClientGarrisonReply Task
    abstract member GetGameTime : unit -> int Task
    abstract member GetHero : heroId:string -> GetHeroReply Task
    abstract member GetHeroArray : heroIds:string array -> GetHeroArrayReply Task
    abstract member GetHeroInventory : heroId:string -> GetHeroInventoryReply Task
    abstract member GetHeroQuest : heroId:string -> GetHeroQuestReply Task
    abstract member GetItem : recordId: string -> (ItemRecord * Item) Task
    abstract member GetRegion : heroId:string -> GetRegionReply Task
    abstract member GetZone : heroId:string -> GetZoneReply Task
    abstract member GetZoneAssetPositionInfo : zoneId:string -> AssetPositionInfo Task
    abstract member GetZoneQuests: zoneId:string -> Quest array Task
    abstract member HeroBeginQuest: heroId:string * questId:string -> HeroBeginQuestReply Task
    abstract member RemGarrison : garrisonId:string -> RemGarrisonReply Task
    abstract member SetStartingZone: race:Race * zoneId:string -> SetStartingZoneReply Task
