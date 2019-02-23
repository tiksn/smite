namespace TIKSN.smite.lib

[<AutoOpen>]
module Parser =
    open System.IO
    open YamlDotNet.RepresentationModel
    open System

    let parseModelYamlDocument (document: YamlDocument) =
        let doc  = document
        let t = doc.RootNode.NodeType.ToString()
        printfn "%s" t

    let parseModelYaml fileName =
        let yaml = File.ReadAllText fileName
        use reader = new StringReader(yaml)
        let stream = YamlStream()
        stream.Load(reader)
        stream.Documents
        |> Seq.iter parseModelYamlDocument