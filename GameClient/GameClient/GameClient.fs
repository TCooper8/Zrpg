namespace Zrpg.Game

open System
open System.IO
open System.Text
open System.Threading.Tasks
open System.Net

open Newtonsoft.Json

type ClientId = string

type private RestGameClient (endPoint) =
  let lockObj = new Object()
  let enc = Encoding.UTF8

  let request (msg:Msg) =
    lock lockObj (fun () ->
      async {
        let data = JsonConvert.SerializeObject(msg) |> enc.GetBytes
        let req = HttpWebRequest.Create (sprintf "%s/api" endPoint) :?> HttpWebRequest

        req.Method <- "POST"
        req.ContentType <- "application/json"

        use! output = req.GetRequestStreamAsync() |> Async.AwaitTask
        do! output.AsyncWrite data
        output.Dispose()

        let! resp = req.AsyncGetResponse()
        let resp = resp :?> HttpWebResponse
        use resp = resp.GetResponseStream()
        use reader = new StreamReader(resp, enc)

        let! data = reader.ReadToEndAsync() |> Async.AwaitTask
        let reply = JsonConvert.DeserializeObject<Reply>(data)
        return reply
      }
    )

  interface IGameClient with
    member this.AddItem addItem =
      async {
        let! reply = request <| AddItem addItem
        let reply = match reply with
        | AddItemReply itemId -> itemId
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected AddItemReply but got %A" msg
        return reply
      } |> Async.StartAsTask
    
    member this.AddGarrison (clientId, garrisonName, race, faction) =
      async {
        let! reply = request <| AddGarrison {
          clientId = clientId
          name = garrisonName
          race = race
          faction = faction
        }

        let reply = match reply with
        | AddGarrisonReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected AddGarrisonReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.AddHero msg =
      async {
        let! reply = request <| AddHero msg

        let reply = match reply with
        | AddHeroReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected AddHeroReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.AddRegion region =
      async {
        let! reply = region |> AddRegion |> request
        let reply = match reply with
        | AddRegionReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected AddRegionReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.AddZoneAssetPositionInfo info =
      async {
        let! reply = info |> AddZoneAssetPositionInfo |> request
        let reply = match reply with
        | AddZoneAssetPositionInfoReply -> ()
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected AddZoneAssetPositionInfoReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.AddQuest quest =
      async {
        let! reply = quest |> AddQuest |> request
        let reply = match reply with
        | AddQuestReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected AddQuestReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.AddZone region =
      async {
        let! reply = region |> AddZone |> request
        let reply = match reply with
        | AddZoneReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected AddZoneReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.GetClientGarrison clientId =
      async {
        let! reply = request <| GetClientGarrison clientId
        let reply = match reply with
        | GetClientGarrisonReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected AddGarrisonReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.GetClientNotifications clientId =
      async {
        let! reply = request <| GetClientNotifications clientId
        let reply = match reply with
        | GetClientNotificationsReply notifications -> notifications
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected GetClientNotificationsReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.GetGameTime () =
      async {
        let! reply = request <| GetGameTime
        let reply = match reply with
        | GetGameTimeReply gameTime -> gameTime
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected GetGameTimeReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.GetHero heroId =
      async {
        let! reply = request <| GetHero heroId
        let reply = match reply with
        | GetHeroReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected AddGarrisonReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.GetHeroArray heroIds =
      async {
        let! reply = request <| GetHeroArray heroIds
        let reply = match reply with
        | GetHeroArrayReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected AddGarrisonReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.GetHeroInventory heroId = 
      async {
        let! reply = request <| GetHeroInventory heroId
        let reply = match reply with
        | GetHeroInventoryReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected GetHeroInventoryReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.GetHeroQuest heroId = 
      async {
        let! reply = request <| GetHeroQuest heroId
        let reply = match reply with
        | GetHeroQuestReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected GetHeroQuestReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.GetItem recordId = 
      async {
        let! reply = request <| GetItem recordId
        let reply = match reply with
        | GetItemReply (record, item) -> record, item
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected GetItemReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.GetRegion heroId = 
      async {
        let! reply = request <| GetRegion heroId
        let reply = match reply with
        | GetRegionReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected GetRegionReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.GetZone id = 
      async {
        let! reply = request <| GetZone id
        let reply = match reply with
        | GetZoneReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected GetZoneReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.GetZoneAssetPositionInfo id = 
      async {
        let! reply = request <| GetZoneAssetPositionInfo id
        let reply = match reply with
        | GetZoneAssetPositionInfoReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected GetZoneAssetPositionInfoReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.GetZoneQuests id = 
      async {
        let! reply = request <| GetZoneQuests id
        let reply = match reply with
        | GetZoneQuestsReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected GetZoneQuestsReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.HeroBeginQuest (heroId, questId) =
      async {
        let! reply = request <| HeroBeginQuest(heroId, questId)
        let reply = match reply with
        | HeroBeginQuestReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected HeroBeginQuestReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.RemGarrison heroIds =
      async {
        let! reply = request <| RemGarrison heroIds
        let reply = match reply with
        | RemGarrisonReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected AddGarrisonReply but got %A" msg

        return reply
      } |> Async.StartAsTask

    member this.SetStartingZone (race, zoneId) =
      async {
        let! reply = request <| SetStartingZone(race, zoneId)
        let reply = match reply with
        | SetStartingZoneReply reply -> reply
        | ExnReply msg -> failwith msg
        | msg -> failwith <| sprintf "Expected SetStartingZoneReply but got %A" msg

        return reply
      } |> Async.StartAsTask

module GameClient =
  let RESTClient endPoint =
    RestGameClient endPoint :> IGameClient