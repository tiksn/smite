namespace TIKSN.smite.lib

module TypeScriptTranspiler =
    open IndentationFeatures
    open TIKSN.Time

    let fileExtension = ".ts"
    let indentSpaces = 4

    let getLeadingFileComments (timeProvider: ITimeProvider) =
        let firstLines =
            [ { LineIndentCount = 0
                LineContent = "/*" } ]

        let lastLines =
            [ { LineIndentCount = 0
                LineContent = "*/" } ]

        let middleLines =
            CommonFeatures.getFileComment (timeProvider)
            |> List.map (fun x -> { LineIndentCount = 1; LineContent = x })

        firstLines
        @ middleLines @ lastLines @ [ emptyLine ]

    let getSpecialType (t: PrimitiveType) =
        match t with
        | PrimitiveType.BooleanType -> "boolean"
        | PrimitiveType.IntegerType -> "number"
        | PrimitiveType.RealType -> "number"
        | PrimitiveType.StringType -> "string"

    let generateFieldCode (fieldDefinition: FieldDefinition) =
        let ns, tn =
            CommonFeatures.getFieldTypeSyntaxNode (fieldDefinition.Type, getSpecialType)

        let fullTypeName =
            match ns with
            | Some x ->
                CommonFeatures.composeDotSeparatedNamespace (x)
                + "."
                + tn
            | None -> tn

        let t =
            match fieldDefinition.IsArray with
            | true -> fullTypeName + "[]"
            | false -> fullTypeName

        let line =
            { LineIndentCount = 2
              LineContent = fieldDefinition.Name + ": " + t + ";" }

        (ns, line)

    let generateFieldsCode (fieldDefinitions: FieldDefinition []) =
        fieldDefinitions
        |> Seq.map (fun x -> generateFieldCode (x))
        |> Seq.toList

    let generateClassDeclaration (model: ModelDefinition) =
        let firstLine =
            { LineIndentCount = 1
              LineContent = "export class " + model.Name + " {" }

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
            [ emptyLine; firstLine ] @ members @ [ lastLine ]

        (namespaces, lines)

    let generateEnumerationDeclaration (enumeration: EnumerationDefinition) =
        let firstLine =
            { LineIndentCount = 1
              LineContent = "export enum " + enumeration.Name + " {" }

        let lastLine =
            { LineIndentCount = 1
              LineContent = "}" }

        let members =
            enumeration.Values
            |> Seq.map (fun v ->
                { LineIndentCount = 2
                  LineContent = v + "," })
            |> Seq.toList

        [ emptyLine; firstLine ] @ members @ [ lastLine ]

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

    let generateNamespaceDeclaration (ns: NamespaceDefinition) =
        let nsString =
            CommonFeatures.composeDotSeparatedNamespace (ns.Namespace)

        let namespaces, modelsLines = generateClassDeclarations ns.Models

        let enumerationsLines =
            generateEnumerationDeclarations ns.Enumerations

        let directives =
            [ { LineIndentCount = 0
                LineContent = "export namespace " + nsString + " {" } ]

        let namespaceClosingLines =
            [ { LineIndentCount = 0
                LineContent = "}" }
              emptyLine ]

        directives
        @ enumerationsLines
          @ modelsLines @ namespaceClosingLines

    let generateSourceFileCodePerNamespace (namespaceDefinition: NamespaceDefinition, getFilespaces) =
        namespaceDefinition.Models
        |> Seq.collect (fun x -> x.Fields)
        |> Seq.choose (fun x ->
            match x.Type with
            | ComplexTypeDifferentNamespace (nsArray, typeName) ->
                Some nsArray

                if nsArray.[0] <> namespaceDefinition.Namespace.[0]
                then Some nsArray
                else None
            | _ -> None)
        |> Seq.distinct
        |> Seq.map getFilespaces
        |> Seq.collect (fun x -> x)
        |> Seq.map (fun x -> x.Filespace)
        |> Seq.distinct
        |> Seq.map (fun x -> x.[0])
        |> Seq.map (fun x ->
            { LineIndentCount = 0
              LineContent = "import { " + x + " } from \"./" + x + "\"" })

    let generateSourceFileCode (filespaceDefinition: MultiNamespaceFilespaceDefinition,
                                getFilespaces,
                                comments: IndentedLine list) =
        let usings =
            filespaceDefinition.Namespaces
            |> Seq.map (fun x -> generateSourceFileCodePerNamespace (x, getFilespaces))
            |> Seq.collect (fun x -> x)
            |> Seq.toList

        let usingsWithEmptyLine =
            match usings.Length with
            | 0 -> usings
            | _ -> usings @ [ emptyLine ]

        let namespacesLines =
            filespaceDefinition.Namespaces
            |> Seq.map (fun x -> generateNamespaceDeclaration x)
            |> Seq.collect (fun x -> x)
            |> Seq.toList

        let sourceFileLines =
            comments @ usingsWithEmptyLine @ namespacesLines

        convertIndentedLinesToString (sourceFileLines, indentSpaces)

    let transpileFilespaceDefinition (filespaceDefinition: MultiNamespaceFilespaceDefinition,
                                      getFilespaces,
                                      comments: IndentedLine list) =
        let filePath =
            CommonFeatures.getFilePathWithExtensionForMultiNamespace (filespaceDefinition, fileExtension)

        let sourceFileCode =
            generateSourceFileCode (filespaceDefinition, getFilespaces, comments)

        { RelativeFilePath = filePath
          FileContent = sourceFileCode }

    let transpile (namespaceDefinitions: seq<NamespaceDefinition>, timeProvider: ITimeProvider) =
        let comments = getLeadingFileComments (timeProvider)

        let filespaceDefinitions =
            CommonFeatures.getFilespaceDefinitionsForRootOnlyNamespaces (namespaceDefinitions)

        let getFilespaces (ns: string []) =
            filespaceDefinitions
            |> Seq.where (fun x ->
                x.Namespaces
                |> Seq.map (fun x -> x.Namespace)
                |> Seq.contains ns)

        filespaceDefinitions
        |> Seq.map (fun x -> transpileFilespaceDefinition (x, getFilespaces, comments))
