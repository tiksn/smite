namespace TIKSN.smite.lib

module TypeScriptTranspiler =
    let fileExtension = ".ts"
    let indentSpaces = 4

    let getSpecialType (t : FieldType) =
        match t with
        | FieldType.BooleanType -> "boolean"
        | FieldType.IntegerType -> "number"
        | FieldType.RealType -> "number"
        | FieldType.StringType -> "string"

    let transpile (models : seq<NamespaceDefinition>) = 0
