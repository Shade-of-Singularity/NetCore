using NetCore.Transports.Loopback;
using NetCore.Transports.Pipes;
using NetCore.Transports.TCP;
using NetCore.Transports.UDP;
using System;
using System.Net;
using System.Runtime.InteropServices;

namespace NetCore.Examples
{
    internal class Program
    {
        static void Main()
        {
            UDPTesting();

            Console.WriteLine("Press <enter> key to stop UDP server...");
            Console.ReadLine(); // This block until user hit <enter> key
        }

        static void UDPTesting()
        {
            Server server = new();
            server.RegisterUnreliableTransport(new UDPTransport());
            Client client = new();
            client.RegisterUnreliableTransport(new UDPTransport());
            server.Start(25000);
            client.Start(new IPEndPoint(IPAddress.Any, 0));
            client.Connect(new IPEndPoint(IPAddress.Loopback, 25000));
            client.SendUnreliable(default, MemoryMarshal.AsBytes("test".AsSpan()));
        }

        static void RegisterTransports()
        {
            Client client = new();
            client.RegisterUnreliableTransport(new UDPTransport());
            client.RegisterReliableTransport(new TCPTransport());
            client.RegisterTransportAsBoth(new PipeTransport());

            Server server = new();
            server.RegisterUnreliableTransport(new UDPTransport());
            server.RegisterReliableTransport(new TCPTransport());
            server.RegisterTransportAsBoth(new PipeTransport());

            client.SendReliable(default, MemoryMarshal.AsBytes("data #1 (from client)".AsSpan()));
            server.SendReliable(default, MemoryMarshal.AsBytes("data #1 (from server)".AsSpan()));
            Console.WriteLine();
            client.SendUnreliable(default, MemoryMarshal.AsBytes("data #2 (from client)".AsSpan()));
            server.SendUnreliable(default, MemoryMarshal.AsBytes("data #2 (from server)".AsSpan()));
            Console.WriteLine();
            server.SendUnreliableTo(default, MemoryMarshal.AsBytes("data #3 (from server)".AsSpan()), (ConnectionID)32);
            server.SendReliableTo(default, MemoryMarshal.AsBytes("data #3 (from server)".AsSpan()), (ConnectionID)32);
            server.SendReliableExcluding(default, MemoryMarshal.AsBytes("data #3 (from server)".AsSpan()), (ConnectionID)32);
            server.SendUnreliableExcluding(default, MemoryMarshal.AsBytes("data #3 (from server)".AsSpan()), (ConnectionID)32);
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

            client.SendReliable(default, MemoryMarshal.AsBytes("client (OrderedReliable) message".AsSpan()));
            server.SendReliable(default, MemoryMarshal.AsBytes("server (OrderedReliable) message".AsSpan()));
            client.SendUnreliable(default, MemoryMarshal.AsBytes("client (Unreliable) message".AsSpan()));
            server.SendUnreliable(default, MemoryMarshal.AsBytes("server (Unreliable) message".AsSpan()));
        }

        static void SendMessagesFromTesting()
        {
            Client client = new();
            client.RegisterUnreliableTransport(new UDPTransport());
            client.RegisterReliableTransport(new TCPTransport());
            client.RegisterTransportAsBoth(new PipeTransport());
            client.Start(new IPEndPoint(IPAddress.Loopback, 25001));

            Server server = new();
            server.RegisterUnreliableTransport(new UDPTransport());
            server.RegisterReliableTransport(new TCPTransport());
            server.RegisterTransportAsBoth(new PipeTransport());
            server.Start(25000);

            client.Connect(new IPEndPoint(IPAddress.Loopback, 25000));

            server.SendReliable(default, MemoryMarshal.AsBytes(new string('a', 16).AsSpan()));
            server.SendReliable<TCPTransport>(default, MemoryMarshal.AsBytes(new string('a', 16).AsSpan()));
            server.SendUnreliable(default, MemoryMarshal.AsBytes(new string('a', 16).AsSpan()));
            server.SendUnreliable<UDPTransport>(default, MemoryMarshal.AsBytes(new string('a', 16).AsSpan()));

            client.SendReliable(default, MemoryMarshal.AsBytes(new string('a', 32).AsSpan()));
            client.SendReliable<LoopbackTransport>(default, MemoryMarshal.AsBytes(new string('a', 32).AsSpan()));
            client.SendUnreliable(default, MemoryMarshal.AsBytes(new string('a', 32).AsSpan()));
            client.SendUnreliable<LoopbackTransport>(default, MemoryMarshal.AsBytes(new string('a', 32).AsSpan()));

            client.Disconnect();
            client.Stop();
            server.Stop();
        }
    }
}
