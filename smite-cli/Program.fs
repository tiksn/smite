﻿
open System
open Argu
open TIKSN.smite.cli
open TIKSN.Time
open Microsoft.Extensions.DependencyInjection
open System
open System.IO

type SupportedProgrammingLanguage =
    | FSharp = 1
    | CSharp = 2
    | TypeScript = 3

type CLIArguments =
    | [<Mandatory>] Input_File of string
    | [<Mandatory>] Output_Folder of string
    | [<Mandatory>] Lang of SupportedProgrammingLanguage
with
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Input_File _ -> "Input YAML file"
            | Output_Folder _ -> "Output Folder for model sources files to be generated in."
            | Lang _ -> "Supported Programming Language"

[<EntryPoint>]
let main argv =
    let configurationRootSetup = new ConfigurationRootSetup()
    let configurationRoot = configurationRootSetup.GetConfigurationRoot();
    let compositionRootSetup = new CompositionRootSetup(configurationRoot)
    let serviceProvider = compositionRootSetup.CreateServiceProvider()
    let timeProvider = serviceProvider.GetRequiredService<ITimeProvider>()
    let parser = ArgumentParser.Create<CLIArguments>(programName = "smite-cli.dll")
    try
        let results =  parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
        let all = results.GetAllResults()
        let inputFilePath = results.GetResult(Input_File)
        let outputFolderPath = results.GetResult(Output_Folder)
        let lang = results.GetResult(Lang)
        let langName = lang.ToString()
        let inputFileAbsolutePath = Path.GetFullPath(inputFilePath)
        let outputFolderAbsolutePath = Path.GetFullPath(outputFolderPath)
        printfn "Reading models definitions from %s" inputFileAbsolutePath
        printfn "Writing models %s source files into %s" langName outputFolderAbsolutePath
        0
    with e ->
        printfn "%s" e.Message
        1
