namespace TIKSN.smite.lib

module FSharpTranspiler =
    open FSharp.Compiler.Ast
    open IndentationFeatures

    let fileExtension = ".fs"
    let indentSpaces = 4

    let generateSourceFileCode (ns : string [], moduleName : string,
                                models : ModelDefinition []) =
        let nsString = CommonFeatures.composeDotSeparatedNamespace (ns)

        let directives =
            [ { LineIndentCount = 0
                LineContent = "namespace " + nsString }
              { LineIndentCount = 0
                LineContent = "" }
              { LineIndentCount = 0
                LineContent = "module " + moduleName + "Models =" }
              { LineIndentCount = 1
                LineContent = "open System" } ]
        convertIndentedLinesToString (directives, indentSpaces)

    let rec getFilespacesWithExtension filespaces : string list =
        match filespaces with
        | hd :: [] -> [ hd + fileExtension ]
        | hd :: tl -> hd :: getFilespacesWithExtension tl
        | _ -> failwith "Empty list."

    let transpileFilespaceDefinition (filespaceDefinition : FilespaceDefinition) =
        let filespacesWithExtension =
            getFilespacesWithExtension
                (filespaceDefinition.Filespace |> Array.toList)
        let filePath = filespacesWithExtension |> List.toArray
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
