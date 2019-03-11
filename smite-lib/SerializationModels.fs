namespace TIKSN.smite.lib

type PrimitiveType =
    | IntegerType
    | StringType
    | RealType
    | BooleanType

type FieldType =
    | PrimitiveType of PrimitiveType
    | ComplexTypeSameNamespace of string
    | ComplexTypeDifferentNamespace of string [] * string

type FieldDefinition =
    { Name : string
      Type : FieldType
      IsArray : bool }

type ModelDefinition =
    { Name : string
      Fields : FieldDefinition [] }

type NamespaceDefinition =
    { Namespace : string []
      Models : ModelDefinition [] }

type FilespaceDefinition =
    { Namespace : string []
      Filespace : string []
      Models : ModelDefinition [] }

type SourceFile =
    { RelativeFilePath : string []
      FileContent : string }
