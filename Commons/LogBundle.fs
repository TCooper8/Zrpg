namespace Zrpg.Commons

open System
open System.IO

open Logging
open Zrpg.Commons.Bundle

module LogBundle =
  type private Msg =
    | Log of LogLevel * string

  let private Id = "log"

  type private LogBundle () =
    inherit IBundle()

    let stream = new MemoryStream()
    let reader = new StreamReader(stream)

    let log = new Logging.StreamLogger("Log", LogLevel.Debug, stream)

    let handle (msg, sender) =
      match msg with
      | Log (level, msg) ->
        log.Log(level, msg)

    override this.Stop context =
      log.Dispose()

    override this.Id = Id

    override this.Receive (msg, sender) =
      ()

  type Log (bundle:IBundleRef) =
    let log level msg = bundle.Send <| Msg.Log(level, msg)
    //let log level = Printf.ksprintf (fun res -> log.Debug (res))

    member this.Info = log LogLevel.Info
    member this.Debug = log LogLevel.Debug
    member this.Warn = log LogLevel.Warn
    member this.Error = log LogLevel.Error

  let private bundle = LogBundle()
  let private platform = Platform.createPlatform()
  do platform.Register bundle
  
  let log () =
    platform.Lookup Id |> Option.get |> Log