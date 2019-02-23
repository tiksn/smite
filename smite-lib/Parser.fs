namespace TIKSN.smite.lib

[<AutoOpen>]
module Parser =
    open System.IO
    open YamlDotNet.RepresentationModel
    open System

    let parseModelYamlRootElement (rootNode: YamlMappingNode) =
        let ns = rootNode.Children.[new YamlScalarNode("namespace")]
        let models = rootNode.Children.[new YamlScalarNode("models")]

        let nsSeq = match ns with
        | :? YamlSequenceNode as yamlSequenceNode -> yamlSequenceNode
        | _ -> raise (FormatException("Node 'namespace' must be sequence"))

        let nsArray = nsSeq |> Seq.map (fun x -> x.ToString()) |> Seq.toArray

        Console.WriteLine(ns.NodeType)
        Console.WriteLine(models.NodeType)
        ()

    let parseModelYamlDocument (document: YamlDocument) =
        match document.RootNode with
        | :? YamlMappingNode as mappingNode -> parseModelYamlRootElement(mappingNode)
        | _ -> raise (FormatException("Document's root element must be mapping node."))

    let parseModelYaml fileName =
        let yaml = File.ReadAllText fileName
        use reader = new StringReader(yaml)
        let stream = YamlStream()
        stream.Load(reader)
        stream.Documents
        |> Seq.iter parseModelYamlDocument