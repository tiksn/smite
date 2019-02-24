namespace TIKSN.smite.lib

module CSharpTranspiler =
    open TIKSN.smite.lib
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.Editing

    let fileExtension = ".cs"

    let generateCSharpCode(ns: string[], model: ModelDefinition) =
        let syntaxGenerator = SyntaxGenerator.GetGenerator(new AdhocWorkspace(), LanguageNames.CSharp)
        RoslynTranspiler.generateSourceFileCode(syntaxGenerator, ns, model)

    let transpileModelDefinition(ns: string[], fs: string[], model: ModelDefinition) =
        let sourceFileName = model.Name + fileExtension
        let relativeFilePath = Array.append fs [|sourceFileName|]
        {RelativeFilePath=relativeFilePath; FileContent=generateCSharpCode(ns, model)}

    let transpileFilespaceDefinition(filespaceDefinition: FilespaceDefinition) =
        filespaceDefinition.Models
        |> Seq.map (fun x -> transpileModelDefinition(filespaceDefinition.Namespace, filespaceDefinition.Filespace, x))

    let transpile(models: seq<NamespaceDefinition>) =
        let filespaceDefinitions = CommonFeatures.getFilespaceDefinitions(models)
        filespaceDefinitions
        |> Seq.collect transpileFilespaceDefinition