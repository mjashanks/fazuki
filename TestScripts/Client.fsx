#r "../Fazuki.Client/bin/Debug/fszmq.dll"
#r "../Fazuki.Client/bin/Debug/Fazuki.Common.dll"
#r "../Fazuki.Client/bin/Debug/Fazuki.Client.dll"
#r "../Fazuki.NewtonsoftJson/bin/Debug/Fazuki.NewtonsoftJson.dll"
#r "../Fazuki.NewtonsoftJson/bin/Debug/Newtonsoft.Json.dll"

open fszmq
open Fazuki.Client
open Fazuki.NewtonsoftJson

type GetCreatures = {
    CreatureType : string
}

type Creature = {
    Type : string;
    Name : string;
}

let config =
    {Url = "localhost:5555";
     Serializer = Serialization.NewtonsoftSerializer}

printfn "config: %s" config.Url

let client = Client(config)

for i in [0..100] do
    client.Send<Creature list> "get_creatures" ({CreatureType="Dog"})
    |> Seq.iter (fun c -> printfn "CreatureName = %s " c.Name)

