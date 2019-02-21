namespace TIKSN.smite.lib

type FieldType =
    | IntegerType
    | StringType
    | RealType
    | BooleanType

type FieldDefinition = {Name: string; Type: FieldType}
