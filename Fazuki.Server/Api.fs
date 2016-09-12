namespace Fazuki.Server

open fszmq
open System.Text
open Fazuki.Server.Config
open System

module Server = // the server pipeline
    
    let Start (config:ServerConfig)  =              
        let Instance = ConfiguredServerInstance(config)
  
        // this is the main server pipeline
        while config.IsRunning() do
            Instance.Receive ()
            |> Encoding.UTF8.GetString
            |> Instance.GetMessageType
            |> Instance.Deserialize 
            |> Instance.Execute 
            |> Instance.Serialize 
            |> Encoding.UTF8.GetBytes
            |> Instance.Send
            |> ignore

 

        