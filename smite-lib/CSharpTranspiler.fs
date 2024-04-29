namespace TIKSN.smite.lib

open System

module CSharpTranspiler =
    open TIKSN.smite.lib
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.Editing
    open IndentationFeatures

    let fileExtension = ".cs"
    let indentSpaces = 4

    let getLeadingFileComments (timeProvider: TimeProvider) =
        let firstLines =
            [ { LineIndentCount = 0
                LineContent = "/*" } ]

        let lastLines =
            [ { LineIndentCount = 0
                LineContent = "*/" } ]

        let middleLines =
            CommonFeatures.getFileComment (timeProvider)
            |> List.map (fun x -> { LineIndentCount = 1; LineContent = x })

        let lines = firstLines @ middleLines @ lastLines
        convertIndentedLinesToString (lines, indentSpaces)

    let transpile (models: seq<NamespaceDefinition>, fieldKind: FieldKind, timeProvider: TimeProvider) =
        let comments = getLeadingFileComments (timeProvider)

        let syntaxGenerator =
            SyntaxGenerator.GetGenerator(new AdhocWorkspace(), LanguageNames.CSharp)

        let filespaceDefinitions = CommonFeatures.getFilespaceDefinitions (models)

        filespaceDefinitions
        |> Seq.collect (fun x ->
            RoslynTranspiler.transpileFilespaceDefinition (syntaxGenerator, fileExtension, x, fieldKind, comments))
