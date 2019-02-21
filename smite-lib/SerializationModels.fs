namespace TIKSN.smite.lib

type FieldType =
    | IntegerType
    | StringType
    | RealType
    | BooleanType

type FieldDefinition = { Name: string; Type: FieldType }

type ModelDefinition = { Namespace: string[]; Fields: FieldDefinition[] }
