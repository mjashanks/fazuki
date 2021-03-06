﻿namespace Fazuki.Server

open fszmq
open System.Text
open Fazuki.Server
open System

module Server = // the server pipeline
    
    let Start (config:ServerConfig)  =              
        let Instance = ConfiguredServerInstance(config)
  
        // this is the main server pipeline
        while Instance.IsRunning() do
            Instance.Receive ()
            |> Instance.GetHandler
            |> Instance.Deserialize 
            |> Instance.Execute 
            |> Instance.Serialize 
            |> Instance.Send
            |> ignore

 

        