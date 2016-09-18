namespace Fazuki.Client

open System
open System.Collections.Generic
open Fazuki.Common

type RequestResponse = { Req:Type; Rep:Type }

type MessageTypes = Dictionary<string, RequestResponse>

type ClientConfig = {
    Url : Url;
    Serializer : Serializer;
} 

type RouteAttribute(path) =
    inherit System.Attribute() 
    member a.Path = path

