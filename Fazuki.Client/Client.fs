namespace Fazuki.Client

open System.Collections.Generic
open System
open fszmq
open System.Text
open Fazuki.Common.Helpers
open Fazuki.Client
open Fazuki.Common
open System.Threading

type ConfiguredClientInstance(config) =
    let context = new Context()
    let socket = 
        let addr = match config.Url with
                    | u when String.IsNullOrWhiteSpace(u) ->  failwith "target url not set"
                    | u -> (sprintf "tcp://%s" u)

        let clientsocket  = Context.req context
        Socket.connect clientsocket addr
        printfn "connecting to %s" addr
        clientsocket

    member c.Deserialize<'rep> body = 
        config.Serializer.Deserialize typeof<'rep> body       
    member c.Serialize ob =
        config.Serializer.Serialize (getType ob) ob
    member c.ExtractSuccessfullness byt =
        byte[0]

    member c.AddHeader name serializedOb = 
        serializedOb
        |> Array.append (handlerNameToId name)

    member c.Send message = 
        Socket.send socket message

    member c.Receive () = 
        let b = Socket.recv socket
        printfn "received: %s" (Encoding.UTF8.GetString b)
        b
     