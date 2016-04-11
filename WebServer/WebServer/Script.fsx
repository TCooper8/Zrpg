// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.

#load "Try.fs"
#load "IO.fs"
#load "WebServer.fs"
open Pario

// Define your library scripting code here

let server = WebServer.Server()
server.get <| WebServer.FileLoader.serveStatic "./" (Some "index.html")

server.listen "localhost" 8080 |> Async.RunSynchronously