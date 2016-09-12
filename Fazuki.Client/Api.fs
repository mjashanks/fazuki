namespace Fazuki.Client

open fszmq
open System.Text
open Fazuki.Client.Config
open System

module Client = // the client pipline
    
    let Send<'rep> config req = 
        let Instance = ConfiguredClientInstance<'rep>(config)
        let cast (o:obj) = o :?> 'rep

        // SEND REQUEST
        req
        |> Instance.Serialize
        |> Instance.AddHeader 
        |> Instance.Encode
        |> Instance.Send

        // RECEIVE REPLY
        Instance.Receive ()
        |> Instance.Decode
        |> Instance.Deserialize 
        |> cast    

        