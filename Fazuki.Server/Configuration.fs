namespace Fazuki.Server

open System.Collections.Generic
open System
open fszmq
open System.Text
open Fazuki.Common.Helpers
open Fazuki.Common

module HandlerCollectionFactory = 
    
    let AddHandler<'req,'rep> (handler:Handler<'req,'rep>) (collection:list<UntypedHandler>) = 
        let untyped = {Consume = (fun req -> (handler.Consume (req :?> 'req)) :> obj)
                       Id=handler.Id; 
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
                        | t when t = typeof<GetHandlerResult> -> GetHandlerError <| GetHandlerError.Unknown(ex)
                        | t when t = typeof<DeserializeResult> -> DeserializeError ex
                        | t when t = typeof<ExecuteResult> -> ExecuteError ex
                        | t when t = typeof<SerializeResult> -> SerializeError ex
                        | t when t = typeof<EncodeResult> -> EncodeError ex
                        | _ -> failwith "should be impossible but i dont know how to restrict it at compile time")
                    

    type internal ConfiguredServerInstance(config:ServerConfig) = 
        let context = new Context ()

        let mutable isRunning = true

        let conn = 
            match config.Port with
            | None -> failwith "Port not set!"
            | Some i -> sprintf "tcp://*:%i" i 

        let responder  = 
            let resp = Context.rep context 
            conn |> Socket.bind resp
            resp     
       
        member c.Receive () : ReceiveResult = 
            try Success(
                    {Id=Guid.NewGuid(); 
                    EncodedRequest=responder |> Socket.recv}) 
            with | ex -> Failed(ReceiveError ex)

        member c.Decode result : DecodeResult=
            PipelineWrap 
                result 
                (fun (b:ReceiveSuccess) -> Success(
                                            {Id=b.Id; 
                                            DecodedRequest=Encoding.UTF8.GetString(b.EncodedRequest)}))                

        member c.GetHandler (message:DecodeResult) : GetHandlerResult = 
            PipelineWrap message (fun (msg:DecodeSuccess) -> 
                let decodedReq = msg.DecodedRequest
                let bodyStart = decodedReq.IndexOf(":")
                let name, body = 
                    match bodyStart with
                    | -1 -> "",""
                    | s when s = decodedReq.Length - 1 -> decodedReq.Substring(0, s-1), ""
                    | s -> decodedReq.Substring(0,s-2), decodedReq.Substring(s+1, decodedReq.Length - s+2)

                match name,body with
                | "","" -> Failed <| GetHandlerError(MessageEmpty)
                | n,"" -> Failed <| GetHandlerError(NoContent)
                | "",b -> Failed <| GetHandlerError(NoMessageName)
                | m,b  ->     
                    (match config.Handlers |> List.tryFind (fun c -> c.Id = name) with
                    | None -> Failed <| GetHandlerError(HandlerNotFound)
                    | Some handler -> Success(
                                        {Id=msg.Id;
                                        Body=body;
                                        Handler=handler})
                ))
        // this is a class just made to make the pipline below nice and readable
        member c.Deserialize handlerResult : DeserializeResult=
            PipelineWrap handlerResult (fun (r:GetHandlerSuccess) ->  
                Success(
                    {Id=r.Id; 
                    Handler=r.Handler; 
                    Message= config.Serializer.Deserialize r.Handler.Request r.Body}))

        member c.Execute deserializedResult : ExecuteResult  = 
            PipelineWrap deserializedResult 
                            (fun (r:DeserializeSuccess) -> 
                                Success(
                                    {Id=r.Id;
                                    Handler=r.Handler;
                                    Response=r.Handler.Consume r.Message}))

        member c.Serialize response : SerializeResult =
            PipelineWrap response 
                (fun (r:ExecuteSuccess) -> 
                    Success(
                        {Id=r.Id;
                        SerializedResponse=config.Serializer.Serialize (r.Handler.Response) r.Response}))

        member c.Encode message : EncodeResult= 
            PipelineWrap message (fun (m:SerializeSuccess) -> Success(
                                                        {Id=m.Id;
                                                        EncodedResponse=Encoding.UTF8.GetBytes(m.SerializedResponse)}))
       
        member c.Send (message:EncodeResult) : SendResult =
            let replyBytes = 
                match message with
                | Failed(s) -> [||] // need to define thie
                | Success(b) -> b.EncodedResponse
            try 
                Socket.send responder replyBytes
                Success
            with | ex -> Failed(SendError ex) // need to define this

        member c.IsRunning () = 
            isRunning
       
        member c.Stop () =
            isRunning <- false