namespace Zrpg.Game

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.Diagnostics
open System.IO
open System.Text
open System.Net
open Newtonsoft.Json

open Zrpg.Game.GameServer

[<TestClass>]
type TestGameServer () =
  let server = new Zrpg.Game.GameServer.GameServer()

  let enc = Encoding.UTF8

  [<TestInitialize>]
  member this.init () =
    async {
      let! res = server.Listen "localhost" 8080 |> Async.Catch
      match res with
      | Choice2Of2 e ->
        Debug.WriteLine(sprintf "%A" e)
      | _ -> ()
    } |> Async.Start

  [<TestMethod>]
  member this.testAddCharacter () =
    let cmd: AddCharacter = {
      clientId = "testId"
      name = "bob"
      race = Race.Human
      gender = Gender.Male
      classType = ClassType.Warrior
    }
    let data = JsonConvert.SerializeObject(cmd) |> enc.GetBytes

    let req = HttpWebRequest.Create "http://localhost:8080/addCharacter" :?> HttpWebRequest

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
          let resp = e.Response :?> HttpWebResponse
          failwith <| sprintf "%A : %A" resp.StatusCode resp.StatusDescription
        | e -> failwith <| e.Message

    use resp = resp.GetResponseStream()
    use resp = new StreamReader(resp, enc)
    let data = resp.ReadToEnd()

    let expectedData = "Character created!"
    match data = expectedData with
    | true -> ()
    | _ ->
      failwith
      <| sprintf "Data %s does not match %s" data expectedData
    ()
     