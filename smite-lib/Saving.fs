namespace TIKSN.smite.lib

[<AutoOpen>]
module Saving =
    open System.IO

    let saveSourceFile (outputFolderAbsolutePath: string, file: SourceFile) =
        let getAbsoluteFilePath relativeFilePath =
            Seq.fold (fun acc elem -> Path.Combine(acc, elem)) outputFolderAbsolutePath relativeFilePath

        let absoluteFilePath =
            getAbsoluteFilePath file.RelativeFilePath

        let absoluteFolderPath = Path.GetDirectoryName absoluteFilePath
        printfn "Creating folder %s" absoluteFolderPath

        Directory.CreateDirectory(absoluteFolderPath)
        |> ignore

        printfn "Writing file %s" absoluteFilePath
        File.WriteAllText(absoluteFilePath, file.FileContent)

    let saveSourceFiles (outputFolderAbsolutePath: string, files: seq<SourceFile>) =
        files
        |> Seq.iter (fun x -> saveSourceFile (outputFolderAbsolutePath, x))
