namespace TIKSN.smite.lib

[<AutoOpen>]
module Parser =
    open System.IO
    open YamlDotNet.RepresentationModel
    open System

    let getSequenceNode (node : YamlNode) =
        match node with
        | :? YamlSequenceNode as yamlSequenceNode -> yamlSequenceNode
        | _ -> raise (FormatException("Node must be sequence node."))

    let getMappingNode (node : YamlNode) =
        match node with
        | :? YamlMappingNode as yamlMappingNode -> yamlMappingNode
        | _ -> raise (FormatException("Node must be mapping node."))

    let getScalarNode (node : YamlNode) =
        match node with
        | :? YamlScalarNode as yamlScalarNode -> yamlScalarNode
        | _ -> raise (FormatException("Node must be scalar node."))

    let parseModelFieldNode (fieldNode : YamlMappingNode) =
        let nameValue =
            getScalarNode(fieldNode.Children.[new YamlScalarNode("name")]).Value
        let typeValue =
            getScalarNode(fieldNode.Children.[new YamlScalarNode("type")]).Value

        let typeEnum =
            match typeValue with
            | "integer" -> FieldType.IntegerType
            | "boolean" -> FieldType.BooleanType
            | "real" -> FieldType.RealType
            | "string" -> FieldType.StringType
            | _ -> raise (FormatException("Unknown format type"))

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

    let parseModelSequence (modelNode : YamlMappingNode) =
        let nameValue =
            getScalarNode(modelNode.Children.[new YamlScalarNode("name")]).Value
        let fieldsNodeChildren =
            getSequenceNode(modelNode.Children.[new YamlScalarNode("fields")]).Children

        let fields =
            fieldsNodeChildren
            |> Seq.map getMappingNode
            |> Seq.map (fun x -> parseModelFieldNode (x))
            |> Seq.toArray
        { Name = nameValue
          Fields = fields }

    let parseYamlRootElement (rootNode : YamlMappingNode) =
        let nsNode =
            getSequenceNode
                (rootNode.Children.[new YamlScalarNode("namespace")])
        let modelsNode =
            getSequenceNode (rootNode.Children.[new YamlScalarNode("models")])

        let nsArray =
            nsNode
            |> Seq.map getScalarNode
            |> Seq.map (fun x -> x.Value)
            |> Seq.toArray

        let modelsArray =
            modelsNode.Children
            |> Seq.map getMappingNode
            |> Seq.map (fun x -> parseModelSequence (x))
            |> Seq.toArray

        { Namespace = nsArray
          Models = modelsArray }

    let parseModelYaml fileName =
        let yaml = File.ReadAllText fileName
        use reader = new StringReader(yaml)
        let stream = YamlStream()
        stream.Load(reader)
        stream.Documents
        |> Seq.map (fun x -> getMappingNode (x.RootNode))
        |> Seq.map parseYamlRootElement
