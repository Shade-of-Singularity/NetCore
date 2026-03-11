using NetCore.Common;
using NetCore.Loopback;
using NetCore.TCP;
using NetCore.UDP;
using System;
using System.Net;
using System.Runtime.InteropServices;

namespace NetCore.Examples
{
    internal class Program
    {
        static void Main()
        {
            SendMessagesFromTesting();
        }

        static void RegisterTransports()
        {
            Client client = new();
            client.RegisterUnreliableTransport(new UDP.UDPTransport());
            client.RegisterReliableTransport(new TCP.TCPTransport());
            client.RegisterTransportAsBoth(new Pipes.PipeTransport());

            Server server = new();
            server.RegisterUnreliableTransport(new UDP.UDPTransport());
            server.RegisterReliableTransport(new TCP.TCPTransport());
            server.RegisterTransportAsBoth(new Pipes.PipeTransport());

            client.SendReliable(MemoryMarshal.AsBytes("data #1 (from client)".AsSpan()));
            server.SendReliable(MemoryMarshal.AsBytes("data #1 (from server)".AsSpan()));
            Console.WriteLine();
            client.SendUnreliable(MemoryMarshal.AsBytes("data #2 (from client)".AsSpan()));
            server.SendUnreliable(MemoryMarshal.AsBytes("data #2 (from server)".AsSpan()));
            Console.WriteLine();
            server.SendUnreliableTo(MemoryMarshal.AsBytes("data #3 (from server)".AsSpan()), (ConnectionID)32);
            server.SendReliableTo(MemoryMarshal.AsBytes("data #3 (from server)".AsSpan()), (ConnectionID)32);
            server.SendReliableExcluding(MemoryMarshal.AsBytes("data #3 (from server)".AsSpan()), (ConnectionID)32);
            server.SendUnreliableExcluding(MemoryMarshal.AsBytes("data #3 (from server)".AsSpan()), (ConnectionID)32);
        }

        static void SendMessages()
        {
            Client client = new();
            Server server = new();

            const int ServerPort = 25000;
            const int ClientPort = 25001;
            server.Start(ServerPort);
            client.Start(new IPEndPoint(IPAddress.Loopback, ClientPort));
            client.Connect(new IPEndPoint(IPAddress.Loopback, ServerPort));

            client.SendReliable(MemoryMarshal.AsBytes("client (Reliable) message".AsSpan()));
            server.SendReliable(MemoryMarshal.AsBytes("server (Reliable) message".AsSpan()));
            client.SendUnreliable(MemoryMarshal.AsBytes("client (Unreliable) message".AsSpan()));
            server.SendUnreliable(MemoryMarshal.AsBytes("server (Unreliable) message".AsSpan()));
        }

        static void SendMessagesFromTesting()
        {
            Client client = new();
            client.RegisterUnreliableTransport(new UDP.UDPTransport());
            client.RegisterReliableTransport(new TCP.TCPTransport());
            client.RegisterTransportAsBoth(new Pipes.PipeTransport());
            client.Start(new IPEndPoint(IPAddress.Loopback, 25001));

            Server server = new();
            server.RegisterUnreliableTransport(new UDP.UDPTransport());
            server.RegisterReliableTransport(new TCP.TCPTransport());
            server.RegisterTransportAsBoth(new Pipes.PipeTransport());
            server.Start(25000);

            client.Connect(new IPEndPoint(IPAddress.Loopback, 25000));

            server.SendReliable(MemoryMarshal.AsBytes(new string('a', 16).AsSpan()));
            server.SendReliable<TCP.TCPTransport>(MemoryMarshal.AsBytes(new string('a', 16).AsSpan()));
            server.SendUnreliable(MemoryMarshal.AsBytes(new string('a', 16).AsSpan()));
            server.SendUnreliable<UDP.UDPTransport>(MemoryMarshal.AsBytes(new string('a', 16).AsSpan()));

            client.SendReliable(MemoryMarshal.AsBytes(new string('a', 32).AsSpan()));
            client.SendReliable<Loopback.LoopbackTransport>(MemoryMarshal.AsBytes(new string('a', 32).AsSpan()));
            client.SendUnreliable(MemoryMarshal.AsBytes(new string('a', 32).AsSpan()));
            client.SendUnreliable<Loopback.LoopbackTransport>(MemoryMarshal.AsBytes(new string('a', 32).AsSpan()));

            client.Disconnect();
            client.Stop();
            server.Stop();
        }
    }
}
