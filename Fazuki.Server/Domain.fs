namespace Fazuki.Server

open System
open System.Collections.Generic
open Fazuki.Common

type MessageStream = IObservable<string> 

type UntypedConsumer = {
    Consume : obj -> obj;
    Id : string;
    Request : Type;
    Response : Type;
}

type Consumer<'req, 'rep> = {
    Consume : 'req -> 'rep;
    Id : string;
}

type GetConsumerError = 
    | MessageEmpty
    | NoContent
    | NoMessageName
    | HandlerNotFound
    | Unknown of Exception

type ServerError = 
    | ReceiveError of Exception
    | DecodeError of Exception
    | GetConsumerError of GetConsumerError
    | DeserializeError of Exception
    | ExecuteError of Exception
    | SerializeError of Exception
    | EncodeError of Exception
    | SendError of Exception

type PipelineOutput<'res> =
    | Success of 'res
    | Failed of ServerError

type ReceiveSuccess = {Id:Guid; EncodedRequest:byte[]}
type DecodeSuccess = {Id:Guid; DecodedRequest:string}
type GetConsumerSuccess = {Id:Guid; Consumer:UntypedConsumer; Body:string}
type DeserializeSuccess = {Id:Guid; Consumer:UntypedConsumer; Message:obj}
type ExecuteSuccess = {Id:Guid; Consumer:UntypedConsumer; Response:obj}
type SerializeSuccess = {Id:Guid; SerializedResponse:string}
type EncodeSuccess = {Id:Guid; EncodedResponse:string}
type SendSuccess = {Id:Guid}
  
type ReceiveResult = PipelineOutput<byte[]>
type DecodeResult = PipelineOutput<string>
type GetConsumerResult = PipelineOutput<UntypedConsumer * string>
type DeserializeResult = PipelineOutput<UntypedConsumer * obj>
type ExecuteResult = PipelineOutput<UntypedConsumer * obj>
type SerializeResult = PipelineOutput<string>
type EncodeResult = PipelineOutput<byte[]>
type SendResult = PipelineOutput<unit>

type Filter = 
    | ReceiveFilter of (ReceiveResult -> ReceiveResult)
    | DecodeFilter of (DecodeResult -> DecodeResult)
    | GetConsumerFilter of (GetConsumerResult -> GetConsumerResult)
    | DeserializeFilter of (DeserializeResult -> DeserializeResult)
    | ExecuteFilter of (ExecuteResult -> ExecuteResult)
    | SerializeFilter of (SerializeResult -> SerializeResult)
    | EncodeFilter of (EncodeResult -> EncodeResult)
    | SendFilter of (SendResult -> SendResult)

type ServerConfig = {
    Serializer : Serializer
    Consumers : UntypedConsumer list 
    Port : Port
    Filers : Filter list
}    


