namespace Zrpg.Commons

open System
open System.IO

open Logging
open Zrpg.Commons.Bundle

module LogBundle =
  type Msg =
    | Log of name:string * LogLevel * msg:string
    | ListLogs of limit:int

  type private LogBundle (id) =
    inherit IBundle()

    let stream = new MemoryStream()
    let reader = new StreamReader(stream)

    let mutable logs = Map.empty<string, Logging.Logger>

    //let log = new Logging.StreamLogger("Log", LogLevel.Debug, Console.OpenStandardOutput())

    let pullNLogs n =
      let rec loop i acc = async {
        if i >= n then
          return acc
        else
          let! msg = reader.ReadLineAsync() |> Async.AwaitTask
          return! loop (i + 1) (msg::acc)
      }
      loop 0 []

    let handle (msg, sender:IBundleRef option) =
      match msg with
      | Log (name, level, msg) ->
        let log =
          match logs.TryFind name with
          | None -> new Logging.StreamLogger(name, LogLevel.Debug, Console.OpenStandardOutput()) :> Logging.Logger
          | Some log -> log
        log.Log(level, msg)

      | ListLogs limit ->
        sender |> Option.iter (fun sender ->
          pullNLogs limit
          |> Async.RunSynchronously
          |> fun res -> sender.Send res
        )

    override this.Stop context =
      logs |> Map.iter (fun _ log -> log.Dispose())

    override this.Id = id

    override this.Receive (msg, sender) =
      match msg with
      | :? Msg as msg ->
        handle (msg, sender)
      ()

  type Log (name, bundle:IBundleRef) =
    let log level msg = bundle.Send <| Msg.Log(name, level, msg)

    member this.Info = log LogLevel.Info
    member this.Debug = log LogLevel.Debug
    member this.Warn = log LogLevel.Warn
    member this.Error = log LogLevel.Error
  
  let log (platform:IPlatform) id =
    match platform.Lookup id with
    | None ->
      let log = LogBundle id
      platform.Register log
      platform.Lookup id
      |> Option.get
      |> fun bundle -> Log(id, bundle)
    | Some log ->
      log
      |> fun bundle -> Log(id, bundle)