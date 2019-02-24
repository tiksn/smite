namespace TIKSN.smite.lib

module CommonFeatures =
    let getFilespaceDefinition(namespaceDefinitions: seq<NamespaceDefinition>) =
    let minNamespaceLength = namespaceDefinitions |> Seq.map (fun x -> x.Namespace.Length) |> Seq.min
    0