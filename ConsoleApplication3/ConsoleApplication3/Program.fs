[<EntryPoint>]
let main argv = 
  let f a b = a + b
  printfn "Res = %A" <| f 1 2
  0