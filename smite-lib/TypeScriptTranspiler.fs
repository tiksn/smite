namespace TIKSN.smite.lib

module TypeScriptTranspiler =
    open IndentationFeatures
    open System.IO
    open TIKSN.Time

    let fileExtension = ".ts"
    let indentSpaces = 4

    let getLeadingFileComments (timeProvider : ITimeProvider) =
        let firstLines =
            [ { LineIndentCount = 0
                LineContent = "/*" } ]

        let lastLines =
            [ { LineIndentCount = 0
                LineContent = "*/" } ]

        let middleLines =
            CommonFeatures.getFileComment (timeProvider)
            |> List.map (fun x ->
                   { LineIndentCount = 1
                     LineContent = x })

        firstLines @ middleLines @ lastLines @ [ emptyLine ]

    let getSpecialType (t : PrimitiveType) =
        match t with
        | PrimitiveType.BooleanType -> "boolean"
        | PrimitiveType.IntegerType -> "number"
        | PrimitiveType.RealType -> "number"
        | PrimitiveType.StringType -> "string"

    let getFieldTypeSyntaxNode (fieldType : FieldType) =
        match fieldType with
        | PrimitiveType primitiveType -> (None, getSpecialType (primitiveType))
        | ComplexTypeSameNamespace typeName -> (None, typeName)
        | ComplexTypeDifferentNamespace(nsArray, typeName) ->
            (Some nsArray, typeName)

    let generateFieldCode (fieldDefinition : FieldDefinition) =
        let ns, tn = getFieldTypeSyntaxNode (fieldDefinition.Type)

        let fullTypeName =
            match ns with
            | Some x ->
                CommonFeatures.composeDotSeparatedNamespace (x) + "." + tn
            | None -> tn

        let t =
            match fieldDefinition.IsArray with
            | true -> fullTypeName + "[]"
            | false -> fullTypeName

        let line =
            { LineIndentCount = 2
              LineContent = fieldDefinition.Name + ": " + t + ";" }

        (ns, line)

    let generateFieldsCode (fieldDefinitions : FieldDefinition []) =
        fieldDefinitions
        |> Seq.map (fun x -> generateFieldCode (x))
        |> Seq.toList

    let generateClassDeclaration (model : ModelDefinition) =
        let firstLine =
            { LineIndentCount = 1
              LineContent = "export class " + model.Name + " {" }

        let lastLine =
            { LineIndentCount = 1
              LineContent = "}" }

        let fields = generateFieldsCode (model.Fields)

        let members =
            fields
            |> Seq.map (fun (_, f) -> f)
            |> Seq.toList

        let namespaces =
            fields
            |> Seq.filter (fun (o, _) -> o.IsSome)
            |> Seq.map (fun (o, _) -> o.Value)

        let lines = [ emptyLine; firstLine ] @ members @ [ lastLine ]
        (namespaces, lines)

    let generateClassDeclarations (models : ModelDefinition []) =
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

    let generateNamespaceDeclaration (ns : NamespaceDefinition) =
        let nsString =
            CommonFeatures.composeDotSeparatedNamespace (ns.Namespace)
        let namespaces, lines = generateClassDeclarations ns.Models

        let directives =
            [ { LineIndentCount = 0
                LineContent = "export namespace " + nsString + " {" } ]

        let namespaceClosingLines =
            [ { LineIndentCount = 0
                LineContent = "}" }
              emptyLine ]

        directives @ lines @ namespaceClosingLines

    let generateSourceFileCode (filespaceDefinition : MultiNamespaceFilespaceDefinition,
                                getFilespaces, comments : IndentedLine list) =
        let usings =
            filespaceDefinition.Namespaces
            |> Seq.collect (fun x -> x.Models)
            |> Seq.collect (fun x -> x.Fields)
            |> Seq.choose (fun x ->
                   match x.Type with
                   | ComplexTypeDifferentNamespace(nsArray, typeName) ->
                       Some nsArray
                   | _ -> None)
            |> Seq.distinct
            |> Seq.map getFilespaces
            |> Seq.collect (fun x -> x)
            |> Seq.map (fun x -> x.Filespace)
            |> Seq.distinct
            |> Seq.map (fun x -> x.[0])
            |> Seq.map
                   (fun x ->
                   { LineIndentCount = 0
                     LineContent = "import { " + x + " } from \"./" + x + "\"" })
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

        let sourceFileLines = comments @ usingsWithEmptyLine @ namespacesLines
        convertIndentedLinesToString (sourceFileLines, indentSpaces)

    let transpileFilespaceDefinition (filespaceDefinition : MultiNamespaceFilespaceDefinition,
                                      getFilespaces,
                                      comments : IndentedLine list) =
        let filePath =
            CommonFeatures.getFilePathWithExtensionForMultiNamespace
                (filespaceDefinition, fileExtension)
        let sourceFileCode =
            generateSourceFileCode
                (filespaceDefinition, getFilespaces, comments)
        { RelativeFilePath = filePath
          FileContent = sourceFileCode }

    let transpile (models : seq<NamespaceDefinition>,
                   timeProvider : ITimeProvider) =
        let comments = getLeadingFileComments (timeProvider)
        let filespaceDefinitions =
            CommonFeatures.getFilespaceDefinitionsForRootOnlyNamespaces (models)

        let getFilespaces (ns : string []) =
            filespaceDefinitions
            |> Seq.where (fun x ->
                   x.Namespaces
                   |> Seq.map (fun x -> x.Namespace)
                   |> Seq.contains ns)
        filespaceDefinitions
        |> Seq.map
               (fun x ->
               transpileFilespaceDefinition (x, getFilespaces, comments))
