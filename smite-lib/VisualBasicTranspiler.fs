namespace TIKSN.smite.lib

module VisualBasicTranspiler =
    open Microsoft.CodeAnalysis.Editing
    open Microsoft.CodeAnalysis
    open TIKSN.Time
    open IndentationFeatures

    let fileExtension = ".vb"
    let indentSpaces = 4

    let getLeadingFileComments (timeProvider: ITimeProvider) =
        let lines =
            CommonFeatures.getFileComment (timeProvider)
            |> List.map (fun x ->
                { LineIndentCount = 0
                  LineContent = "' " + x })
        convertIndentedLinesToString (lines, indentSpaces)

    let transpile (models: seq<NamespaceDefinition>, fieldKind: FieldKind, timeProvider: ITimeProvider) =
        let comments = getLeadingFileComments (timeProvider)
        let syntaxGenerator = SyntaxGenerator.GetGenerator(new AdhocWorkspace(), LanguageNames.VisualBasic)
        let filespaceDefinitions = CommonFeatures.getFilespaceDefinitions (models)
        filespaceDefinitions
        |> Seq.collect
            (fun x ->
            RoslynTranspiler.transpileFilespaceDefinition (syntaxGenerator, fileExtension, x, fieldKind, comments))
