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
    abstract member GetClientGarrison : clientId:string -> GetClientGarrisonReply Task
    abstract member GetHero : heroId:string -> GetHeroReply Task
    abstract member GetHeroArray : heroIds:string array -> GetHeroArrayReply Task
    abstract member GetHeroInventory : heroId:string -> GetHeroInventoryReply Task
    abstract member AddRegion : AddRegion -> AddRegionReply Task
    abstract member AddZone : AddZone -> AddZoneReply Task
    abstract member RemGarrison : garrisonId:string -> RemGarrisonReply Task
    abstract member SetStartingZone: race:Race * zoneId:string -> SetStartingZoneReply Task
