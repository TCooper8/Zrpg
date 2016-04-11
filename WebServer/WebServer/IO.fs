namespace Pario

open System.IO
open System.Threading.Tasks

module Async =
  let AwaitVoidTask: (Task -> Async<unit>) =
    Async.AwaitIAsyncResult >> Async.Ignore

module IO =
  module File =
    let openRead path =
      Try.apply <| fun () -> File.OpenRead path