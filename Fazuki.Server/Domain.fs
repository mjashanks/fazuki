namespace Fazuki.Server

open System
open System.Collections.Generic
open Fazuki.Common

type MessageStream = IObservable<string> 

type StopServer = unit -> unit

type RunState = { IsRunning : bool }

type MessageHandler = obj -> obj
        
type ServerConfig = {
    Serializer : Serializer
    Execute : MessageHandler
    MessageTypes : MessageTypes
    Port : Port
    IsRunning : unit -> bool
}    

type RouteAttribute(path) =
    inherit System.Attribute() 
    member a.Path = path

