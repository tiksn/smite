namespace TIKSN.smite.lib

module TypeScriptTranspiler =
    open IndentationFeatures

    let fileExtension = ".ts"
    let indentSpaces = 4

    let getSpecialType (t : FieldType) =
        match t with
        | FieldType.BooleanType -> "boolean"
        | FieldType.IntegerType -> "number"
        | FieldType.RealType -> "number"
        | FieldType.StringType -> "string"

    let generateFieldCode (fieldDefinition : FieldDefinition) =
        let tn = getSpecialType (fieldDefinition.Type)
        let atn = tn + "[]"

        let t =
            match fieldDefinition.IsArray with
            | true -> atn
            | false -> tn
        { LineIndentCount = 2
          LineContent = fieldDefinition.Name + ": " + t + ";" }

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

        let members = generateFieldsCode (model.Fields)
        [ emptyLine; firstLine ] @ members @ [ lastLine ]

    let generateClassDeclarations (models : ModelDefinition []) =
        models
        |> Seq.collect generateClassDeclaration
        |> Seq.toList

    let generateSourceFileCode (ns : string [], models : ModelDefinition []) =
        let nsString = CommonFeatures.composeDotSeparatedNamespace (ns)

        let directives =
            [ { //{ LineIndentCount = 0
                //    LineContent = "import \"./my-module.ts\"" }
                //  emptyLine
                LineIndentCount = 0
                LineContent = "namespace " + nsString + " {" } ]

        let namespaceClosingLines =
            [ { LineIndentCount = 0
                LineContent = "}" } ]

        let classDeclarationLines = generateClassDeclarations models
        let sourceFileLines =
            directives @ classDeclarationLines @ namespaceClosingLines
        convertIndentedLinesToString (sourceFileLines, indentSpaces)

    let transpileFilespaceDefinition (filespaceDefinition : FilespaceDefinition) =
        let filePath =
            CommonFeatures.getFilePathWithExtension
                (filespaceDefinition, fileExtension)
        let sourceFileCode =
            generateSourceFileCode
                (filespaceDefinition.Namespace, filespaceDefinition.Models)
        { RelativeFilePath = filePath
          FileContent = sourceFileCode }

    let transpile (models : seq<NamespaceDefinition>) =
        let filespaceDefinitions =
            CommonFeatures.getFilespaceDefinitions (models)
        filespaceDefinitions
        |> Seq.map (fun x -> transpileFilespaceDefinition x)
