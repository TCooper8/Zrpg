namespace Zrpg.Commons

open System
open System.IO
open System.Text
open System.Globalization
open Zrpg.Commons.Bundle
open System.ComponentModel

open Microsoft.FSharp.Compiler.SourceCodeServices
open Microsoft.FSharp.Compiler.Interactive.Shell

open FSharp.Reflection
open FSharp.Reflection.FSharpReflectionExtensions

module CommandLine =
  type private Msg = 
    | Eval of string
    | LoadAssembly of string
    | LoadConsole

  type private REPLBundle (id) =
    inherit IBundle()

    let argv = [| "C:\\fsi.exe" |]
    let allArgs = Array.append argv [| "--noninteractive" |]

    let sbOut = new StringBuilder()
    let sbErr = new StringBuilder()
    let inStream = new StringReader("")
    let outStream = new StringWriter(sbOut)
    let errStream = new StringWriter(sbErr)

    let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
    let session = FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream)

    let loadAssembly asm =
      let res, warnings = session.EvalInteractionNonThrowing <| sprintf "#r \"%s\"" asm

      for w in warnings do
        printfn "Warning %s at %d,%d" w.Message w.StartLineAlternate w.StartColumn

      match res with
      | Choice1Of2 () -> printfn "Loaded %s" asm
      | Choice2Of2 e -> printfn "Failure %A" e

    let eval code =
      printfn "Evaluating %s" code
      let res, warnings = session.EvalInteractionNonThrowing code

      for w in warnings do
        printfn "Warning %s at %d,%d" w.Message w.StartLineAlternate w.StartColumn

      match res with
      | Choice1Of2 () -> ()
      | Choice2Of2 e -> printfn "Failure %A" e

    let mutable consoleRunning = false
    let start () =
      if consoleRunning then ()
      else

      loadAssembly "Commons.dll"

      let rec loop () = async {
        let mutable hasReturned = false
        let mutable seq = ""

        printf ">>> "

        while not hasReturned do
          let key = Console.ReadKey(true)

          match key.Key with
          | ConsoleKey.Enter ->
            hasReturned <- true
          | ConsoleKey.Backspace ->
            if seq.Length > 0 then
              seq <- seq.Substring(0, seq.Length - 1)
              Console.CursorLeft <- Console.CursorLeft - 1
              printf " "
              Console.CursorLeft <- Console.CursorLeft - 1

          | ConsoleKey.Tab ->
            let completions = session.GetCompletions seq
            printfn ""
            printfn "%A" completions
            printf ">>> %s" seq

          | _ ->
            seq <- seq + key.KeyChar.ToString()
            Console.Write(key.KeyChar)

        // Client returned.
        // Evaluate the expression.
        printfn ""

        if seq = "" then return! loop()
        else
        printfn "Running %s" seq

        let doEval = [
          "let"
          "open"
          "#"
        ]

        let res, warnings =
          if doEval |> List.tryFind (fun s -> seq.StartsWith(s)) |> Option.isSome then
          //if seq.StartsWith("let") then
            session.EvalInteractionNonThrowing seq
            |> fun (a, b) ->
              match a with
              | Choice1Of2 () -> Choice1Of2(None), b
              | Choice2Of2 e -> Choice2Of2 e, b
          else
            session.EvalExpressionNonThrowing seq

        for w in warnings do
          printfn "Warning %s at %d,%d" w.Message w.StartLineAlternate w.StartColumn

        match res with
        | Choice1Of2 (Some value) -> printfn "res = %A" value.ReflectionValue
        | Choice1Of2 None -> printfn "Got none"
        //| Choice1Of2 () -> ()
        | Choice2Of2 e -> printfn "Failure %A" e

        return! loop()
      }

      loop () |> Async.Start

    override this.Id = id

    override this.Receive (msg, sender) =
      match msg with
      | :? Msg as msg ->
        match msg with
        | Eval code -> eval code
        | LoadAssembly asm -> loadAssembly asm
        | LoadConsole -> start()
      ()

  type ReplRef (bundle:IBundleRef) =
    member this.Eval code =
      code |> Eval |> bundle.Send

    member this.LoadAssembly asm =
      asm |> LoadAssembly |> bundle.Send

    member this.LoadConsole () =
      LoadConsole |> bundle.Send

  let create (platform:IPlatform) id =
    match platform.Lookup id with
    | None ->
      let bundle = REPLBundle id :> IBundle
      platform.Register bundle
      platform.Lookup id
      |> Option.get
      |> ReplRef
    | Some ref ->
      ref |> ReplRef