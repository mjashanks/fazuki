namespace Fazuki.Common

open System
open System.Collections.Generic

type Serializer ={
    Serialize : Type -> obj -> byte[]
    Deserialize : Type -> byte[] -> obj
}

type RequestResponse = { Req:Type; Rep:Type }

type MessageTypes = Dictionary<string, RequestResponse>

type MessageStream = IObservable<string> 

type RunState = { IsRunning : bool }

type Port = int Option

type Url = string Option
        
type RouteAttribute(path) =
    inherit System.Attribute() 
    member a.Path = path

module Helpers = 
    let getTypeRoute (typ:Type) = 
        typ.Name
    let getType ob = 
        ob.GetType()
    let getObjRoute<'t> () =
        getTypeRoute typeof<'t>
