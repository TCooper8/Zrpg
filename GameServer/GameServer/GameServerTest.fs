namespace Zrpg.Game

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Diagnostics
open System.IO
open System.Text
open System.Net
open Newtonsoft.Json

open Zrpg.Game.GameServer
open Logging

[<TestClass>]
type TestGameServer () =
  let log = Logging.StreamLogger(
    "TestGameServer",
    LogLevel.Debug,
    Console.OpenStandardOutput()
  )

  do
    for i in 0 .. 100 do
      log.Debug <| sprintf "%i" i

  let server = new Zrpg.Game.GameServer.GameServer()

  let enc = Encoding.UTF8

  let msgServer (msg:Msg) =
    log.Info <| sprintf "Requesting %A" msg
    let data = JsonConvert.SerializeObject(msg) |> enc.GetBytes

    let req = HttpWebRequest.Create "http://localhost:8080/api" :?> HttpWebRequest
    req.Timeout <- 10000

    req.Method <- "POST"
    req.ContentType <- "application/json"
    req.ContentLength <- data.LongLength

    use output = req.GetRequestStream()
    output.Write(data, 0, data.Length)
    output.Close()

    // Now get the response.
    use resp =
      try req.GetResponse() :?> HttpWebResponse
      with
        | :? WebException as e ->
          if e.Response = null then
            failwith <| sprintf "%A" e

          let resp = e.Response :?> HttpWebResponse
          failwith <| sprintf "%A : %A" resp.StatusCode resp.StatusDescription
        | e -> failwith <| e.Message

    use resp = resp.GetResponseStream()
    use resp = new StreamReader(resp, enc)
    let data = resp.ReadToEnd()
    let reply = JsonConvert.DeserializeObject<Reply>(data)

    log.Info <| sprintf "Received response %A" reply

    reply

  [<TestInitialize>]
  member this.init () =
    async {
      let! res = server.Listen "localhost" 8080us |> Async.Catch
      match res with
      | Choice2Of2 e ->
        Debug.WriteLine(sprintf "%A" e)
      | _ -> ()
    } |> Async.Start

  [<TestMethod>]
  member this.testNewGarrison () =
    // New player. Going to add a new garrison.
    let msg = AddGarrison {
      clientId = "testClientId"
      name = "My garrison"
      race = Human
      faction = Alliance
    }

    let reply = msgServer msg
    match reply with
    | ExnReply e -> raise e
    | _ -> ()