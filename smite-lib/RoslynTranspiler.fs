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
            (None,
             syntaxGenerator.TypeExpression(getSpecialType (primitiveType)))
        | ComplexTypeSameNamespace typeName ->
            (None, syntaxGenerator.IdentifierName(typeName))
        | ComplexTypeDifferentNamespace(nsArray, typeName) ->
            (Some nsArray, syntaxGenerator.IdentifierName(typeName))

    let generateFieldCode (syntaxGenerator : SyntaxGenerator,
                           fieldDefinition : FieldDefinition) =
        let ns, ts =
            getFieldTypeSyntaxNode (syntaxGenerator, fieldDefinition.Type)
        let ats = syntaxGenerator.ArrayTypeExpression(ts)

        let t =
            match fieldDefinition.IsArray with
            | true -> ats
            | false -> ts

        let fd =
            syntaxGenerator.FieldDeclaration
                (fieldDefinition.Name, t, Accessibility.Public)
        (ns, fd)

    let generateFieldsCode (syntaxGenerator : SyntaxGenerator,
                            fieldDefinitions : FieldDefinition []) =
        fieldDefinitions
        |> Seq.map (fun x -> generateFieldCode (syntaxGenerator, x))
        |> Seq.toArray

    let generateClassDeclaration (syntaxGenerator : SyntaxGenerator,
                                  model : ModelDefinition) =
        let fields = generateFieldsCode (syntaxGenerator, model.Fields)
        let members = fields |> Seq.map (fun (_, f) -> f)

        let namespaces =
            fields
            |> Seq.filter (fun (o, _) -> o.IsSome)
            |> Seq.map (fun (o, _) -> o.Value)

        let syntaxNode =
            syntaxGenerator.ClassDeclaration
                (model.Name, null, Accessibility.Public,
                 DeclarationModifiers.None, null, null, members)
        (namespaces, syntaxNode)

    let generateNamespaceDeclaration (syntaxGenerator : SyntaxGenerator,
                                      ns : string [], model : ModelDefinition) =
        let nsString = CommonFeatures.composeDotSeparatedNamespace (ns)
        let namespaces, classDefinition =
            generateClassDeclaration (syntaxGenerator, model)
        let syntaxNode =
            syntaxGenerator.NamespaceDeclaration(nsString, classDefinition)
        (namespaces, syntaxNode)

    let generateSourceFileCode (syntaxGenerator : SyntaxGenerator,
                                ns : string [], model : ModelDefinition) =
        let namespaces, namespaceDeclaration =
            generateNamespaceDeclaration (syntaxGenerator, ns, model)
        let namespaceDeclarations = [ namespaceDeclaration ]

        let usingDirectives =
            namespaces
            |> Seq.map CommonFeatures.composeDotSeparatedNamespace
            |> Seq.map (fun n -> syntaxGenerator.NamespaceImportDeclaration(n))
            |> Seq.toList

        let directives = usingDirectives @ namespaceDeclarations
        let newNode =
            syntaxGenerator.CompilationUnit(directives).NormalizeWhitespace()
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
