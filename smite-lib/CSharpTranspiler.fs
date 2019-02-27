namespace TIKSN.smite.lib

module CSharpTranspiler =
    open TIKSN.smite.lib
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.Editing

    let fileExtension = ".cs"

    let transpile (models : seq<NamespaceDefinition>) =
        let syntaxGenerator =
            SyntaxGenerator.GetGenerator
                (new AdhocWorkspace(), LanguageNames.CSharp)
        let filespaceDefinitions =
            CommonFeatures.getFilespaceDefinitions (models)
        filespaceDefinitions
        |> Seq.collect
               (fun x ->
               RoslynTranspiler.transpileFilespaceDefinition
                   (syntaxGenerator, fileExtension, x))
