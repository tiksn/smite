﻿namespace TIKSN.smite.lib

module CSharpTranspiler =
    open TIKSN.smite.lib
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.Formatting
    open Microsoft.CodeAnalysis
    open System.Text
    open System.IO
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Microsoft.CodeAnalysis.Editing

    let fileExtension = ".cs"

    let getTypeName(t: FieldType) =
        match t with
        | FieldType.BooleanType -> "bool"
        | FieldType.IntegerType -> "int"
        | FieldType.RealType -> "double"
        | FieldType.StringType -> "string"

    let generateFieldsCode(syntaxGenerator: SyntaxGenerator, fieldDefinitions: FieldDefinition[]) =
        fieldDefinitions
        |> Seq.map (fun x -> RoslynTranspiler.generateFieldCode(syntaxGenerator, x, getTypeName))
        |> Seq.map (fun x -> x :> MemberDeclarationSyntax)
        |> Seq.toArray

    let generateCSharpCode(ns: string[], model: ModelDefinition) =
        let syntaxGenerator = SyntaxGenerator.GetGenerator(new AdhocWorkspace(), LanguageNames.CSharp)
        let cu = SyntaxFactory.CompilationUnit()
        let nsString = CommonFeatures.composeDotSeparatedNamespace(ns)
        let nsds = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(nsString))
        let cds = SyntaxFactory.ClassDeclaration(model.Name).AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.SealedKeyword))
        let fieldDeclarations = generateFieldsCode(syntaxGenerator, model.Fields)
        let cdsWithFields = cds.AddMembers(fieldDeclarations)
        let customWorkspace = new AdhocWorkspace()
        let formattedNode = Formatter.Format(cu.AddMembers(nsds.AddMembers(cdsWithFields)), customWorkspace)
        let sb = new StringBuilder()
        use writer = new StringWriter(sb)
        formattedNode.WriteTo(writer)
        sb.ToString()

    let transpileModelDefinition(ns: string[], fs: string[], model: ModelDefinition) =
        let sourceFileName = model.Name + fileExtension
        let relativeFilePath = Array.append fs [|sourceFileName|]
        {RelativeFilePath=relativeFilePath; FileContent=generateCSharpCode(ns, model)}

    let transpileFilespaceDefinition(filespaceDefinition: FilespaceDefinition) =
        filespaceDefinition.Models
        |> Seq.map (fun x -> transpileModelDefinition(filespaceDefinition.Namespace, filespaceDefinition.Filespace, x))

    let transpile(models: seq<NamespaceDefinition>) =
        let filespaceDefinitions = CommonFeatures.getFilespaceDefinitions(models)
        filespaceDefinitions
        |> Seq.collect transpileFilespaceDefinition