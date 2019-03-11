namespace TIKSN.smite.lib

module TypeScriptTranspiler =
    open IndentationFeatures
    open System.IO

    let fileExtension = ".ts"
    let indentSpaces = 4

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
        let atn = tn + "[]"

        let t =
            match fieldDefinition.IsArray with
            | true -> atn
            | false -> tn

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

    let generateSourceFileCode (ns : string [], models : ModelDefinition [],
                                getFilespace : string [] -> FilespaceDefinition) =
        let nsString = CommonFeatures.composeDotSeparatedNamespace (ns)
        let namespaces, lines = generateClassDeclarations models

        let usings =
            namespaces
            |> Seq.distinct
            |> Seq.map getFilespace
            |> Seq.map
                   (fun x ->
                   CommonFeatures.getFilePathWithExtension (x, fileExtension))
            |> Seq.map Path.Combine
            |> Seq.map (fun x ->
                   { LineIndentCount = 0
                     LineContent = "import \"./" + x + ".ts\"" })
            |> Seq.toList

        let directives =
            [ emptyLine
              { LineIndentCount = 0
                LineContent = "namespace " + nsString + " {" } ]

        let namespaceClosingLines =
            [ { LineIndentCount = 0
                LineContent = "}" } ]

        let sourceFileLines =
            usings @ directives @ lines @ namespaceClosingLines
        convertIndentedLinesToString (sourceFileLines, indentSpaces)

    let transpileFilespaceDefinition (filespaceDefinition : FilespaceDefinition,
                                      getFilespace) =
        let filePath =
            CommonFeatures.getFilePathWithExtension
                (filespaceDefinition, fileExtension)
        let sourceFileCode =
            generateSourceFileCode
                (filespaceDefinition.Namespace, filespaceDefinition.Models,
                 getFilespace)
        { RelativeFilePath = filePath
          FileContent = sourceFileCode }

    let transpile (models : seq<NamespaceDefinition>) =
        let filespaceDefinitions =
            CommonFeatures.getFilespaceDefinitions (models)

        let getFilespace (ns : string []) =
            filespaceDefinitions
            |> Seq.filter (fun x -> x.Namespace = ns)
            |> Seq.exactlyOne
        filespaceDefinitions
        |> Seq.map (fun x -> transpileFilespaceDefinition (x, getFilespace))
