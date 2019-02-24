namespace TIKSN.smite.lib

module CSharpTranspiler =
    open TIKSN.smite.lib

    let fileExtension = ".cs"

    let transpileModelDefinition(ns: string[], fs: string[], model: ModelDefinition) =
        let sourceFileName = model.Name + fileExtension
        let relativeFilePath = Array.append fs [|sourceFileName|]
        {RelativeFilePath=relativeFilePath; FileContent=""}

    let transpileFilespaceDefinition(filespaceDefinition: FilespaceDefinition) =
        filespaceDefinition.Models
        |> Seq.map (fun x -> transpileModelDefinition(filespaceDefinition.Namespace, filespaceDefinition.Filespace, x))

    let transpile(models: seq<NamespaceDefinition>) =
        let filespaceDefinitions = CommonFeatures.getFilespaceDefinitions(models)
        filespaceDefinitions
        |> Seq.collect transpileFilespaceDefinition