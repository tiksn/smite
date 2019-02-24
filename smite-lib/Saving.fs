namespace TIKSN.smite.lib

[<AutoOpen>]
module Saving =
    open System.IO

    let saveSourceFile(outputFolderAbsolutePath: string, file: SourceFile) =
        let getAbsoluteFilePath relativeFilePath= Seq.fold (fun acc elem -> Path.Combine(acc, elem)) outputFolderAbsolutePath relativeFilePath
        let absoluteFilePath = getAbsoluteFilePath file.RelativeFilePath
        printfn "%s" absoluteFilePath

    let saveSourceFiles(outputFolderAbsolutePath: string, files: seq<SourceFile>) =
        files
        |> Seq.iter (fun x -> saveSourceFile(outputFolderAbsolutePath, x))