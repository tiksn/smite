module CommonFeaturesTests

open Expecto
open TIKSN.smite.lib
open TIKSN.smite.lib

[<Tests>]
let tests =
    testList "CommonFeatures"
        [ testCase "No common segment" <| fun _ ->
            let modelsArray = [||]

            let nsd =
                seq
                    [ { Namespace = [| "Microsoft"; "Graph" |]
                        Models = modelsArray }
                      { Namespace = [| "System"; "Collections"; "Generic" |]
                        Models = modelsArray }
                      { Namespace = [| "TIKSN"; "smite"; "lib"; "core" |]
                        Models = modelsArray } ]

            let subject = CommonFeatures.getFilespaceDefinitions (nsd)
            Expect.equal (nsd |> Seq.length) (subject |> Seq.length) "Count must be equal"
            let nsArray =
                nsd
                |> Seq.map (fun x -> x.Namespace)
                |> Seq.toArray

            let fsArray =
                subject
                |> Seq.map (fun x -> x.Filespace)
                |> Seq.toArray

            let itemCount = nsArray.Length
            for i = 0 to itemCount - 1 do
                for j = 0 to nsArray.[i].Length - 1 do
                    if i <> j then Expect.equal nsArray.[i].[j] fsArray.[i].[j] "Item values must be equal"

          testCase "One common segment" <| fun _ ->
              let modelsArray = [||]

              let nsd =
                  seq
                      [ { Namespace = [| "System" |]
                          Models = modelsArray }
                        { Namespace = [| "System"; "Collections" |]
                          Models = modelsArray }
                        { Namespace = [| "System"; "Collections"; "Generic" |]
                          Models = modelsArray }
                        { Namespace = [| "System"; "Collections"; "Specialized " |]
                          Models = modelsArray } ]

              let subject = CommonFeatures.getFilespaceDefinitions (nsd)
              Expect.equal (nsd |> Seq.length) (subject |> Seq.length) "Count must be equal"
              let nsArray =
                  nsd
                  |> Seq.map (fun x -> x.Namespace)
                  |> Seq.toArray

              let fsArray =
                  subject
                  |> Seq.map (fun x -> x.Filespace)
                  |> Seq.toArray

              let itemCount = nsArray.Length
              for i = 0 to itemCount - 1 do
                  for j = 0 to nsArray.[i].Length - 2 do
                      if i <> j then Expect.equal nsArray.[i].[j + 1] fsArray.[i].[j] "Item values must be equal"

          testCase "Two common segment" <| fun _ ->
              let modelsArray = [||]

              let nsd =
                  seq
                      [ { Namespace = [| "System"; "Collections" |]
                          Models = modelsArray }
                        { Namespace = [| "System"; "Collections"; "Generic" |]
                          Models = modelsArray }
                        { Namespace = [| "System"; "Collections"; "Specialized " |]
                          Models = modelsArray } ]

              let subject = CommonFeatures.getFilespaceDefinitions (nsd)
              Expect.equal (nsd |> Seq.length) (subject |> Seq.length) "Count must be equal"
              let nsArray =
                  nsd
                  |> Seq.map (fun x -> x.Namespace)
                  |> Seq.toArray

              let fsArray =
                  subject
                  |> Seq.map (fun x -> x.Filespace)
                  |> Seq.toArray

              let itemCount = nsArray.Length
              for i = 0 to itemCount - 1 do
                  for j = 0 to nsArray.[i].Length - 3 do
                      if i <> j then Expect.equal nsArray.[i].[j + 2] fsArray.[i].[j] "Item values must be equal" ]
