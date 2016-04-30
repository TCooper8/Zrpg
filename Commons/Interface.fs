namespace Zrpg.Commons

open System
open System.Threading

module Bundle =

  [<AbstractClass>]
  type IBundle () =
    abstract member PreStart: unit -> unit
    abstract member PostStop: unit -> unit
    abstract member PreRestart: e:exn * context:IContext -> unit
    abstract member Start: IContext -> unit
    abstract member Stop: IContext -> unit
    abstract member Id: string

    default this.PreStart () = ()
    default this.PostStop () = ()
    default this.PreRestart (e:exn, context:IContext) = raise e
    default this.Start (context:IContext) = ()
    default this.Stop (context:IContext) = ()
    default this.Id = Guid.NewGuid().ToString()

    abstract Receive: value:Object * ?sender:IBundleRef -> unit

  and IContext =
    abstract member Platform: IPlatform
    abstract member Props: Props
    abstract member Put: key:string * value:string -> unit
    abstract member Get: key:string -> string option
    abstract member Become: IBundle -> unit

  and Props = {
    queueThreshold: int option
  }
  and IBundleRef =
    abstract Send: value:Object * ?sender:IBundleRef -> unit

  and IPlatform =
    abstract Lookup: id:string -> IBundleRef option
    abstract Register: bundle:IBundle * ?props:Props -> unit
    abstract Deregister: id:string -> unit
    abstract DeadLetter: IBundleRef

    abstract BundleIds: string list

  type 'a Chan () =
    let mut = new AutoResetEvent(false)
    let mutable res: Choice<'a, exn> = Choice2Of2 <| Exception "Not defined"
    let res = Choice2Of2 <| Exception "Not defined" |> ref

    let task =
      async {
        mut.WaitOne() |> ignore

        return !res
      } |> Async.StartAsTask

    interface IBundleRef with
      member this.Send (msg, sender) =
        match msg with
        | :? 'a as msg ->
          res := Choice1Of2 msg
        | reply ->
          res := sprintf "Wrong reply type %A" reply |> Exception |> Choice2Of2

        mut.Set() |> ignore

    member this.Await (?timeout: int) = async {
      let! res = task |> Async.AwaitTask
      return res
    }