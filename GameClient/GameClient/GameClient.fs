namespace Zrpg.Game

open System
open System.IO
open System.Text
open System.Threading.Tasks
open System.Net

open Newtonsoft.Json

type ClientId = string

[<Interface>]
type IGameClient =
  abstract member AddGarrison :
    clientId:string *
    garrisonName:string *
    race:Race *
    faction:Faction
      -> Reply Task

  abstract member GetClientGarrison : clientId:string -> Reply Task
  abstract member GetGarrison : garrisonId:string -> Reply Task
  abstract member RemGarrison : garrisonId:string -> Reply Task

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
    } |> Async.StartAsTask

  interface IGameClient with
    member this.AddGarrison (clientId, garrisonName, race, faction) = request <| AddGarrison {
      clientId = clientId
      name = garrisonName
      race = race
      faction = faction
    }

    member this.GetClientGarrison clientId = request <| GetClientGarrison clientId
    member this.GetGarrison garrisonId = request <| GetGarrison garrisonId
    member this.RemGarrison garrisonId = request <| RemGarrison garrisonId

module GameClient =
  let RESTClient endPoint =
    RestGameClient endPoint :> IGameClient