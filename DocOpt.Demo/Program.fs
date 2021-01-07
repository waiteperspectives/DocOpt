open Docopt

let doc = """
Naval Fate.

Usage:
  naval_fate.exe ship new <name>...
  naval_fate.exe ship <name> move <x> <y> [--speed=<kn>]
  naval_fate.exe ship shoot <x> <y>
  naval_fate.exe mine (set|remove) <x> <y> [--moored | --drifting]
  naval_fate.exe (-h | --help)
  naval_fate.exe --version

Options:
  -h --help     Show this screen.
  --version     Show version.
  --speed=<kn>  Speed in knots [default: 10].
  --moored      Moored (anchored) mine.
  --drifting    Drifting mine.
"""

[<EntryPoint>]
let main argv =
  let docopt = Docopt(doc)
  try
    let dict = docopt.Parse(argv)
    printfn "Success:\n%A" dict
    0
  with ArgvException(message) ->
    printfn "Error: %s" message
    -42
