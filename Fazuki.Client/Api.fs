namespace Fazuki.Client

open fszmq
open System.Text
open Fazuki.Client
open System

type Client(config) = 
    let instance = ConfiguredClientInstance config

    member c.Send<'rep> name req = 
        let cast (o:obj) = o :?> 'rep

        // SEND REQUEST
        req
        |> instance.Serialize
        |> instance.AddHeader name
        |> instance.Send

        // RECEIVE REPLY
        instance.Receive ()
        |> instance.Deserialize 
        |> cast 

        