namespace Fazuki.Server

open System.Collections.Generic
open System
open fszmq
open System.Text
open Fazuki.Common.Helpers
open Fazuki.Common

module ConsumerExecution = 

    type Consumer = {RequestResponse:RequestResponse; Execute:MessageHandler}
    type private TypedMessageHandler<'a, 'b> =  'a -> 'b 

    let CreateConsumer<'a, 'b> (handler:TypedMessageHandler<'a,'b>) =
        {RequestResponse= {Req=typeof<'a>; Rep=typeof<'b>}; 
        Execute= fun ob -> (handler (ob:?>'a)) :> obj}

    let private CreateHandlerFactory consumers = 
        let handlers = consumers 
                        |> List.map (fun c -> (c.RequestResponse.Req, c.Execute))
                        |> dict
        (fun t ->
            if handlers.ContainsKey(t) = false then failwith(sprintf "Could not find handler for: %s" t.FullName)
            handlers.[t])

    let internal CreateExecute consumers = 
        let factory = CreateHandlerFactory consumers
        fun obj -> obj |> (factory (obj.GetType())) 

    let Use consumers (config:ServerConfig) =
        {config with Execute = CreateExecute consumers}

    let BuildMessageTypes consumers = 
        consumers 
        |> Seq.map (fun c -> ((getTypeRoute c.RequestResponse.Req), c.RequestResponse))
        |> dict

module Config =

    let private stubSerializer = {
        Serialize = fun t o ->  failwith "serializer not set";
        Deserialize = fun t s -> failwith "serializer not set";
    }

    let IsAlwaysRunning () = true 

    let InitialiseServerConfig consumers = 
        {Serializer = stubSerializer;
         Execute = ConsumerExecution.CreateExecute consumers
         MessageTypes = MessageTypes(ConsumerExecution.BuildMessageTypes consumers)
         Port = Port.None
         IsRunning = IsAlwaysRunning}

    type internal ConfiguredServerInstance(config:ServerConfig) = 
        let context = new Context ()

        let conn = 
            match config.Port with
            | None -> failwith("Port not set!")
            | Some i -> sprintf "tcp://*:%i" i 

        let responder  = 
            let resp = Context.rep context 
            conn |> Socket.bind resp
            resp
        // this is a class just made to make the pipline below nice and readable
        member c.Deserialize (req_rep,body) = 
            req_rep, (config.Serializer.Deserialize req_rep.Req body)

        member c.Execute (req_rep, msg) = 
            config.Execute msg

        member c.Serialize obj =
            config.Serializer.Serialize (getType obj) obj

        member c.GetMessageType (message:string) = 
            let splitMsg = message.Split(':')
            let name = splitMsg.[0]
            let body = splitMsg.[1]
            if config.MessageTypes.ContainsKey (if name=null then "" else name) = false then 
                failwith(sprintf "message %s not found" message)
            config.MessageTypes.[name], body 

        member c.Receive () :byte[] = 
            responder |> Socket.recv

        member c.Send message = 
            Socket.send responder message
