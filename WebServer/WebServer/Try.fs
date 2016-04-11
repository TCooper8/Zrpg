namespace Pario

type 'a Try =
  | Success of 'a
  | Failure of exn
with
  member this.get () =
    match this with
    | Success v -> v
    | Failure e -> raise e

  member this.isSuccess () =
    match this with
    | Success _ -> true
    | _ -> false

  member this.isFailure () =
    match this with
    | Failure _ -> true
    | _ -> false

module Try =
  let apply f =
    try f() |> Success
    with e -> e |> Failure
