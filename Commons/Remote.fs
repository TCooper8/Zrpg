namespace Zrpg.Commons

open System
open System.IO
open System.Net
open System.Net.Sockets

module Remote =
  type private RemoteTcp (socket: Socket) =
    let stream = new NetworkStream(socket, false)
    do socket.NoDelay <- true

    let receive () = async {
      use reader = new StreamReader(stream)
      let! data = reader.ReadToEndAsync() |> Async.AwaitTask
      return 5
    }

    member this.Receive () =
      5