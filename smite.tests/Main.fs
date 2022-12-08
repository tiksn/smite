module smite.tests

open Expecto

[<EntryPoint>]
let main args =
    Tests.runTestsInAssembly defaultConfig args
