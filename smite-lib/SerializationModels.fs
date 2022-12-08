namespace TIKSN.smite.lib

type PrimitiveType =
    | IntegerType
    | StringType
    | RealType
    | BooleanType

type FieldKind =
    | Field = 1
    | Property = 2

type FieldType =
    | PrimitiveType of PrimitiveType
    | ComplexTypeSameNamespace of string
    | ComplexTypeDifferentNamespace of string[] * string

type FieldDefinition =
    { Name: string
      Type: FieldType
      IsArray: bool }

type ModelDefinition =
    { Name: string
      Fields: FieldDefinition[] }

type EnumerationDefinition = { Name: string; Values: string[] }

type NamespaceDefinition =
    { Namespace: string[]
      Models: ModelDefinition[]
      Enumerations: EnumerationDefinition[] }

type SingleNamespaceFilespaceDefinition =
    { Namespace: string[]
      Filespace: string[]
      Models: ModelDefinition[]
      Enumerations: EnumerationDefinition[] }

type MultiNamespaceFilespaceDefinition =
    { Filespace: string[]
      Namespaces: NamespaceDefinition[] }

type SourceFile =
    { RelativeFilePath: string[]
      FileContent: string }
