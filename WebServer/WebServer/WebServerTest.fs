namespace Test

open Microsoft.VisualStudio.TestTools.UnitTesting
open Pario

open System
open System.IO
open System.Text
open System.Net
open System.Diagnostics

[<TestClass>]
type Testrun () =
  let server = new WebServer.Server()

  let staticDir = Path.Combine(Directory.GetCurrentDirectory(), "static")
  let defaultFile = "index.html"
  let fileData = "Hello!"

  let enc = new UTF8Encoding()

  [<TestInitialize>]
  member this.init () =
    // Create the directory.
    Directory.CreateDirectory staticDir |> ignore
    // Create the default file.
    use fileStream = File.Create <| Path.Combine(staticDir, defaultFile)
    fileStream.Write(
      enc.GetBytes fileData,
      0,
      fileData.Length
    )

    server.handle {
      priority = 100
      handler = fun req resp -> async {
        let path = req.Url.AbsolutePath
        if path <> "/getChar" then
          Debug.WriteLine("Request handled false!")
          return false
        else

        return true
      }
    }

    //WebServer.FileLoader.serveStatic
    //<| staticDir 
    //<| Some defaultFile
    //|> server.get

    server.listen "localhost" 8080 |> Async.Start |> ignore

  [<TestCleanup>]
  member this.cleanup () =
    File.Delete <| Path.Combine(staticDir, defaultFile)
    Directory.Delete staticDir
    ()

  [<TestMethod>]
  member this.addModules () =
    let req = WebRequest.Create "http://localhost:8080/index.html"
    use resp = req.GetResponse() :?> HttpWebResponse

    use resp = resp.GetResponseStream()
    use resp = new StreamReader(resp, enc)
    let data = resp.ReadToEnd()

    match data = fileData with
    | true -> ()
    | _ ->
      failwith
      <| sprintf "Data %s does not match %s" data fileData
    ()
    