open Docopt
open Docopt.Arguments


// Desired CLI
//let DOC = """
//Herd Inventory
//=============================================================
//
//Usage:
//  cow born <name> --dam=<dam> [--asof=<date>] [--force]...
//  cow died <name> [--asof=<date>] [--force]...
//  cow bought <name> [--asof=<date>] [--force]...
//  cow sold <name> [--asof=<date>]...
//  cow show <name> [--asof=<date>]...
//  cow show --report=<report> [--asof=<date>]
//  cow (-h | --help)
//  cow --version
//
//
//Options:
//  -h --help     Show this screen.
//  --version     Show version.
//"""

let DOC = """
Herd Inventory
=============================================================

Usage:
  cow born <name> --dam=<dam> [--asof=<date>] [--force]...
  cow (-h | --help)
  cow --version


Options:
  -h --help     Show this screen.
  --version     Show version.
"""

let VERSION = "\nHerd Inventory - Version 1.0.0\n\n"
let NOTIMPLEMENTED = "This command has not yet been implmented"
let INVALIDCOMMAND = sprintf "Invalid Command!\n%s" DOC

type BornCommand = {
  Name: string
  Dam: string
}

// "Help" and "Version" are not domain objects, so should not be sent deeper
// hence calling this CliCommand. There should probably be another thing DomainCommand
type CliCommand =
  | Help of string
  | Version of string
  | Invalid of string
  | Born of BornCommand

type CliResponse = CliResponse of string

let printCliResponse (response:CliResponse): unit =
  match response with
  | CliResponse s -> printfn "%s" s

// This is the interface with the domain.
// For "help" and "version" cases, just repackage the input
// But for born, process the domain command, then wrap the result in CliResponse
let handleCliCommand (request:CliCommand): CliResponse =
  match request with
  | Help s -> CliResponse s
  | Version s -> CliResponse s
  | Born r -> CliResponse (sprintf "Saved! {name=%s; dam=%s}" r.Name r.Dam)
  | Invalid x -> CliResponse x


type StopOrContinue =
  | Stop of CliCommand
  | Continue of Docopt.Arguments.Dictionary


let bind continueHandler stopOrContinue =
  match stopOrContinue with
  | Continue parsed -> continueHandler parsed
  | Stop value -> Stop value


let makeBornCommand (parsed:Docopt.Arguments.Dictionary) : CliCommand =
  let herName = 
    match parsed.Item "<name>" with
    | Argument s -> s
    | _ -> "who cares"

  let herDam = 
    match parsed.Item "--dam" with
    | Argument s -> s
    | _ -> "who cares"
  
  Born {Name=herName; Dam=herDam}
  


// switch functions to extract a subcommand and either:
// 1) stop parsing and handle the subcommand
// 2) continue parsing
// TODO: This can be generalized
let stopMaker k f parsed =
  let wrapped (parsed:Docopt.Arguments.Dictionary): StopOrContinue =
    let rs = parsed.Item k
    match rs with
    | Flag _ -> Stop (f parsed)
    | None -> Continue parsed
    | _ -> failwith "????"
  wrapped parsed

let helpStop (parsed:Docopt.Arguments.Dictionary): StopOrContinue =
  let maybeHelp = parsed.Item "-h"
  match maybeHelp with
  | Flag _ -> Stop (Help DOC)
  | None -> Continue parsed
  | _ -> failwith "????"


let versionStop (parsed:Docopt.Arguments.Dictionary): StopOrContinue =
  let maybeVersion = parsed.Item "--version"
  match maybeVersion with
  | Flag _ -> Stop (Version VERSION)
  | None -> Continue parsed
  | _ -> failwith "????"


let bornStop (parsed:Docopt.Arguments.Dictionary): StopOrContinue =
  let maybeBorn = parsed.Item "born"
  match maybeBorn with
  | Flag _ -> Stop (makeBornCommand parsed)
  | None -> Continue parsed
  | _ -> failwith "????"



// wireup: (help | version | born | died | bought | sold ...)
// uses Railway oriented programing style of handling subcommands -
// if one is matched, parsing stops and executes the subcommand
let versionStop' = bind versionStop
let bornStop' = bind bornStop
let convertToCliCommand (input:Docopt.Arguments.Dictionary): CliCommand =
  let railroad = helpStop >> versionStop' >> bornStop'
  input |> railroad |> (fun stopOrContinue ->
    match stopOrContinue with
    | Stop cliCmd -> cliCmd
    | Continue _ -> Invalid INVALIDCOMMAND)


[<EntryPoint>]
let main argv =
  try
    Docopt(DOC).Parse(argv)
    |> convertToCliCommand
    |> handleCliCommand
    |> printCliResponse
    0
  with ArgvException(message) ->
    printfn "Error: %s" message
    -1
