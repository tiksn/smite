namespace TIKSN.smite.lib

module FSharpTranspiler =
    open FSharp.Compiler.Ast

    let fileExtension = ".fs"

    let transpile(models: seq<NamespaceDefinition>) =
        []