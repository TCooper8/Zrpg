namespace Zrpg

open Microsoft.VisualStudio.TestTools.UnitTesting
open System
open System.IO
open System.Diagnostics
open Logging

[<TestClass>]
type AuthTest () =
  let log = StreamLogger("test", LogLevel.Debug, Console.OpenStandardOutput())

  let auth =
    Auth.createLocal <| log.Fork("Auth", log.Level)
    |> Async.Catch
    |> Async.RunSynchronously
    |> fun c ->
      match c with
      | Choice2Of2 e -> raise e
      | Choice1Of2 auth -> auth

  let record: Auth.RegisterUserRecord = {
    username = "testUsername"
    password = "testPassword"
    birthdate = DateTime.Now
    screenname = "testScreenname"
  }

  [<TestMethod>]
  member this.register () =
    auth.register record
    |> Async.RunSynchronously
    |> Option.iter (fun e -> raise e)

  [<TestMethod>]
  member this.verify () =
    this.register()
    auth.verify (record.username, record.password)
    |> Async.RunSynchronously
    |> Option.iter (fun e -> raise e)

  [<TestMethod>]
  member this.deregister () =
    this.register()
    auth.deregister record.username
    |> Async.RunSynchronously
    |> Option.iter (fun e -> raise e)

  [<TestMethod>]
  member this.noVerify () =
    auth.verify (record.username, record.password)
    |> Async.RunSynchronously
    |> fun o ->
      match o with
      | None -> failwith "User credentials were verified after user was deregistered"
      | _ -> ()