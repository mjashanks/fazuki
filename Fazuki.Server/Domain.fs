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

type LogLevel = Debug | Info | Warning | Error | Fatal

type Logger = LogLevel -> string -> unit

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

  
type ReceiveResult = PipelineOutput<byte[]>
type DecodeResult = PipelineOutput<string>
type GetConsumerResult = PipelineOutput<UntypedConsumer * string>
type DeserializeResult = PipelineOutput<UntypedConsumer * obj>
type ExecuteResult = PipelineOutput<UntypedConsumer * obj>
type SerializeResult = PipelineOutput<string>
type EncodeResult = PipelineOutput<byte[]>
type SendResult = PipelineOutput<unit>

type ServerConfig = {
    Serializer : Serializer
    Consumers : UntypedConsumer list 
    Port : Port
    Loggers : Logger list
}    


