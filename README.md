# Managed Sockets
## Why?

I decided to write this after finding that existing sockets implementations did not meet my requirements.

My requirements:
 - Asyncrounous with no unnecessary threads
 - Event driven
 - Thread-safe
 - SIMPLE
 - Fast

Projects like DotNetty and ReactiveSockets are way too complicated, especially for smaller projects.
Looking at the super old Winsock Orcas project (and a c# ported version of it), it was almost what I was looking for.. but still had some unnecessary complications.

So here you have it, Managed Sockets.

Due to my usages only being TCP at the moment, that is all this supports. If I need it in the future then I may add it then.

# Usage
## Server
Use a `TcpListener`, set on a `BeginAcceptTcpClient` callback.
When a new client comes in from the `TcpListener`, create a new socket like so:

```c#
var socket = new ClientSocket();
socket.DataReceived += SocketOnDataReceived;
socket.Disconnected += SocketOnDisconnected;
socket.Accept([TcpClient]);
```

From here on out, Managed Sockets has your back. If data is received, your event will be called.
If the client disconnects for whatever reason, your event will be called. (and if it was because of an error, you even get a reason!)

## Client
Check out the included project called Viewer.

It's a simple winforms gui, currently setup to just show the output of whatever data gets received.
It's setup for staying connected to IRC servers, so go ahead and throw an IRC Ip in there and try it out.
It should serve as a super basic example of usage for client applications.

# Licence
MIT