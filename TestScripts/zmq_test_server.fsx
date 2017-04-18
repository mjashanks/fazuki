#r "../Fazuki.Server/bin/Debug/fszmq.dll"

open fszmq
open System.Text
open System.Threading

let context = new Context()
let socket = Context.rep context
Socket.bind socket "tcp://*:5555"
printfn "binding.."

while true do
    printfn "receiving..."
    Socket.recv socket 
    |> Encoding.UTF8.GetString
    |> printfn "Received: %s"
    Thread.Sleep(500)
    Socket.send socket <| Encoding.UTF8.GetBytes("Thanks!")
    