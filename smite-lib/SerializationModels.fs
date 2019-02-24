namespace TIKSN.smite.lib

type FieldType =
    | IntegerType
    | StringType
    | RealType
    | BooleanType

type FieldDefinition = { Name: string; Type: FieldType }

type ModelDefinition = { Name: string; Fields: FieldDefinition[] }

type NamespaceDefinition = { Namespace: string[]; Models: ModelDefinition[] }

type FilespaceDefinition = { Namespace: string[]; Filespace: string[]; Models: ModelDefinition[] }

type SourceFile = {RelativeFilePath: string[]; FileContent: string}