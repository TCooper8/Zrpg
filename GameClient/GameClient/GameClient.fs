namespace Zrpg.Game

open System
open System.IO
open System.Text
open System.Threading.Tasks
open System.Net

open Newtonsoft.Json

type ClientId = string

type private RestGameClient (endPoint) =
  let enc = Encoding.UTF8

  let request (msg:Msg) =
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

  interface IGameClient with
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