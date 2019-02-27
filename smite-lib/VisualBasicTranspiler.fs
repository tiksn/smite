namespace TIKSN.smite.lib

module VisualBasicTranspiler =
    open Microsoft.CodeAnalysis.Editing
    open Microsoft.CodeAnalysis

    let fileExtension = ".vb"

    let transpile (models : seq<NamespaceDefinition>) =
        let syntaxGenerator =
            SyntaxGenerator.GetGenerator
                (new AdhocWorkspace(), LanguageNames.VisualBasic)
        let filespaceDefinitions =
            CommonFeatures.getFilespaceDefinitions (models)
        filespaceDefinitions
        |> Seq.collect
               (fun x ->
               RoslynTranspiler.transpileFilespaceDefinition
                   (syntaxGenerator, fileExtension, x))
