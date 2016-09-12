#r "../Zoco/bin/Debug/Zoco.dll"
#r "../Zoco.Jil/bin/Debug/Zoco.Jil.dll"
#r "./bin/Debug/ZocoTestApps.dll"


open Zoco
open Zoco.Jil
open Zoco.ConsumerExecution
open ZocoTestApps.Domain

let configureServer consumers=
    {(Config.InitialiseServerConfig consumers) with
        Serializer = Serialization.JilSerializer
        Port = Some(4567)}

let CreatureConsumer req = 
    printf "Requested: %s" req.CreatureType
    [{Name="Bobby"; CreatureType="Dog"};
     {Name="Fern"; CreatureType="Princess"}]
    |> List.filter (fun c -> c.CreatureType = req.CreatureType)

let Consumers = 
    [CreateConsumer<CreaturesInMyHomeRequest, Creatures> CreatureConsumer]

Consumers
|> configureServer
|> Server.Start