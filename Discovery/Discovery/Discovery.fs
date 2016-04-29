namespace Zrpg

module Discovery =
  type ServiceName = string
  type ServiceId = string

  type HostInfo = {
    ipAddress: string
    port: uint16
    hostType: string
    status: string
  } with
    member this.id (): ServiceId =
      sprintf "%s://%s:%i/" this.hostType this.ipAddress this.port

  [<Interface>]
  type IDiscovery =
    abstract addServiceHost : ServiceName -> HostInfo -> Async<exn option>
    abstract remServiceHost : ServiceName -> ServiceId -> Async<exn option>
    abstract getHostsOf : ServiceName -> Async<HostInfo list>

  type private State () =
    let mutable addedServices = List.empty<string>

  type private Msg =
    | AddServiceHost of ServiceName * HostInfo * AsyncReplyChannel<exn option>
    | RemServiceHost of ServiceName * ServiceId * AsyncReplyChannel<exn option>
    | GetHostsOf of ServiceName * AsyncReplyChannel<Choice<string list, exn>>

  type Config = {
    redisEndPoint: string
  }

  type private Discovery (agent:MailboxProcessor<Msg>) =
    interface IDiscovery with
      member this.addServiceHost serviceName hostInfo =
        agent.PostAndAsyncReply(fun reply -> AddServiceHost(serviceName, hostInfo, reply))

      member this.remServiceHost serviceName serviceId =
        agent.PostAndAsyncReply(fun reply -> RemServiceHost(serviceName, serviceId, reply))

      member this.getHostsOf serviceName = async {
        let! resp = agent.PostAndAsyncReply(fun reply -> GetHostsOf(serviceName, reply))
        return []
      }

  type private LocalCache () =
    let mutable hostsByService = Map.empty<string, Map<string, HostInfo>>

    member this.addHost serviceName key hostInfo =
      let service = hostsByService |> Map.tryFind serviceName |> defaultArg <| Map.empty
      hostsByService <- hostsByService.Add(serviceName, service.Add(key, hostInfo))

    member this.remHost serviceName serviceId =
      match Map.tryFind serviceName hostsByService with
      | None -> ()
      | Some hosts ->
        // Remove the serviceId from the hosts map
        hostsByService <- hostsByService.Add(serviceName, Map.remove serviceId hosts)

    member this.getHosts serviceName =
      Map.tryFind serviceName hostsByService
      |> Option.map (Map.toList >> (List.map fst))
      |> fun o -> match o with
      | None -> failwith <| sprintf "Cannot find service %s" serviceName
      | Some hosts -> hosts

  let createLocal () = async {
    let agent = MailboxProcessor.Start(fun inbox ->
      let rec loop (cache:LocalCache) = async {
        let! msg = inbox.Receive()
        match msg with
        | AddServiceHost(serviceName, hostInfo, reply) ->
          let res =
            try
              cache.addHost serviceName (hostInfo.id()) hostInfo
              None
            with e -> Some e
          reply.Reply(res)

        | RemServiceHost(serviceName, serviceId, reply) ->
          let res =
            try
              cache.remHost serviceName serviceId
              None
            with e -> Some e
          reply.Reply(res)

        | GetHostsOf(serviceName, reply) ->
          let res =
            try
              let hosts = cache.getHosts serviceName
              Choice1Of2 hosts
            with e -> Choice2Of2 e
          reply.Reply(res)

        return! loop cache
      }

      async {
        while true do
          let! res = loop (LocalCache()) |> Async.Catch
          match res with
          | Choice2Of2 e ->
            printfn "AgentLoopFailure: %A" e
          | _ -> () // State is okay.
      }
    )

    return Discovery agent :> IDiscovery
  }