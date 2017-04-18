#r "../Fazuki.Client/bin/Debug/fszmq.dll"
#r "../Fazuki.Client/bin/Debug/Fazuki.Common.dll"
#r "../Fazuki.Client/bin/Debug/Fazuki.Client.dll"
#r "../Fazuki.Jil/bin/Debug/Fazuki.Jil.dll"
#r "../Fazuki.Jil/bin/Debug/Sigil.dll"
#r "../Fazuki.Jil/bin/Debug/Jil.dll"

open fszmq
open System.Text
open Fazuki.Client
open Fazuki.Jil
open System
open Fazuki.Common.Helpers
open Fazuki.Client
open Fazuki.Common
open System.Threading
open Fazuki.Client.Config

(*
type Instance() = 

    let context = new Context()
    let socket = 
        let sock = Context.req context
        Socket.connect sock "tcp://localhost:5555"
        printfn "connected ..."
        sock

    member c.Send (message:string) = 
        Socket.send socket (Encoding.UTF8.GetBytes message)
    
    member c.Receive () =
        Socket.recv socket |> Encoding.UTF8.GetString
*)

type Instance() =
    let context = new Context ()
    let requester =
        let sock = Context.req context
        Socket.connect sock "tcp://localhost:5555"
        sock
  //printfn "Connecting to hello world server..."
    member i.Serialize msg = 
        Serialization.Serialize (msg.GetType()) msg

    member i.Send msg =
        let strMsg = (Encoding.UTF8.GetString(msg))
        printfn "Sending Hello %s..." strMsg
        Socket.send requester msg

    member i.Receive msg =
        let _buffer = Socket.recv requester
        _buffer//"Received World %s" msg


let config =
    {Url = "localhost:5555";
     Serializer = Serialization.JilSerializer}

let instance = Instance () //ConfiguredClientInstance(config)

type GetCreatures = {
    CreatureType : string
}

type Creature = {
    Type : string
    Name : string
}

printfn "config: %s" config.Url

let request = {CreatureType="Dog"}

let cast (o:obj) = o :?> Creature list

//printfn "%s" (Encoding.UTF8.GetString(instance.Serialize request))

let msg = "{\"CreatureType\":\"Dog\"}"
let msg2 = "hello"
//Encoding.UTF8.GetBytes(msg2)

let doReq num = 
    try
        //Thread.Sleep 1000
        let serialized = request |> instance.Serialize
        //printf "%s" (Encoding.UTF8.GetString(serialized))
        let mess = "yeeeo"B

        //Encoding.UTF8.GetString(mess)


        mess
        //|> instance.AddHeader "get_creatures"
        |> instance.Send

        printfn "sent!"
        // RECEIVE REPLY
        instance.Receive ()
        |> Encoding.UTF8.GetString
        |> printfn "received %s" 
        //|> instance.Deserialize 
        //|>  cast
        //|> Seq.iter (fun c -> printfn "CreatureName = %s " c.Name)
    with | e -> 
            printfn "%i ... %s" num (e.ToString())
            ()

[1..10] 
|> Seq.iter doReq


(*
inst.Send (Encoding.UTF8.GetBytes("you rock"))

inst.Receive() 
|> Encoding.UTF8.GetString
|> printfn "response: %s"
*)
