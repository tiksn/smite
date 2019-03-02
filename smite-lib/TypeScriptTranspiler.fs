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

    let generateSourceFileCode (ns : string [], models : ModelDefinition []) =
        let nsString = CommonFeatures.composeDotSeparatedNamespace (ns)

        let directives =
            [ //{ LineIndentCount = 0
            //    LineContent = "import \"./my-module.ts\"" }
            //  emptyLine
              { LineIndentCount = 0
                LineContent = "namespace " + nsString + " {" }
              emptyLine
              { LineIndentCount = 0
                LineContent = "}" } ]
        //let classDeclarationLines = generateClassDeclarations models
        //let sourceFileLines = directives @ classDeclarationLines
        convertIndentedLinesToString (directives, indentSpaces)

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
