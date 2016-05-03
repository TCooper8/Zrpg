// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open System
open Zrpg.Commons

open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations

type Log =
  | Info
  | Report

type LoggingBundle (platform) as this =
  inherit Bundle.IBundle(platform)
  let mutable i = 0

  override this.Id = "log"

  override this.Receive (msg, sender) =
    match msg with
    | :? Log as msg ->
      match msg with
      | Report -> printfn "Msgs = %i" i
      | Info -> i <- i + 1

  override this.PreRestart (reason, context) =
    printfn "Got error: %A" reason
    //context.Become(AnyLoggingBundle(platform, props)
    AnyLoggingBundle (i, platform, context)
    |> context.Become

and AnyLoggingBundle (i, platform, context) =
  inherit Bundle.IBundle(platform)

  //let mutable i = i

  override this.Receive (msg, sender) =
    if i = 2000000 then
      printfn "%i" i
    //i <- i + 1
    AnyLoggingBundle (i + 1, platform, context)
    |> context.Become
    //i <- i + 1


[<EntryPoint>]
let main argv = 
  let platform = Platform.createPlatform()
  let bundle = LoggingBundle platform

  platform.Register(bundle, {
    queueThreshold = 1 <<< 20 |> int |> Some
  })

  let watch = System.Diagnostics.Stopwatch()
  let msgs = 1000
  let tasks = 100

  watch.Start()
  [ for i in 1 .. tasks do
    let log = platform.Lookup "log" |> Option.get
    log.Send "hi"

    yield async {
      for i in 0 .. msgs do
        Info |> log.Send
    }
  ]
  |> Async.Parallel
  |> Async.RunSynchronously
  |> ignore
  watch.Stop()

  let dt = watch.Elapsed.TotalSeconds
  let totalMsgs = msgs * tasks |> float
  let mps = totalMsgs / dt / 1000000.0
  printfn "Watch took %A seconds" dt
  printfn "%A mil mps" mps

  Console.ReadLine() |> ignore

  0 // return an integer exit code
