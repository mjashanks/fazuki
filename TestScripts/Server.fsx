#r "../Fazuki.Server/bin/Debug/Fazuki.Server.dll"
#r "../Fazuki.Server/bin/Debug/Fazuki.Common.dll"
#r "../Fazuki.Jil/bin/Debug/Fazuki.Jil.dll"

open Fazuki.Server
open Fazuki.Jil

type GetCreatures = {
    CreatureType : string
}

type Creature = {
    Type : string;
    Name : string;
}

let configureServer consumers=
    {(Config.InitialiseServerConfig consumers) with
        Serializer = Serialization.JilSerializer
        Port = Some(4567)}

let CreatureConsumer req = 
    printf "Requested: %s" req.CreatureType
    [{Name="Bobby"; Type="Dog"};
     {Name="Fern"; Type="Princess"}]
    |> List.filter (fun c -> c.Type = req.CreatureType)

let Consumers = 
    [CreateConsumer<CreaturesInMyHomeRequest, Creatures> CreatureConsumer]

Consumers
|> configureServer
|> Server.Start