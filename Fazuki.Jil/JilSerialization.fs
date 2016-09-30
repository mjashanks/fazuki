namespace Fazuki.Jil

open Fazuki.Common
open Fazuki.Server
open Jil
open System
open System.Text

module Serialization = 

    let Serialize typ ob =
        JSON.SerializeDynamic ob 
        |> Encoding.UTF8.GetBytes

    let Deserialize (typ:Type) (byt:byte[]) = 
        let decoded = Encoding.UTF8.GetString byt
        JSON.Deserialize (decoded, typ, JSON.GetDefaultOptions())

    let JilSerializer = 
        {Serialize = Serialize;
        Deserialize = Deserialize }

    let UseJilSerialization (config:ServerConfig) = 
        {config with Serializer = JilSerializer}