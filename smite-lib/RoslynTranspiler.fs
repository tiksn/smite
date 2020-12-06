namespace TIKSN.smite.lib

module RoslynTranspiler =
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.Editing
    open System

    let getSpecialType (t: PrimitiveType) =
        match t with
        | BooleanType -> SpecialType.System_Boolean
        | IntegerType -> SpecialType.System_Int32
        | RealType -> SpecialType.System_Double
        | StringType -> SpecialType.System_String

    let getFieldTypeSyntaxNode (syntaxGenerator: SyntaxGenerator, fieldType: FieldType) =
        match fieldType with
        | PrimitiveType primitiveType -> (None, syntaxGenerator.TypeExpression(getSpecialType (primitiveType)))
        | ComplexTypeSameNamespace typeName -> (None, syntaxGenerator.IdentifierName(typeName))
        | ComplexTypeDifferentNamespace (nsArray, typeName) -> (Some nsArray, syntaxGenerator.IdentifierName(typeName))

    let generateFieldCode (syntaxGenerator: SyntaxGenerator, fieldDefinition: FieldDefinition, fieldKind: FieldKind) =
        let ns, ts =
            getFieldTypeSyntaxNode (syntaxGenerator, fieldDefinition.Type)

        let ats = syntaxGenerator.ArrayTypeExpression(ts)

        let t =
            match fieldDefinition.IsArray with
            | true -> ats
            | false -> ts

        let backfieldName = sprintf "_%s" fieldDefinition.Name

        let fd =
            match fieldKind with
            | FieldKind.Field -> [ syntaxGenerator.FieldDeclaration(fieldDefinition.Name, t, Accessibility.Public) ]
            | FieldKind.Property ->
                [ syntaxGenerator.FieldDeclaration(backfieldName, t, Accessibility.Private)
                  syntaxGenerator.PropertyDeclaration
                      (fieldDefinition.Name,
                       t,
                       Accessibility.Public,
                       DeclarationModifiers.None,
                       [ syntaxGenerator.ReturnStatement
                           (syntaxGenerator.MemberAccessExpression
                               (syntaxGenerator.ThisExpression(), syntaxGenerator.IdentifierName(backfieldName))) ],
                       [ syntaxGenerator.AssignmentStatement
                           (syntaxGenerator.MemberAccessExpression
                               (syntaxGenerator.ThisExpression(), syntaxGenerator.IdentifierName(backfieldName)),
                            syntaxGenerator.IdentifierName("value")) ]) ]

        (ns, fd)

    let generateFieldsCode (syntaxGenerator: SyntaxGenerator, fieldDefinitions: FieldDefinition [], fieldKind: FieldKind) =
        fieldDefinitions
        |> Seq.map (fun x -> generateFieldCode (syntaxGenerator, x, fieldKind))
        |> Seq.toArray

    let generateClassDeclaration (model: ModelDefinition) (fieldKind: FieldKind) (syntaxGenerator: SyntaxGenerator) =
        let fields =
            generateFieldsCode (syntaxGenerator, model.Fields, fieldKind)

        let members =
            fields
            |> Seq.map (fun (_, f) -> f)
            |> Seq.collect (fun x -> x)

        let namespaces =
            fields
            |> Seq.filter (fun (o, _) -> o.IsSome)
            |> Seq.map (fun (o, _) -> o.Value)

        let syntaxNode =
            syntaxGenerator.ClassDeclaration
                (model.Name, null, Accessibility.Public, DeclarationModifiers.Partial, null, null, members)

        (namespaces, syntaxNode)

    let generateEnumerationDeclaration (enumeration: EnumerationDefinition) (syntaxGenerator: SyntaxGenerator) =
        let enumMemberNodes =
            enumeration.Values
            |> Seq.map (fun x -> syntaxGenerator.EnumMember(x))

        let syntaxNode =
            syntaxGenerator.EnumDeclaration
                (enumeration.Name, Accessibility.Public, DeclarationModifiers.None, enumMemberNodes)

        (Seq.empty, syntaxNode)

    let generateNamespaceDeclaration (syntaxGenerator: SyntaxGenerator,
                                      ns: string [],
                                      generateTypeDeclaration: SyntaxGenerator -> (seq<string []> * SyntaxNode)) =
        let nsString =
            CommonFeatures.composeDotSeparatedNamespace (ns)

        let namespaces, classDefinition =
            generateTypeDeclaration (syntaxGenerator)

        let syntaxNode =
            syntaxGenerator.NamespaceDeclaration(nsString, classDefinition)

        (namespaces, syntaxNode)

    let generateTypeSourceFileCode (syntaxGenerator: SyntaxGenerator,
                                    ns: string [],
                                    comments: string,
                                    generateTypeDeclaration) =
        let namespaces, namespaceDeclaration =
            generateNamespaceDeclaration (syntaxGenerator, ns, generateTypeDeclaration)

        let namespaceDeclarations = [ namespaceDeclaration ]

        let usingDirectives =
            namespaces
            |> Seq.map CommonFeatures.composeDotSeparatedNamespace
            |> Seq.map (fun n -> syntaxGenerator.NamespaceImportDeclaration(n))
            |> Seq.toList

        let directives = usingDirectives @ namespaceDeclarations

        let newNode =
            syntaxGenerator
                .CompilationUnit(directives)
                .NormalizeWhitespace()

        comments
        + Environment.NewLine
        + newNode.ToString()

    let transpileModelDefinition (syntaxGenerator: SyntaxGenerator,
                                  fileExtension: string,
                                  ns: string [],
                                  fs: string [],
                                  model: ModelDefinition,
                                  fieldKind: FieldKind,
                                  comments: string) =
        let sourceFileName = model.Name + fileExtension
        let relativeFilePath = Array.append fs [| sourceFileName |]
        let generateTypeDeclaration = generateClassDeclaration model fieldKind

        { RelativeFilePath = relativeFilePath
          FileContent = generateTypeSourceFileCode (syntaxGenerator, ns, comments, generateTypeDeclaration) }

    let transpileEnumerationDefinition (syntaxGenerator: SyntaxGenerator,
                                        fileExtension: string,
                                        ns: string [],
                                        fs: string [],
                                        enumeration: EnumerationDefinition,
                                        comments: string) =
        let sourceFileName = enumeration.Name + fileExtension
        let relativeFilePath = Array.append fs [| sourceFileName |]

        let generateTypeDeclaration =
            generateEnumerationDeclaration enumeration

        { RelativeFilePath = relativeFilePath
          FileContent = generateTypeSourceFileCode (syntaxGenerator, ns, comments, generateTypeDeclaration) }

    let transpileFilespaceDefinition (syntaxGenerator: SyntaxGenerator,
                                      fileExtension: string,
                                      filespaceDefinition: SingleNamespaceFilespaceDefinition,
                                      fieldKind: FieldKind,
                                      comments: string) =
        let modelsFiles =
            filespaceDefinition.Models
            |> Seq.map (fun x ->
                transpileModelDefinition
                    (syntaxGenerator,
                     fileExtension,
                     filespaceDefinition.Namespace,
                     filespaceDefinition.Filespace,
                     x,
                     fieldKind,
                     comments))

        let enumerationsFiles =
            filespaceDefinition.Enumerations
            |> Seq.map (fun x ->
                transpileEnumerationDefinition
                    (syntaxGenerator,
                     fileExtension,
                     filespaceDefinition.Namespace,
                     filespaceDefinition.Filespace,
                     x,
                     comments))

        Seq.concat [ modelsFiles
                     enumerationsFiles ]
