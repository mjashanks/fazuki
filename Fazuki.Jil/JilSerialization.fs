namespace Fazuki.Jil

open Fazuki.Common
open Fazuki.Server
open Jil
open System

module Serialization = 

    let Serialize typ ob =
        JSON.SerializeDynamic ob 

    let Deserialize (typ:Type) (str:string) = 
        JSON.Deserialize (str, typ, JSON.GetDefaultOptions())

    let JilSerializer = 
        {Serialize = Serialize;
        Deserialize = Deserialize }

    let UseJilSerialization (config:ServerConfig) = 
        {config with Serializer = JilSerializer}