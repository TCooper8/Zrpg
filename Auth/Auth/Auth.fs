#nowarn "0058"

namespace Zrpg

open System
open System.IO
open System.Security.Cryptography
open System.Diagnostics
open System.Text

open Logging
open Pario.WebServer
open Zrpg.Discovery

module Auth =
  type Username = string
  type Screenname = string
  type Password = string

  type private UserRegistry = {
    username: Username
    passwordHash: Password
    passwordSalt: string
    birthdate: DateTime
    screenname: Screenname
  }

  type RegisterUserRecord = {
    username: Username
    password: Password
    birthdate: DateTime
    screenname: Screenname
  }

  type private Msg =
    | Verify of Username * Password * AsyncReplyChannel<exn option>
    | Register of RegisterUserRecord * AsyncReplyChannel<exn option>
    | Deregister of Username * AsyncReplyChannel<exn option>

  type private LocalAuthReceive (log:Logger) =
    let enc = Encoding.UTF8
    let mutable userStore = Map.empty<string, UserRegistry>
    
    member this.receive =
      function 
      | Verify(username, password, reply) ->
        log.Debug <| sprintf "Verying user %s" username
        try
          log.Debug <| sprintf "Verifying user %s" username

          // Retrieve the record and check if the password matches up.
          let res = match userStore.TryFind username with
          | None -> "User does not exist" |> Exception |> Some
          | Some reg ->
            use hashObj = new Rfc2898DeriveBytes(
              password,
              reg.passwordSalt |> enc.GetBytes,
              4096
            )
            let passHash = hashObj.ToString()

            if passHash <> reg.passwordHash then
              log.Debug <| sprintf "User %s is invalid" username
              "User credentials are invalid." |> Exception |> Some
            else
              log.Debug <| sprintf "User %s is valid" username
              None

          reply.Reply(res)
        with e -> reply.Reply(Some e)

      | Register(record, reply) ->
        use crypto = new Rfc2898DeriveBytes(
          record.password,
          512,
          4096
        )
        let passwordHash = crypto.ToString()
        let passwordSalt = crypto.Salt |> enc.GetString

        let registry = {
          username = record.username
          passwordHash = passwordHash
          passwordSalt = passwordSalt
          birthdate = record.birthdate
          screenname = record.screenname
        }

        let res = match userStore.TryFind(record.username) with
        | Some _ ->
          "User already registered" |> Exception |> Some
          
        | None ->
          // Insert the new registry.
          userStore <- userStore.Add(record.username, registry)
          None

        reply.Reply(res)

      | Deregister(username, reply) ->
        let res = userStore.TryFind username
        userStore <- userStore.Remove username

        let res = match res with
        | None -> "User does not exist" |> Exception |> Some
        | _ -> None

        reply.Reply(res)

  [<Interface>]
  type IAuthService =
    abstract verify: Username * Password -> exn option Async
    abstract register: RegisterUserRecord -> exn option Async
    abstract deregister: Username -> exn option Async

  type private AuthService (agent:MailboxProcessor<Msg>) =
    interface IAuthService with
      member this.verify (username, password) =
        agent.PostAndAsyncReply(fun reply -> Verify(username, password, reply))

      member this.register record =
        agent.PostAndAsyncReply(fun reply -> Register(record, reply))

      member this.deregister username =
        agent.PostAndAsyncReply(fun reply -> Deregister(username, reply))

  let createLocal (log:Logger) = async {
    let agent = MailboxProcessor.Start(fun inbox ->
      let receive = LocalAuthReceive(log)

      let rec loop () = async {
        try
          let! msg = inbox.Receive()
          log.Debug <| sprintf "Received %A" msg
          receive.receive(msg)
        with e -> log.Warn <| sprintf "AgentError: %A" e

        return! loop()
      }
      loop()
    )

    return AuthService(agent) :> IAuthService
  }