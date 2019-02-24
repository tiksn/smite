namespace TIKSN.smite.lib

module CommonFeatures =
    let getStartSegments(ns: string[], startingSegmentsCount) =
        let left, _ = ns |> Array.splitAt(startingSegmentsCount)
        left

    let getEndSegments(ns: string[], startingSegmentsCount) =
        let _, right = ns |> Array.splitAt(startingSegmentsCount)
        right

    let hasSameStartSegments(nsSeq: seq<string[]>, startingSegmentsCount) =
        let nsSeqDistinct = (nsSeq
        |> Seq.map (fun x -> getStartSegments(x, startingSegmentsCount))
        |> Seq.distinct
        |> Seq.toArray)
        nsSeqDistinct.Length = 1

    let getFilespaceDefinition(namespaceDefinitions: seq<NamespaceDefinition>) =
        let nsSeq = namespaceDefinitions |> Seq.map (fun x -> x.Namespace)
        let minNamespaceLength = namespaceDefinitions |> Seq.map (fun x -> x.Namespace.Length) |> Seq.min
        let numberOfCommonSegments = seq { 0 .. minNamespaceLength } |> Seq.where (fun x -> hasSameStartSegments(nsSeq, x)) |> Seq.max
        namespaceDefinitions
        |> Seq.map (fun x -> { Namespace=x.Namespace; Filespace=getEndSegments(x.Namespace, numberOfCommonSegments) |> Seq.toArray; Models=x.Models })