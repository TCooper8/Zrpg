// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open Microsoft.VisualStudio.TestTools.UnitTesting

[<EntryPoint>]
let main argv =
  let test = Zrpg.Game.TestClass()
  test.Init()
  test.testHeroQuest()

  async {
    while true do
      do! Async.Sleep 1024
  } |> Async.RunSynchronously
  //Zrpg.Game.Test.web |> ignore
  printfn "%A" argv
  0 // return an integer exit code
