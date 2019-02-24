module CommonFeaturesTests

open Expecto
open TIKSN.smite.lib
open TIKSN.smite.lib

[<Tests>]
let tests =
  testList "CommonFeatures" [
    testCase "no common segment" <| fun _ ->
      let modelsArray = [||]
      let nsd = seq [{Namespace=[|"Microsoft"; "Graph"|]; Models=modelsArray} ;
                    {Namespace=[|"System"; "Collection"; "Generic"|]; Models=modelsArray} ;
                    {Namespace=[|"TIKSN"; "smite"; "lib"; "core"|]; Models=modelsArray}]
      let subject = CommonFeatures.getFilespaceDefinition(nsd)
      Expect.isTrue (subject = 0) "xxx"
  ]


