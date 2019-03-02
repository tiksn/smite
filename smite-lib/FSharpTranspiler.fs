namespace TIKSN.smite.lib

module FSharpTranspiler =
    open FSharp.Compiler.Ast
    open IndentationFeatures

    let fileExtension = ".fs"
    let indentSpaces = 4

    let getSpecialType (t : FieldType) =
        match t with
        | FieldType.BooleanType -> "bool"
        | FieldType.IntegerType -> "int"
        | FieldType.RealType -> "double"
        | FieldType.StringType -> "string"

    let generateFieldCode (fieldDefinition : FieldDefinition) =
        let tn = getSpecialType (fieldDefinition.Type)
        { LineIndentCount = 2
          LineContent = fieldDefinition.Name + ": " + tn + ";" }

    let generateFieldsCode (fieldDefinitions : FieldDefinition []) =
        fieldDefinitions
        |> Seq.map (fun x -> generateFieldCode (x))
        |> Seq.toList

    let generateClassDeclaration (model : ModelDefinition) =
        let firstLine =
            { LineIndentCount = 1
              LineContent = "type " + model.Name + " = {" }

        let lastLine =
            { LineIndentCount = 1
              LineContent = "}" }

        let members = generateFieldsCode (model.Fields)
        [ emptyLine; firstLine ] @ members @ [ lastLine; emptyLine ]

    let generateClassDeclarations (models : ModelDefinition []) =
        models
        |> Seq.collect generateClassDeclaration
        |> Seq.toList

    let generateSourceFileCode (ns : string [], moduleName : string,
                                models : ModelDefinition []) =
        let nsString = CommonFeatures.composeDotSeparatedNamespace (ns)

        let directives =
            [ { LineIndentCount = 0
                LineContent = "namespace " + nsString }
              emptyLine
              { LineIndentCount = 0
                LineContent = "module " + moduleName + "Models =" }
              { LineIndentCount = 1
                LineContent = "open System" } ]

        let classDeclarationLines = generateClassDeclarations models
        let sourceFileLines = directives @ classDeclarationLines
        convertIndentedLinesToString (sourceFileLines, indentSpaces)

    let transpileFilespaceDefinition (filespaceDefinition : FilespaceDefinition) =
        let filePath =
            CommonFeatures.getFilePathWithExtension
                (filespaceDefinition, fileExtension)
        let moduleName = filespaceDefinition.Filespace |> Seq.last
        let sourceFileCode =
            generateSourceFileCode
                (filespaceDefinition.Namespace, moduleName,
                 filespaceDefinition.Models)
        { RelativeFilePath = filePath
          FileContent = sourceFileCode }

    let transpile (models : seq<NamespaceDefinition>) =
        let filespaceDefinitions =
            CommonFeatures.getFilespaceDefinitions (models)
        filespaceDefinitions
        |> Seq.map (fun x -> transpileFilespaceDefinition x)
