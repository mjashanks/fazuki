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
    
        let LogDebug id message = logger Debug message    
        let LogError id message = logger Error message    

        let ReceiveLogger (result:ReceiveResult) = 
            match result.StepResult with
            | Success(r) -> LogDebug result.Id (sprintf "Received Success: %i" r.EncodedRequest.Length)
            | Failed(e) -> 
                match e with
                | ReceiveError(e) -> LogError result.Id (e.ToString())
                | _ -> ()
            result

        let DecodeLogger (result:DecodeResult) = 
            match result.StepResult with
            | Success(r) -> LogDebug result.Id (sprintf "Decode Success: %s" r.DecodedRequest)
            | Failed(e) -> 
                match e with
                | DecodeError(e) -> LogError result.Id (e.ToString())
                | _ -> ()
            result
        
        let StepLogger<'res> (result:PipelineOutput<'res>) 
                             (debugText:'res->string)
                             (errorText:ServerError->string) =      
            match result.StepResult with
            | Success(r) -> LogDebug result.Id <| debugText r
                                                             
        [ReceiveFilter ReceiveLogger;
        DecodeFilter DecodeLogger]
            

