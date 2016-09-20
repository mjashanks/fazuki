namespace Fazuki.Server
open System

type LogLevel = Debug | Info | Warning | Error | Fatal

type FilterLog = {
    RequestId : Guid
    LogMessage : string
    Level : LogLevel
    Date : DateTime
}

type Logger = LogLevel -> string -> unit

module Logging = 

    let GetLoggingFilters logger = 
    
        let LogDebug message = logger Debug message    
        let LogError message = logger Error message    

        let ReceiveLogger (result:ReceiveResult) = 
            match result with
            | Success(r) -> LogDebug (sprintf "Received Success: %i" r.EncodedRequest.Length)
            | Failed(e) -> 
                match e with
                | ReceiveError(e) -> LogError (e.ToString())
                | _ -> ()
            result

        let DecodeLogger (result:DecodeResult) = 
            match result with
            | Success(r) -> LogDebug (sprintf "Decode Success: %s" r.DecodedRequest)
            | Failed(e) -> 
                match e with
                | DecodeError(e) -> LogError (e.ToString())
                | _ -> ()
            result

        [ReceiveFilter ReceiveLogger;
        DecodeFilter DecodeLogger]
            

