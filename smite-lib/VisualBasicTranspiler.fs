namespace TIKSN.smite.lib

module VisualBasicTranspiler =
    let fileExtension = ".vb"

    let getTypeName(t: FieldType) =
        match t with
        | FieldType.BooleanType -> "bool"
        | FieldType.IntegerType -> "int"
        | FieldType.RealType -> "double"
        | FieldType.StringType -> "string"