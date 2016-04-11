namespace Pario

open System
open System.IO
open System.Net
open System.Text
open System.Text.RegularExpressions
open System.Diagnostics

module WebServer =
  type Handler = HttpListenerRequest -> HttpListenerResponse -> Async<bool>

  type ServerModule = {
    handler: Handler
    priority: int
  }

  module FileLoader =
    let serveStatic rootDirectory defaultFile =
      let handler: Handler = fun req resp -> async {
        // First, grab the path from the Uri.
        let uri = req.Url

        let pathName = 
          let path = uri.LocalPath
          if path.StartsWith("/") then
            path.Substring(1)
          else
            path

        let filePath = Path.Combine(
          rootDirectory,
          pathName
        )

        printfn "Loading file %s" filePath

        let fileStream = match IO.File.openRead filePath with
        | Success stream -> Some stream
        | _ ->
          match defaultFile with
          | Some filePath ->
            let filePath = Path.Combine(rootDirectory, filePath)

            match IO.File.openRead filePath with
            | Success stream -> Some stream
            | _ -> None
          | _ -> None

        if fileStream.IsNone then
          return false
        else

        use fileStream = fileStream.Value
        use respStream = resp.OutputStream

        do! fileStream.CopyToAsync respStream |> Async.AwaitVoidTask
        return true
      }

      { handler = handler
        priority = 0
      }

  type Work =
    | Handle of HttpListenerRequest * HttpListenerResponse
    | SetModules of ServerModule list

  type private ServerState () =
    let mutable modules = List.empty<ServerModule>
    let mutable workers = List.empty<MailboxProcessor<Work>>

    member this.addModule serverModule =
      modules <- 
        fun m -> m.priority
        |> List.sortBy
        <| serverModule::modules
      modules

    member this.setWorkers newWorkers =
      workers <- newWorkers

    member this.getWorkers () =
      workers

    member this.getModules () =
      modules

  type Msg =
    | AddModule of ServerModule
    | SetWorkers of List<MailboxProcessor<Work>>
    | RouteWork of Work
    | Kill

  type Server () =
    let rand = new Random()

    let agent = MailboxProcessor.Start(fun inbox ->
      let rec loop (state:ServerState): Async<unit> = async {
        let! msg = inbox.Receive()

        match msg with
        | AddModule serverModule ->
          let modules = state.addModule serverModule
          let workers = state.getWorkers()

          for worker in workers do
            worker.Post <| SetModules modules

        | RouteWork work ->
          let workers = state.getWorkers()
          if workers.IsEmpty |> not then
            let workers = workers |> List.sortBy (fun worker -> worker.CurrentQueueLength)
            let worker = workers.[0]
            worker.Post work

        | SetWorkers workers ->
          state.setWorkers workers

        | Kill ->
          return ()

        return! loop state
      }

      loop <| new ServerState()
    )

    do 
      [ 0 .. 8 ]
      |> List.map (fun i ->
        let worker = MailboxProcessor.Start(fun inbox ->
          let rec loop handlers = async {
            let! work = inbox.Receive()

            match work with
            | SetModules handlers ->
              return! loop handlers

            | Handle (req, resp) ->
              use resp = resp

              let url = req.Url

              let rec iter handlers = async {
                match handlers with
                | [] -> return false
                | handler::handlers ->
                  let! res = handler.handler req resp
                  if res then
                    return res
                  else
                    return! iter handlers
              }

              let! res = iter handlers
              if not res then
                Debug.WriteLine("Setting status code to 404")
                resp.StatusCode <- 404
              else () // The response was handled.

              // Will close the response.

            return! loop handlers
          }

          loop <| List.empty<ServerModule>
        )

        worker
      )
      |> SetWorkers
      |> agent.Post

    member this.listen host port =
      let listener = new HttpListener()

      listener.Prefixes.Add <| sprintf "http://%s:%i/" host port
      listener.Start()

      let task = Async.FromBeginEnd(
        listener.BeginGetContext,
        listener.EndGetContext
      )
      async {
        while true do
          let! context = task
          RouteWork <| Handle(context.Request, context.Response) |> agent.Post
      }

    member this.handle serverModule =
      agent.Post <| AddModule serverModule

    member this.get serverModule =
      // We need to only handle this on HTTP 
      let _serverModule = {
        handler = fun req resp -> async {
          if req.HttpMethod = "GET" then
            return! serverModule.handler req resp
          else
            return false
        }
        priority = serverModule.priority
      }
      agent.Post <| AddModule _serverModule