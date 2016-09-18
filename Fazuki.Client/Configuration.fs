namespace Fazuki.Client

open System.Collections.Generic
open System
open fszmq
open System.Text
open Fazuki.Common.Helpers
open Fazuki.Client
open Fazuki.Common

module Config =

    let private stubSerializer = 
        {Serialize = (fun t o ->  failwith "serializer not set");
        Deserialize = (fun t s -> failwith "serializer not set")}

    let IsAlwaysRunning () = true 

    let InitialiseClientConfig url = 
        {Url=url; Serializer=stubSerializer}

    type internal ConfiguredClientInstance<'rep>(config) =
        member c.Config = config 
        member c.Socket = 
            let addr = match config.Url with
                        | None -> failwith "target url not set"
                        | Some u when String.IsNullOrWhiteSpace(u) ->  failwith "target url not set"
                        | Some u -> (sprintf "tcp://%s" u)

            let clientsocket  = Context.req (new Context())
            Socket.connect clientsocket addr
            clientsocket

        member c.Deserialize body = 
            config.Serializer.Deserialize typeof<'rep> body       
        member c.Serialize ob =
            config.Serializer.Serialize (getType ob) ob
        member c.Encode (str:string) = 
            Encoding.UTF8.GetBytes str
        member c.AddHeader serializedOb = 
            sprintf "%s:%s" (getObjRoute<'rep> ()) serializedOb
        member c.Send message = 
            Socket.send c.Socket message
        member c.Receive () = 
            Socket.recv c.Socket
        member c.Decode msg =
            Encoding.UTF8.GetString msg