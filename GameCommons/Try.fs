namespace Zrpg.Game

open System

type 'a Try =
  | Success of 'a
  | Failure of exn

module Try =
  let bind fn maybe =
    match maybe with
    | Success a ->
      try fn a
      with e -> Failure e
    | f -> f

  let map fn maybe =
    match maybe with
    | Success a ->
      try fn a |> Success
      with e -> Failure e
    | f -> f

  let failwith msg =
    msg |> Exception |> Failure