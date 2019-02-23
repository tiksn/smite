﻿namespace TIKSN.smite.lib

[<AutoOpen>]
module Parser =
    open System.IO
    open YamlDotNet.RepresentationModel
    open System

    let getSequenceNode(node: YamlNode) =
        match node with
        | :? YamlSequenceNode as yamlSequenceNode -> yamlSequenceNode
        | _ -> raise (FormatException("Node must be sequence node."))

    let getMappingNode(node: YamlNode) =
        match node with
        | :? YamlMappingNode as yamlMappingNode -> yamlMappingNode
        | _ -> raise (FormatException("Node must be mapping node."))

    let getScalarNode(node: YamlNode) =
        match node with
        | :? YamlScalarNode as yamlScalarNode -> yamlScalarNode
        | _ -> raise (FormatException("Node must be scalar node."))

    let parseModelSequence(modelNode: YamlMappingNode) =
        let nameNode = getScalarNode(modelNode.Children.[new YamlScalarNode("name")]).Value
        0

    let parseYamlRootElement (rootNode: YamlMappingNode) =
        let nsNode = getSequenceNode(rootNode.Children.[new YamlScalarNode("namespace")])
        let modelsNode = getSequenceNode(rootNode.Children.[new YamlScalarNode("models")])

        let nsArray = nsNode |> Seq.map getScalarNode |> Seq.map (fun x -> x.Value) |> Seq.toArray

        let modelsArray = modelsNode.Children |> Seq.map getMappingNode |> Seq.map (fun x -> parseModelSequence(x)) |> Seq.toArray

        Console.WriteLine(modelsNode.NodeType)
        ()

    let parseModelYaml fileName =
        let yaml = File.ReadAllText fileName
        use reader = new StringReader(yaml)
        let stream = YamlStream()
        stream.Load(reader)
        stream.Documents
        |> Seq.map (fun x -> getMappingNode(x.RootNode))
        |> Seq.iter parseYamlRootElement