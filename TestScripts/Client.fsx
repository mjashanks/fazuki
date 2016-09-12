#r "../Zoco/bin/Debug/fszmq.dll"
#r "../Zoco/bin/Debug/Zoco.dll"
#r "../Zoco.Jil/bin/Debug/Zoco.Jil.dll"
#r "./bin/Debug/ZocoTestApps.dll"
#r "../Zoco.Jil/bin/Debug/Sigil.dll"
#r "../Zoco.Jil/bin/Debug/Jil.dll"

open Zoco
open Zoco.Jil
open Zoco.ConsumerExecution
open ZocoTestApps.Domain

let config =
    {Url = Some("localhost:4567")
     Serializer = Serialization.JilSerializer}

printfn "config: %s" (match config.Url with | Some(s) -> s | None -> "")

Client.Send<Creatures> config {CreatureType="Dog"}
|> Seq.iter (fun c -> printfn "CreatureName = %s " c.Name)

