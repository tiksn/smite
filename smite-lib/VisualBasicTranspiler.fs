namespace TIKSN.smite.lib

module VisualBasicTranspiler =
    let fileExtension = ".vb"

    let getTypeName(t: FieldType) =
        match t with
        | FieldType.BooleanType -> "Boolean"
        | FieldType.IntegerType -> "Integer"
        | FieldType.RealType -> "Double"
        | FieldType.StringType -> "String"