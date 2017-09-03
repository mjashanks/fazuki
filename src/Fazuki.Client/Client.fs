namespace Fazuki.Client

open System.Collections.Generic
open System
open NetMQ
open NetMQ.Sockets
open System.Text
open Fazuki.Common.Helpers
open Fazuki.Client
open Fazuki.Common
open System.Threading

type ConfiguredClientInstance(config) =
    let socket = 
        let addr = match config.Url with
                    | u when String.IsNullOrWhiteSpace(u) ->  failwith "target url not set"
                    | u -> (sprintf "tcp://%s" u)
        new RequestSocket(addr)

    member c.Deserialize<'rep> body = 
        config.Serializer.Deserialize typeof<'rep> body       
    member c.Serialize ob =
        config.Serializer.Serialize (getType ob) ob
    member c.ExtractSuccessfullness byt =
        byt[0]

    member c.AddHeader name serializedOb = 
        serializedOb
        |> Array.append (handlerNameToId name)

    member c.Send (message:byte[]) = 
        socket.SendFrame (message, false)

    member c.Receive () = 
        let b = socket.ReceiveFrameBytes()
        printfn "received: %s" (Encoding.UTF8.GetString b)
        b
     