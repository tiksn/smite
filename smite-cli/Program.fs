
open System
open Argu
open TIKSN.smite.cli

type SupportedProgrammingLanguage =
    | FSharp = 1
    | CSharp = 2
    | TypeScript = 3

type CLIArguments =
    | Lang of SupportedProgrammingLanguage
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Lang _ -> "Supported Programming Language"

[<EntryPoint>]
let main argv =
    let configurationRootSetup = new ConfigurationRootSetup()
    let configurationRoot = configurationRootSetup.GetConfigurationRoot();
    let compositionRootSetup = new CompositionRootSetup(configurationRoot)
    let serviceProvider = compositionRootSetup.CreateServiceProvider()
    let parser = ArgumentParser.Create<CLIArguments>(programName = "smite-cli.dll")
    try
        let results =  parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
        let all = results.GetAllResults()
        0
    with e ->
        printfn "%s" e.Message
        1
