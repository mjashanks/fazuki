#r "../Fazuki.Client/bin/Debug/fszmq.dll"
#r "../Fazuki.Client/bin/Debug/Fazuki.Client.dll"
#r "../Fazuki.Client/bin/Debug/Fazuki.Common.dll"
#r "../Fazuki.Jil/bin/Debug/Fazuki.Jil.dll"
#r "../Fazuki.Jil/bin/Debug/Sigil.dll"
#r "../Fazuki.Jil/bin/Debug/Jil.dll"

open Fazuki.Client
open Fazuki.Jil

type GetCreatures = {
    CreatureType : string
}

type Creature = {
    Type : string;
    Name : string;
}

let config =
    {Url = Some("localhost:4567");
     Serializer = Serialization.JilSerializer}

printfn "config: %s" (match config.Url with | Some(s) -> s | None -> "")

Client.Send<Creature list> config {CreatureType="Dog"}
|> Seq.iter (fun c -> printfn "CreatureName = %s " c.Name)

