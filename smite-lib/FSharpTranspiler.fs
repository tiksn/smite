namespace TIKSN.smite.lib

module FSharpTranspiler =
    open IndentationFeatures
    open TIKSN.Time

    let fileExtension = ".fs"
    let indentSpaces = 4

    let getLeadingFileComments (timeProvider: ITimeProvider) =
        let firstLines =
            [ { LineIndentCount = 0
                LineContent = "(*" } ]

        let lastLines =
            [ { LineIndentCount = 0
                LineContent = "*)" } ]

        let middleLines =
            CommonFeatures.getFileComment (timeProvider)
            |> List.map (fun x -> { LineIndentCount = 0; LineContent = x })

        firstLines
        @ middleLines @ lastLines @ [ emptyLine ]

    let getSpecialType (t: PrimitiveType) =
        match t with
        | PrimitiveType.BooleanType -> "bool"
        | PrimitiveType.IntegerType -> "int"
        | PrimitiveType.RealType -> "double"
        | PrimitiveType.StringType -> "string"

    let generateFieldCode (fieldDefinition: FieldDefinition) =
        let ns, tn =
            CommonFeatures.getFieldTypeSyntaxNode (fieldDefinition.Type, getSpecialType)

        let atn = tn + " []"

        let t =
            match fieldDefinition.IsArray with
            | true -> atn
            | false -> tn

        let line =
            { LineIndentCount = 2
              LineContent = fieldDefinition.Name + ": " + t }

        (ns, line)

    let generateFieldsCode (fieldDefinitions: FieldDefinition []) =
        fieldDefinitions
        |> Seq.map (fun x -> generateFieldCode (x))
        |> Seq.toList

    let generateClassDeclaration (model: ModelDefinition) =
        let firstLine =
            { LineIndentCount = 1
              LineContent = "type " + model.Name + " = {" }

        let lastLine =
            { LineIndentCount = 1
              LineContent = "}" }

        let fields = generateFieldsCode (model.Fields)

        let members =
            fields |> Seq.map (fun (_, f) -> f) |> Seq.toList

        let namespaces =
            fields
            |> Seq.filter (fun (o, _) -> o.IsSome)
            |> Seq.map (fun (o, _) -> o.Value)

        let lines =
            [ emptyLine; firstLine ]
            @ members @ [ lastLine; emptyLine ]

        (namespaces, lines)

    let generateEnumerationDeclaration (enumeration: EnumerationDefinition) =
        let firstLine =
            { LineIndentCount = 1
              LineContent = "type " + enumeration.Name + " =" }

        let members =
            enumeration.Values
            |> Seq.mapi
                (fun i v ->
                    { LineIndentCount = 1
                      LineContent = "| " + v + " = " + i.ToString() })
            |> Seq.toList

        [ emptyLine; firstLine ] @ members @ [ emptyLine ]

    let generateClassDeclarations (models: ModelDefinition []) =
        let namespaces =
            models
            |> Seq.map generateClassDeclaration
            |> Seq.collect (fun (x, _) -> x)

        let lines =
            models
            |> Seq.map generateClassDeclaration
            |> Seq.collect (fun (_, x) -> x)
            |> Seq.toList

        (namespaces, lines)

    let generateEnumerationDeclarations (enumerations: EnumerationDefinition []) =
        enumerations
        |> Seq.map generateEnumerationDeclaration
        |> Seq.collect (fun x -> x)
        |> Seq.toList

    let generateSourceFileCode
        (
            ns: string [],
            moduleName: string,
            models: ModelDefinition [],
            enumerations: EnumerationDefinition [],
            comments: IndentedLine list
        ) =
        let nsString =
            CommonFeatures.composeDotSeparatedNamespace (ns)

        let namespaces, modelsLines = generateClassDeclarations models

        let enumerationsLines =
            generateEnumerationDeclarations enumerations

        let directives =
            [ { LineIndentCount = 0
                LineContent = "namespace " + nsString }
              emptyLine
              { LineIndentCount = 0
                LineContent = "module " + moduleName + "Models =" } ]

        let usings =
            namespaces
            |> Seq.distinct
            |> Seq.map CommonFeatures.composeDotSeparatedNamespace
            |> Seq.map
                (fun x ->
                    { LineIndentCount = 1
                      LineContent = "open " + x })
            |> Seq.toList

        let sourceFileLines =
            comments
            @ directives
              @ usings @ enumerationsLines @ modelsLines

        convertIndentedLinesToString (sourceFileLines, indentSpaces)

    let transpileFilespaceDefinition
        (
            filespaceDefinition: SingleNamespaceFilespaceDefinition,
            comments: IndentedLine list
        ) =
        let filePath =
            CommonFeatures.getFilePathWithExtension (filespaceDefinition, fileExtension)

        let moduleName =
            filespaceDefinition.Filespace |> Seq.last

        let sourceFileCode =
            generateSourceFileCode (
                filespaceDefinition.Namespace,
                moduleName,
                filespaceDefinition.Models,
                filespaceDefinition.Enumerations,
                comments
            )

        { RelativeFilePath = filePath
          FileContent = sourceFileCode }

    let transpile (namespaceDefinitions: seq<NamespaceDefinition>, timeProvider: ITimeProvider) =
        let comments = getLeadingFileComments (timeProvider)

        let filespaceDefinitions =
            CommonFeatures.getFilespaceDefinitions (namespaceDefinitions)

        filespaceDefinitions
        |> Seq.map (fun x -> transpileFilespaceDefinition (x, comments))
