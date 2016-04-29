namespace Zrpg.Commons

open System
open System.Threading
open System.Collections
open System.Collections.Generic
open System.Collections.Concurrent

open Zrpg.Commons.Bundle

module Platform =
  type private Msg =
    | PreStart
    | PostStop
    | PreRestart of exn
    | Start
    | Stop
    | Receive of obj * Bundle.IBundleRef option
    | Become of IBundle
    | Kill

  type private BundleProxy (props: Props, bundle: Bundle.IBundle, platform:Bundle.IPlatform) =
    let mutable bundle: IBundle = bundle

    member val Props = props

    member this.Receive (cmd, sender) =
      bundle.Receive(cmd, sender)
      //try
      //  bundle.Receive(cmd, sender)
      //with e ->
      //  platform.DeadLetter.Send(cmd, sender)

    member this.PreStart () =
      bundle.PreStart()
      
    member this.PostStop () =
      bundle.PostStop()

    member this.PreRestart (e, context) =
      bundle.PreRestart(e, context)

    member this.Start context =
      bundle.Start context

    member this.Stop context =
      bundle.Stop context

    member this.Id = bundle.Id

    member this.Become (_bundle:Bundle.IBundle) =
      bundle <- _bundle

  type private BundleRef (props: Props, token: CancellationToken, queue: Msg ConcurrentQueue) =
    let rec send (msg, sender, backoff) =
      if token.IsCancellationRequested then
        failwith "Bundle has been killed"

      match props.queueThreshold with
      | Some max when queue.Count >= max ->
        Async.Sleep backoff |> Async.RunSynchronously
        send(msg, sender, backoff * 2)
      | _ ->
        Receive (msg, sender) |> queue.Enqueue

    interface Bundle.IBundleRef with
      member this.Send (msg, sender) = send (msg, sender, 8)

  type private DeadBundle () =
    interface Bundle.IBundleRef with
      member this.Send (msg, sender) =
        printfn "DeadBundle : %A" msg

  type private Context (platform:IPlatform, props:Props, queue:Msg ConcurrentQueue) =
    let mutable store = Map.empty<string, string>
    let mutable proxy: BundleProxy option = None

    member this.SetProxy _proxy =
      proxy <- Some _proxy

    interface IContext with
      member val Platform = platform

      member val Props = props

      member this.Put(key, value) =
        store <- store.Add(key, value)

      member this.Get key =
        store.TryFind key

      member this.Become bundle =
        proxy.Value.Become(bundle)
        //Become bundle |> queue.Enqueue

  type private Cell = {
    id: string
    props: Props
    tokSource: CancellationTokenSource
    proxy: BundleProxy
    context: Context
    queue: Msg ConcurrentQueue
  }

  type private CellWorker (queue: Cell ConcurrentQueue, deadBundle:IBundleRef) =
    let tokSource = new CancellationTokenSource()
    let token = tokSource.Token

    // Will attempt to consume a message for this cell.
    let handleCell (cell:Cell) =
      let queue = cell.queue
      let proxy = cell.proxy
      let context = cell.context
      let tokSource = cell.tokSource
      let ok, msg = queue.TryDequeue()

      if not ok then ()
      else
        try
          match msg with
          | PreStart -> proxy.PreStart()
          | PostStop -> proxy.PostStop()
          | PreRestart reason -> proxy.PreRestart(reason, context)
          | Start -> proxy.Start context
          | Stop -> proxy.Stop context
          | Receive (msg, sender) ->
            proxy.Receive(msg, defaultArg sender deadBundle)
          | Become bundle -> proxy.Become(bundle)
          | Kill -> ()
        with e ->
          // Failure? Shut down this bundle.
          try
            proxy.PreRestart (e, context)
          with
          | :? FSharp.Core.MatchFailureException as e ->
            deadBundle.Send e
          | e ->
            tokSource.Cancel()
            sprintf "Shut down bundle %s with error: %A" proxy.Id e
            |> deadBundle.Send
          ()

    let task =
      async {
        while not token.IsCancellationRequested do
          // Consume all of the cells on the queue.
          let ok, cell = queue.TryDequeue()
          if not ok then
            do! Async.Sleep 16
          else
            if cell.tokSource.IsCancellationRequested then
              ()
            else
              handleCell cell
      } |> Async.StartAsTask

    member this.Queue = queue
    member this.Task = task
    member this.TokenSource = tokSource

  type private Platform () =
    let objLock = new Object()
    let bundles = Dictionary<string, BundleProxy>()
    let queues = Dictionary<string, Msg ConcurrentQueue>()
    let tokens = Dictionary<string, CancellationTokenSource>()

    let deadBundle = DeadBundle() :> Bundle.IBundleRef

    let cellsLock = new Object()
    let activeCells = Map.empty<string, Cell> |> ref
    let workQueue = new ConcurrentQueue<Cell>()
    let cellWorkers = [
      for i in 1 .. 1 do
        yield CellWorker (workQueue, deadBundle)
    ]

    let task =
      let rec loop (): unit Async = async {
        let cells = !activeCells
        let nCells = cells.Count
        let mutable toRemove = Set.empty<string>

        if workQueue.Count > nCells * cellWorkers.Length then
          do! Async.Sleep 16
          return! loop()
        else

        // Need to distribute this load to the workers.
        for pair in cells do
          let id = pair.Key
          let cell = pair.Value

          if cell.tokSource.IsCancellationRequested then
            toRemove <- toRemove.Add id
          else
            workQueue.Enqueue (cell)

        if not toRemove.IsEmpty then
          lock cellsLock (fun () ->
            activeCells := !activeCells |> Map.filter (fun key _ -> toRemove.Contains key |> not)
          )

        for worker in cellWorkers do
          if worker.Task.IsFaulted then
            printfn "Worker has faulted!"

        return! loop ()
      }
      loop () |> Async.StartAsTask

    let start (id: string, props: Props, source:CancellationTokenSource, proxy:BundleProxy, context:Context, queue:Msg ConcurrentQueue): unit =
      let cell = {
        id = id
        props = props
        tokSource = source
        proxy = proxy
        context = context
        queue = queue
      }
      lock cellsLock (fun () ->
        activeCells := !activeCells |> Map.add id cell
      )
      //async {
      //  let token = source.Token

      //  while not token.IsCancellationRequested do
      //    let ok, msg = queue.TryDequeue()
      //    if ok then
      //      try
      //        match msg with
      //        | PreStart -> proxy.PreStart()
      //        | PostStop -> proxy.PostStop()
      //        | PreRestart reason -> proxy.PreRestart(reason, context)
      //        | Start -> proxy.Start context
      //        | Stop -> proxy.Stop context
      //        | Receive (msg, sender) ->
      //          proxy.Receive(msg, defaultArg sender deadBundle)
      //        | Become bundle -> proxy.Become(bundle)
      //        | Kill ->
      //          return ()
      //      with e ->
      //        // Failure? Shut down this bundle.
      //        try
      //          proxy.PreRestart (e, context)
      //        with
      //        | :? FSharp.Core.MatchFailureException as e ->
      //          deadBundle.Send e
      //        | e ->
      //          source.Cancel()
      //          sprintf "Shut down bundle %s with error: %A" proxy.Id e
      //          |> deadBundle.Send
      //          return ()
      //    else
      //      do! Async.Sleep 16
      //} |> Async.Start

    let defaultProps = {
      queueThreshold = Some 1024
    }

    interface Bundle.IPlatform with
      member this.DeadLetter = deadBundle

      member this.Lookup id =
        lock objLock (fun () ->
          let okA, proxy = bundles.TryGetValue(id)
          let okB, queue = queues.TryGetValue(id)
          let okC, tokSource = tokens.TryGetValue(id)

          if okA && okB && okC then
            let token = tokSource.Token
            let bundle = BundleRef (proxy.Props, token, queue) :> Bundle.IBundleRef
            Some bundle
          else None
        )

      member this.Register (bundle, ?props) =
        let props = defaultArg props defaultProps

        // Create a new context.
        let queue = ConcurrentQueue<Msg>()
        let context = Context(this, props, queue)

        let tokSource = new CancellationTokenSource()
        let token = tokSource.Token

        lock objLock (fun () ->
          match bundles.TryGetValue bundle.Id with
          | true, proxy ->
            // Need to swap the existing bundle with the new one.
            Become bundle |> queue.Enqueue
            ()
          | false, _ ->
            let proxy = BundleProxy(props, bundle, this)
            context.SetProxy(proxy)

            printfn "Adding bundle %s" bundle.Id
            bundles.Add(bundle.Id, proxy)
            queues.Add(bundle.Id, queue)
            tokens.Add(bundle.Id, tokSource)

            // Start the proxy lifecycle.
            start (bundle.Id, props, tokSource, proxy, context, queue)
            
            PreStart |> queue.Enqueue
            Start |> queue.Enqueue
        )

      member this.Deregister id =
        lock objLock (fun () ->
          match queues.TryGetValue id with
          | true, queue ->
            Kill |> queue.Enqueue
          | _ -> ()
          bundles.Remove(id) |> ignore
          queues.Remove(id) |> ignore
          tokens.Remove(id) |> ignore
        )

      member this.BundleIds =
        [ for pair in bundles do yield pair.Key ]

  let private platformsLock = new Object()
  let mutable private platforms = Map.empty<string, IPlatform>

  //do
    //let repl = CommandLine.create ()
    //platform.Register repl

  let create id =
    lock platformsLock (fun () ->
      match platforms.TryFind id with
      | None ->
        let platform = Platform()
        platforms <- platforms.Add(id, platform)
        platform :> IPlatform
      | Some platform ->
        platform
    )
