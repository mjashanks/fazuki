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
            match input.StepResult with 
            | Failed(s) -> {Id=input.Id; StepResult=Failed(s)}
            | Success(i) -> 
                try onSuccess i 
                with 
                | ex -> 
                    {Id=input.Id;
                    StepResult=Failed
                        (match typeof<'out> with
                        | t when t = typeof<ReceiveResult> -> ReceiveError ex
                        | t when t = typeof<GetHandlerResult> -> GetHandlerError <| GetHandlerError.Unknown(ex)
                        | t when t = typeof<DeserializeResult> -> DeserializeError ex
                        | t when t = typeof<ExecuteResult> -> ExecuteError ex
                        | t when t = typeof<SerializeResult> -> SerializeError ex
                        | _ -> failwith "should be impossible but i dont know how to restrict it at compile time")}
                    

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
        
        let OutputFail id error =
            {Id=id; StepResult=Failed(error)}

        let OutputSuccess id success = 
            {Id=id; StepResult=Success(success)}

        member c.Receive () : ReceiveResult = 
            let id = Guid.NewGuid()
            try OutputSuccess id {EncodedRequest=responder |> Socket.recv}
            with | ex -> OutputFail id (ReceiveError ex)

        member c.GetHandler (message:ReceiveResult) : GetHandlerResult = 
            PipelineWrap message (fun (msg:ReceiveSuccess) -> 

                let getHandlerAndBody (req:byte[]) =
                    let name = Encoding.UTF8.GetString(req |> Seq.take 100 |> Array.ofSeq)
                    match config.Handlers |> List.tryFind (fun c -> c.Id = name) with
                    | None -> OutputFail message.Id <| GetHandlerError(HandlerNotFound)
                    | Some handler -> OutputSuccess message.Id
                                                    {Body=(req |> Seq.skip 100 |> Array.ofSeq) ;
                                                    Handler=handler}

                match msg.EncodedRequest.Length with
                | l when l < 100 -> OutputFail message.Id <| GetHandlerError(NoMessageName)
                | l when l = 100 -> OutputFail message.Id <| GetHandlerError(NoContent)
                | _ -> getHandlerAndBody msg.EncodedRequest  

                )
        // this is a class just made to make the pipline below nice and readable
        member c.Deserialize handlerResult : DeserializeResult=
            PipelineWrap handlerResult (fun (r:GetHandlerSuccess) ->  
                OutputSuccess handlerResult.Id
                            {Handler=r.Handler; 
                            Message= config.Serializer.Deserialize r.Handler.Request r.Body})

        member c.Execute deserializedResult : ExecuteResult  = 
            PipelineWrap deserializedResult 
                            (fun (r:DeserializeSuccess) -> 
                                OutputSuccess deserializedResult.Id
                                            {Handler=r.Handler;
                                            Response=r.Handler.Consume r.Message})

        member c.Serialize response : SerializeResult =
            PipelineWrap response 
                (fun (r:ExecuteSuccess) -> 
                    OutputSuccess response.Id
                                  {SerializedResponse=config.Serializer.Serialize (r.Handler.Response) r.Response})

         member c.Send (message:SerializeResult) : SendResult =
            let replyBytes = 
                match message.StepResult with
                | Failed(s) -> [||] // need to define thie
                | Success(b) -> b.SerializedResponse
            try 
                Socket.send responder replyBytes
                OutputSuccess message.Id ()
            with | ex -> OutputFail message.Id <| SendError ex // need to define this

        member c.IsRunning () = 
            isRunning
       
        member c.Stop () =
            isRunning <- false