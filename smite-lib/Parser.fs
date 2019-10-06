namespace TIKSN.smite.lib

open System.Collections.Generic

[<AutoOpen>]
module Parser =
    open System.IO
    open YamlDotNet.RepresentationModel
    open System

    let getSequenceNode (node: YamlNode) =
        match node with
        | :? YamlSequenceNode as yamlSequenceNode -> yamlSequenceNode
        | _ -> raise (FormatException("Node must be sequence node."))

    let getMappingNode (node: YamlNode) =
        match node with
        | :? YamlMappingNode as yamlMappingNode -> yamlMappingNode
        | _ -> raise (FormatException("Node must be mapping node."))

    let getScalarNode (node: YamlNode) =
        match node with
        | :? YamlScalarNode as yamlScalarNode -> yamlScalarNode
        | _ -> raise (FormatException("Node must be scalar node."))

    let getOptionalChild (children: IDictionary<YamlNode, YamlNode>) name =
        match children.ContainsKey(new YamlScalarNode(name)) with
        | true -> Some children.[new YamlScalarNode(name)]
        | false -> None
    
    let getOptionalSequenceNode (children: IDictionary<YamlNode, YamlNode>) name =
        let child = getOptionalChild children name
        match child with
        | Some x -> Some (getSequenceNode x)
        | None -> None

    let getNamespaceStrings (nsNode: YamlSequenceNode) =
        nsNode
        |> Seq.map getScalarNode
        |> Seq.map (fun x -> x.Value)
        |> Seq.toArray

    let parseModelFieldNode (fieldNode: YamlMappingNode) =
        let nameValue = getScalarNode(fieldNode.Children.[new YamlScalarNode("name")]).Value
        let typeValue = getScalarNode(fieldNode.Children.[new YamlScalarNode("type")]).Value
        let typeNamespaceKey = new YamlScalarNode("namespace")

        let typeNamespace =
            match fieldNode.Children.ContainsKey(typeNamespaceKey) with
            | true -> Some(getSequenceNode (fieldNode.Children.[typeNamespaceKey]) |> getNamespaceStrings)
            | false -> None

        let typeEnum =
            match typeValue with
            | "integer" -> PrimitiveType IntegerType
            | "boolean" -> PrimitiveType BooleanType
            | "real" -> PrimitiveType RealType
            | "string" -> PrimitiveType StringType
            | _ ->
                match typeNamespace with
                | Some x -> ComplexTypeDifferentNamespace(x, typeValue)
                | None -> ComplexTypeSameNamespace typeValue

        let isArrayKey = new YamlScalarNode("array")

        let isArrayValue =
            match fieldNode.Children.ContainsKey(isArrayKey) with
            | true -> Some(getScalarNode(fieldNode.Children.[isArrayKey]).Value)
            | false -> None

        let isArray =
            match isArrayValue with
            | Some x -> bool.Parse x
            | None -> false

        { Name = nameValue
          Type = typeEnum
          IsArray = isArray }

    let parseModelSequence (modelNode: YamlMappingNode) =
        let nameValue = getScalarNode(modelNode.Children.[new YamlScalarNode("name")]).Value
        let fieldsNodeChildren = getSequenceNode(modelNode.Children.[new YamlScalarNode("fields")]).Children

        let fields =
            fieldsNodeChildren
            |> Seq.map getMappingNode
            |> Seq.map (fun x -> parseModelFieldNode (x))
            |> Seq.toArray
        { Name = nameValue
          Fields = fields }

    let parseEnumerationSequence (modelNode: YamlMappingNode) =
        let nameValue = getScalarNode(modelNode.Children.[new YamlScalarNode("name")]).Value
        let valuesNodeChildren = getSequenceNode(modelNode.Children.[new YamlScalarNode("values")]).Children

        let fields =
            valuesNodeChildren
            |> Seq.map getScalarNode
            |> Seq.map (fun x -> x.Value)
            |> Seq.toArray
        { Name = nameValue
          Values = fields }

    let parseEntityAsArray parseEntitySequence node  =
        match node with
        | Some children -> 
            children
            |> Seq.map getMappingNode
            |> Seq.map (fun x -> parseEntitySequence (x))
            |> Seq.toArray
        | None -> [||]

    let parseYamlRootElement (rootNode: YamlMappingNode) =
        let nsNode = getSequenceNode (rootNode.Children.[new YamlScalarNode("namespace")])
        let modelsNode = getOptionalSequenceNode rootNode.Children "models"
        let enumerationsNode = getOptionalSequenceNode rootNode.Children "enumerations"
        let nsArray = getNamespaceStrings nsNode

        let modelsArray =
            match modelsNode with
            | Some children -> 
                children
                |> Seq.map getMappingNode
                |> Seq.map (fun x -> parseModelSequence (x))
                |> Seq.toArray
            | None -> [||]

        { Namespace = nsArray
          Models = parseEntityAsArray parseModelSequence modelsNode
          Enumerations = parseEntityAsArray parseEnumerationSequence enumerationsNode}

    let parseModelYaml fileName =
        let yaml = File.ReadAllText fileName
        use reader = new StringReader(yaml)
        let stream = YamlStream()
        stream.Load(reader)
        stream.Documents
        |> Seq.map (fun x -> getMappingNode (x.RootNode))
        |> Seq.map parseYamlRootElement
