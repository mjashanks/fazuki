namespace Fazuki.Server

open System.Collections.Generic
open System
open fszmq
open System.Text
open Fazuki.Common.Helpers
open Fazuki.Common

module ConsumerCollectionFactory = 
    
    let AddConsumer<'req,'rep> (consumer:Consumer<'req,'rep>) (collection:list<UntypedConsumer>) = 
        let untyped = {Consume = (fun req -> (consumer.Consume (req :?> 'req)) :> obj)
                       Id=consumer.Id; 
                       Request=typeof<'req>; 
                       Response=typeof<'rep> }                        
        List.append collection [untyped]

module Config =

    let private PipelineWrap<'inp, 'out> 
                    (input:PipelineOutput<'inp>) 
                    (onSuccess:'inp -> PipelineOutput<'out>) =
            match input with 
            | Failed(s) -> Failed(s)
            | Success(i) -> 
                try onSuccess i 
                with 
                | ex -> 
                    Failed
                        (match typeof<'out> with
                        | t when t = typeof<ReceiveResult> -> ReceiveError ex
                        | t when t = typeof<DecodeResult> -> DecodeError ex
                        | t when t = typeof<GetConsumerResult> -> GetConsumerError <| GetConsumerError.Unknown(ex)
                        | t when t = typeof<DeserializeResult> -> DeserializeError ex
                        | t when t = typeof<ExecuteResult> -> ExecuteError ex
                        | t when t = typeof<SerializeResult> -> SerializeError ex
                        | t when t = typeof<EncodeResult> -> EncodeError ex
                        | _ -> failwith "should be impossible but i dont know how to restrict it at compile time")
                    

    type internal ConfiguredServerInstance(config:ServerConfig) = 
        let context = new Context ()

        let mutable isRunning = true

        let log level message = 
            config.Loggers |> Seq.iter (fun l -> l level message)

        let conn = 
            match config.Port with
            | None -> failwith "Port not set!"
            | Some i -> sprintf "tcp://*:%i" i 

        let responder  = 
            let resp = Context.rep context 
            conn |> Socket.bind resp
            resp     
       
        member c.Receive () : ReceiveResult = 
            try Success(responder |> Socket.recv) with | ex -> Failed(ReceiveError ex)

        member c.Decode result : DecodeResult=
            PipelineWrap 
                result 
                (fun b -> Success(Encoding.UTF8.GetString(b)))                

        member c.GetConsumer (message:DecodeResult) : GetConsumerResult = 
            PipelineWrap message (fun msg -> 
                
                let bodyStart = msg.IndexOf(":")
                let name, body = 
                    match bodyStart with
                    | -1 -> "",""
                    | s when s = msg.Length - 1 -> msg.Substring(0, s-1), ""
                    | s -> msg.Substring(0,s-2), msg.Substring(s+1, msg.Length - s+2)

                match name,body with
                | "","" -> Failed <| GetConsumerError(MessageEmpty)
                | n,"" -> Failed <| GetConsumerError(NoContent)
                | "",b -> Failed <| GetConsumerError(NoMessageName)
                | m,b  ->     
                    (match config.Consumers |> List.tryFind (fun c -> c.Id = name) with
                    | Some consumer -> Success(consumer,body)
                    | None -> Failed <| GetConsumerError(HandlerNotFound))
                    
                )
        // this is a class just made to make the pipline below nice and readable
        member c.Deserialize consumerResult : DeserializeResult=
            PipelineWrap consumerResult (fun r ->
                let consumer, body = r  
                Success(consumer, (config.Serializer.Deserialize consumer.Request body)))

        member c.Execute (deserializedResult:PipelineOutput<UntypedConsumer * obj>)  = 
            PipelineWrap deserializedResult 
                            (fun r -> 
                                let consumer,msg = r
                                Success(consumer, (consumer.Consume msg)))

        member c.Serialize response : SerializeResult=
            PipelineWrap response 
                (fun r -> 
                    let consumer, obj = r
                    Success(config.Serializer.Serialize (consumer.Response) obj))

        member c.Encode message : EncodeResult= 
            PipelineWrap message (fun (m:string) -> Success(Encoding.UTF8.GetBytes(m)))
       
        member c.Send message : SendResult =
            let replyBytes = 
                match message with
                | Failed(s) -> [||] // need to define thie
                | Success(b) -> b
            try Success(Socket.send responder replyBytes) 
            with | ex -> Failed(SendError ex) // need to define this

        member c.IsRunning () = 
            isRunning
       
        member c.Stop () =
            isRunning <- false