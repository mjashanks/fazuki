namespace Fazuki.NewtonsoftJson

open Fazuki.Common
open Newtonsoft.Json
open System
open System.Text

module Serialization = 

    let Serialize typ ob =
        JsonConvert.SerializeObject ob
        |> Encoding.UTF8.GetBytes

    let Deserialize (typ:Type) (byt:byte[]) = 
        let decoded = Encoding.UTF8.GetString byt
        JsonConvert.DeserializeObject(decoded, typ)

    let NewtonsoftSerializer = 
        {Serialize = Serialize;
        Deserialize = Deserialize }
