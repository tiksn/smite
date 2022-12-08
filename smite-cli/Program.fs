open Argu
open TIKSN.Time
open Microsoft.Extensions.DependencyInjection
open System.IO
open TIKSN.smite.lib
open Microsoft.Extensions.Hosting
open TIKSN.DependencyInjection

type SupportedProgrammingLanguage =
    | FSharp = 1
    | CSharp = 2
    | VisualBasic = 3
    | TypeScript = 4

type CLIArguments =
    | [<Mandatory>] Input_File of string
    | [<Mandatory>] Output_Folder of string
    | [<Mandatory>] Lang of SupportedProgrammingLanguage
    | Field of FieldKind

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Input_File _ -> "Input YAML file"
            | Output_Folder _ -> "Output Folder for model sources files to be generated in."
            | Lang _ -> "Supported Programming Language"
            | Field _ -> "Filed structure kind"

[<EntryPoint>]
let main args =

    let builder =
        Host
            .CreateDefaultBuilder()
            .ConfigureServices(fun hostContext services -> services.AddFrameworkPlatform() |> ignore)

    let app = builder.Build()

    let serviceProvider = app.Services

    let timeProvider = serviceProvider.GetRequiredService<ITimeProvider>()

    let parser = ArgumentParser.Create<CLIArguments>(programName = "smite")

    try
        let results = parser.ParseCommandLine(inputs = args, raiseOnUsage = true)

        let inputFilePath = results.GetResult(Input_File)
        let outputFolderPath = results.GetResult(Output_Folder)
        let lang = results.GetResult(Lang)

        let field = results.GetResult(Field, FieldKind.Field)

        let langName = lang.ToString()
        let inputFileAbsolutePath = Path.GetFullPath(inputFilePath)
        let outputFolderAbsolutePath = Path.GetFullPath(outputFolderPath)
        printfn "Reading models definitions from %s" inputFileAbsolutePath
        let models = parseModelYaml (inputFileAbsolutePath)

        let files =
            match lang with
            | SupportedProgrammingLanguage.CSharp -> CSharpTranspiler.transpile (models, field, timeProvider)
            | SupportedProgrammingLanguage.FSharp -> FSharpTranspiler.transpile (models, timeProvider)
            | SupportedProgrammingLanguage.VisualBasic -> VisualBasicTranspiler.transpile (models, field, timeProvider)
            | SupportedProgrammingLanguage.TypeScript -> TypeScriptTranspiler.transpile (models, timeProvider)

        saveSourceFiles (outputFolderAbsolutePath, files)
        printfn "Writing models %s source files into %s" langName outputFolderAbsolutePath
        0
    with e ->
        printfn "%s" e.Message
        1
