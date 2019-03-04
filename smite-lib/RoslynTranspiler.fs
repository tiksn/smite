namespace TIKSN.smite.lib

module RoslynTranspiler =
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.Editing

    let getSpecialType (t : PrimitiveType) =
        match t with
        | BooleanType -> SpecialType.System_Boolean
        | IntegerType -> SpecialType.System_Int32
        | RealType -> SpecialType.System_Double
        | StringType -> SpecialType.System_String

    let getFieldTypeSyntaxNode (syntaxGenerator : SyntaxGenerator,
                                fieldType : FieldType) =
        match fieldType with
        | PrimitiveType primitiveType ->
            syntaxGenerator.TypeExpression(getSpecialType (primitiveType))
        | ComplexTypeSameNamespace typeName ->
            syntaxGenerator.IdentifierName(typeName)
        | ComplexTypeDifferentNamespace(_, typeName) ->
            syntaxGenerator.IdentifierName(typeName)

    let generateFieldCode (syntaxGenerator : SyntaxGenerator,
                           fieldDefinition : FieldDefinition) =
        let ts = getFieldTypeSyntaxNode (syntaxGenerator, fieldDefinition.Type)
        let ats = syntaxGenerator.ArrayTypeExpression(ts)

        let t =
            match fieldDefinition.IsArray with
            | true -> ats
            | false -> ts

        let fd =
            syntaxGenerator.FieldDeclaration
                (fieldDefinition.Name, t, Accessibility.Public)
        fd

    let generateFieldsCode (syntaxGenerator : SyntaxGenerator,
                            fieldDefinitions : FieldDefinition []) =
        fieldDefinitions
        |> Seq.map (fun x -> generateFieldCode (syntaxGenerator, x))
        |> Seq.toArray

    let generateClassDeclaration (syntaxGenerator : SyntaxGenerator,
                                  model : ModelDefinition) =
        let members = generateFieldsCode (syntaxGenerator, model.Fields)
        syntaxGenerator.ClassDeclaration
            (model.Name, null, Accessibility.Public, DeclarationModifiers.None,
             null, null, members)

    let generateNamespaceDeclaration (syntaxGenerator : SyntaxGenerator,
                                      ns : string [], model : ModelDefinition) =
        let nsString = CommonFeatures.composeDotSeparatedNamespace (ns)
        let classDefinition = generateClassDeclaration (syntaxGenerator, model)
        syntaxGenerator.NamespaceDeclaration(nsString, classDefinition)

    let generateSourceFileCode (syntaxGenerator : SyntaxGenerator,
                                ns : string [], model : ModelDefinition) =
        let usingDirectives =
            syntaxGenerator.NamespaceImportDeclaration("System")
        let namespaceDeclaration =
            generateNamespaceDeclaration (syntaxGenerator, ns, model)
        let newNode =
            syntaxGenerator.CompilationUnit(usingDirectives,
                                            namespaceDeclaration)
                           .NormalizeWhitespace()
        newNode.ToString()

    let transpileModelDefinition (syntaxGenerator : SyntaxGenerator,
                                  fileExtension : string, ns : string [],
                                  fs : string [], model : ModelDefinition) =
        let sourceFileName = model.Name + fileExtension
        let relativeFilePath = Array.append fs [| sourceFileName |]
        { RelativeFilePath = relativeFilePath
          FileContent = generateSourceFileCode (syntaxGenerator, ns, model) }

    let transpileFilespaceDefinition (syntaxGenerator : SyntaxGenerator,
                                      fileExtension : string,
                                      filespaceDefinition : FilespaceDefinition) =
        filespaceDefinition.Models
        |> Seq.map
               (fun x ->
               transpileModelDefinition
                   (syntaxGenerator, fileExtension,
                    filespaceDefinition.Namespace, filespaceDefinition.Filespace,
                    x))
