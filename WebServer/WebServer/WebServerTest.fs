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
  let log = new Logging.StreamLogger("Test", Logging.LogLevel.Debug, Console.OpenStandardOutput())
  let server = new WebServer.Server(log)

  let staticDir = Path.Combine(Directory.GetCurrentDirectory(), "static")
  let defaultFile = "index.html"
  let fileData = "Hello!"

  let enc = new UTF8Encoding()

  [<TestInitialize>]
  member this.init () =
    log.Debug <| "Initializing test..."
    // Create the directory.
    Directory.CreateDirectory staticDir |> ignore
    // Create the default file.
    use fileStream = File.Create <| Path.Combine(staticDir, defaultFile)
    fileStream.Write(
      enc.GetBytes fileData,
      0,
      fileData.Length
    )

    WebServer.FileLoader.serveStatic
    <| staticDir 
    <| Some defaultFile
    |> server.get

    server.listen "localhost" 8080us |> Async.Start |> ignore

  [<TestCleanup>]
  member this.cleanup () =
    File.Delete <| Path.Combine(staticDir, defaultFile)
    Directory.Delete staticDir
    log.Debug <| "Shutting down logger"
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