namespace TIKSN.smite.lib

module CSharpTranspiler =
    open TIKSN.smite.lib
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.Formatting
    open Microsoft.CodeAnalysis
    open System.Text
    open System.IO

    let fileExtension = ".cs"

    let generateCSharpCode(ns: string[], model: ModelDefinition) =
        let cu = SyntaxFactory.CompilationUnit()
        let nsString = CommonFeatures.composeDotSeparatedNamespace(ns)
        let nsds = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.IdentifierName(nsString))
        let cuWithNamespace = cu.AddMembers(nsds)
        let customWorkspace = new AdhocWorkspace()
        let formattedNode = Formatter.Format(cuWithNamespace, customWorkspace)
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