namespace TIKSN.smite.lib

module CommonFeatures =
    open System
    open TIKSN.Time

    let getFileComment (timeProvider: ITimeProvider) =
        let time = timeProvider.GetCurrentTime().ToString("D")
        [ "This file is auto-generated"
          "Please do not make manual changes"

          sprintf "File is generated via smite, DTO transpiler (https://github.com/tiksn/smite) on %s" time ]

    let getFieldTypeSyntaxNode (fieldType: FieldType, getSpecialType) =
        match fieldType with
        | PrimitiveType primitiveType -> (None, getSpecialType (primitiveType))
        | ComplexTypeSameNamespace typeName -> (None, typeName)
        | ComplexTypeDifferentNamespace(nsArray, typeName) -> (Some nsArray, typeName)

    let getStartSegments (ns: string [], startingSegmentsCount) =
        let left, _ = ns |> Array.splitAt (startingSegmentsCount)
        left

    let getEndSegments (ns: string [], startingSegmentsCount) =
        let _, right = ns |> Array.splitAt (startingSegmentsCount)
        right

    let hasSameStartSegments (nsSeq: seq<string []>, startingSegmentsCount) =
        let nsSeqDistinct =
            (nsSeq
             |> Seq.map (fun x -> getStartSegments (x, startingSegmentsCount))
             |> Seq.distinct
             |> Seq.toArray)
        nsSeqDistinct.Length = 1

    let getFilespaceDefinitions (namespaceDefinitions: seq<NamespaceDefinition>) =
        let nsSeq = namespaceDefinitions |> Seq.map (fun x -> x.Namespace)

        let minNamespaceLength =
            namespaceDefinitions
            |> Seq.map (fun x -> x.Namespace.Length)
            |> Seq.min

        let numberOfCommonSegments =
            seq { 0 .. minNamespaceLength }
            |> Seq.where (fun x -> hasSameStartSegments (nsSeq, x))
            |> Seq.max

        namespaceDefinitions
        |> Seq.map (fun x ->
            { Namespace = x.Namespace
              Filespace = getEndSegments (x.Namespace, numberOfCommonSegments) |> Seq.toArray
              Models = x.Models })

    let getFilespaceDefinitionsForRootOnlyNamespaces (namespaceDefinitions: seq<NamespaceDefinition>) =
        namespaceDefinitions
        |> Seq.groupBy (fun x -> x.Namespace.[0])
        |> Seq.map (fun (ns, nsds) ->
            { Namespaces = nsds |> Seq.toArray
              Filespace = [| ns |] })

    let composeDotSeparatedNamespace (ns: string []) = String.Join(".", ns)

    let rec getFilespacesWithExtension (filespaces: string list, fileExtension: string) =
        match filespaces with
        | hd :: [] -> [ hd + fileExtension ]
        | hd :: tl -> hd :: getFilespacesWithExtension (tl, fileExtension)
        | _ -> failwith "Empty list."

    let getFilePathWithExtension (filespaceDefinition: SingleNamespaceFilespaceDefinition, fileExtension) =
        let filespacesWithExtension =
            getFilespacesWithExtension ((filespaceDefinition.Filespace |> Array.toList), fileExtension)
        filespacesWithExtension |> List.toArray

    let getFilePathWithExtensionForMultiNamespace (filespaceDefinition: MultiNamespaceFilespaceDefinition, fileExtension) =
        let filespacesWithExtension =
            getFilespacesWithExtension ((filespaceDefinition.Filespace |> Array.toList), fileExtension)
        filespacesWithExtension |> List.toArray
