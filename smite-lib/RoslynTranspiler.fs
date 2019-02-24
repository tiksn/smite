namespace TIKSN.smite.lib

module RoslynTranspiler =
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.Editing

    let getSpecialType(t: FieldType) =
        match t with
        | FieldType.BooleanType -> SpecialType.System_Boolean
        | FieldType.IntegerType -> SpecialType.System_Int32
        | FieldType.RealType -> SpecialType.System_Double
        | FieldType.StringType -> SpecialType.System_String

    let generateFieldCode(syntaxGenerator: SyntaxGenerator, fieldDefinition: FieldDefinition) =
        let tn = getSpecialType(fieldDefinition.Type)
        let ts = syntaxGenerator.TypeExpression(tn)
        let fd = syntaxGenerator.FieldDeclaration(fieldDefinition.Name, ts, Accessibility.Public)
        fd

    let generateFieldsCode(syntaxGenerator: SyntaxGenerator, fieldDefinitions: FieldDefinition[]) =
        fieldDefinitions
        |> Seq.map (fun x -> generateFieldCode(syntaxGenerator, x))
        |> Seq.toArray

    let generateClassDeclaration(syntaxGenerator: SyntaxGenerator, model: ModelDefinition) =
        let members = generateFieldsCode(syntaxGenerator, model.Fields)
        syntaxGenerator.ClassDeclaration(model.Name, null, Accessibility.Public, DeclarationModifiers.None, null, null, members)

    let generateNamespaceDeclaration(syntaxGenerator: SyntaxGenerator, ns: string[], model: ModelDefinition) =
        let nsString = CommonFeatures.composeDotSeparatedNamespace(ns)
        let classDefinition = generateClassDeclaration(syntaxGenerator, model)
        syntaxGenerator.NamespaceDeclaration(nsString, classDefinition);

    let generateSourceFileCode(syntaxGenerator: SyntaxGenerator, ns: string[], model: ModelDefinition) =
        let usingDirectives = syntaxGenerator.NamespaceImportDeclaration("System")
        let namespaceDeclaration = generateNamespaceDeclaration(syntaxGenerator, ns, model)
        let newNode = syntaxGenerator.CompilationUnit(usingDirectives, namespaceDeclaration).NormalizeWhitespace()
        newNode.ToString()