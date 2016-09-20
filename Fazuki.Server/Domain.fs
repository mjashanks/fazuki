namespace Fazuki.Server

open System
open System.Collections.Generic
open Fazuki.Common

type MessageStream = IObservable<string> 

type UntypedHandler = {
    Consume : obj -> obj;
    Id : string;
    Request : Type;
    Response : Type;
}

type Handler<'req, 'rep> = {
    Consume : 'req -> 'rep;
    Id : string;
}

type ReceiveSuccess = {Id:Guid; EncodedRequest:byte[]}
type DecodeSuccess = {Id:Guid; DecodedRequest:string}
type GetHandlerSuccess = {Id:Guid; Handler:UntypedHandler; Body:string}
type DeserializeSuccess = {Id:Guid; Handler:UntypedHandler; Message:obj}
type ExecuteSuccess = {Id:Guid; Handler:UntypedHandler; Response:obj}
type SerializeSuccess = {Id:Guid; SerializedResponse:string}
type EncodeSuccess = {Id:Guid; EncodedResponse:byte[]}
type SendSuccess = {Id:Guid}

type PipelineSuccess = 
    | ReceiveSuccess of ReceiveSuccess
    | DecodeSuccess of DecodeSuccess
    | GetHandlerSuccess of GetHandlerSuccess
    | DeserializeSuccess of DeserializeSuccess
    | ExecuteSuccess of ExecuteSuccess
    | SerializeSuccess of SerializeSuccess
    | EncodeSuccess of EncodeSuccess
    | SendSuccess of SendSuccess

type PipelineException = {
    PipelineInputs : PipelineSuccess list;
    Exception : Exception
}

type GetHandlerError = 
    | MessageEmpty
    | NoContent
    | NoMessageName
    | HandlerNotFound
    | Unknown of Exception

type ServerError = 
    | ReceiveError of Exception
    | DecodeError of Exception
    | GetHandlerError of GetHandlerError
    | DeserializeError of Exception
    | ExecuteError of Exception
    | SerializeError of Exception
    | EncodeError of Exception
    | SendError of Exception

type PipelineOutput<'res> =
    | Success of 'res
    | Failed of ServerError
  
type ReceiveResult = PipelineOutput<ReceiveSuccess>
type DecodeResult = PipelineOutput<DecodeSuccess>
type GetHandlerResult = PipelineOutput<GetHandlerSuccess>
type DeserializeResult = PipelineOutput<DeserializeSuccess>
type ExecuteResult = PipelineOutput<ExecuteSuccess>
type SerializeResult = PipelineOutput<SerializeSuccess>
type EncodeResult = PipelineOutput<EncodeSuccess>
type SendResult = PipelineOutput<SendSuccess>

type Filter = 
    | ReceiveFilter of (ReceiveResult -> ReceiveResult)
    | DecodeFilter of (DecodeResult -> DecodeResult)
    | GetHandlerFilter of (GetHandlerResult -> GetHandlerResult)
    | DeserializeFilter of (DeserializeResult -> DeserializeResult)
    | ExecuteFilter of (ExecuteResult -> ExecuteResult)
    | SerializeFilter of (SerializeResult -> SerializeResult)
    | EncodeFilter of (EncodeResult -> EncodeResult)
    | SendFilter of (SendResult -> SendResult)

type ServerConfig = {
    Serializer : Serializer
    Handlers : UntypedHandler list 
    Port : Port
    Filers : Filter list
}    


