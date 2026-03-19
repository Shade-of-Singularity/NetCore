using NetCore.Transports.Loopback;
using NetCore.Transports.Pipes;
using NetCore.Transports.TCP;
using NetCore.Transports.UDP;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NetCore.Examples
{
    internal class Program
    {
        static void Main()
        {
            UDPSending();

            Console.WriteLine("Press any key to stop UDP server...");
            Console.ReadKey(); // This block until user hits any key.
        }

        static async void UDPSending()
        {
            Server server = new();
            server.RegisterUnreliableTransport(new UDPTransport());
            Client client = new();
            client.RegisterUnreliableTransport(new UDPTransport());

            const ushort ServerPort = 27001;
            server.Start(IPAddress.Loopback, ServerPort);
            await Task.Delay(10);

            client.Start(IPAddress.Any, 0);
            await Task.Delay(10);
            client.Connect(IPAddress.Loopback, ServerPort);
            await Task.Delay(10);

            client.SendUnreliable(Encoding.UTF8.GetBytes("Test message."));
            await Task.Delay(150);
        }

        static void UDPTesting()
        {
            Server server = new();
            server.RegisterUnreliableTransport(new UDPTransport());
            Client client = new();
            client.RegisterUnreliableTransport(new UDPTransport());
            server.Start(25000);
            client.Start(IPAddress.Any, 0);
            client.Connect(IPAddress.Loopback, 25000);
            client.SendUnreliable(MemoryMarshal.AsBytes("test".AsSpan()));
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
            client.Start(IPAddress.Loopback, ClientPort);
            client.Connect(IPAddress.Loopback, ServerPort);

            client.SendReliable(MemoryMarshal.AsBytes("client (OrderedReliable) message".AsSpan()));
            server.SendReliable(MemoryMarshal.AsBytes("server (OrderedReliable) message".AsSpan()));
            client.SendUnreliable(MemoryMarshal.AsBytes("client (Unreliable) message".AsSpan()));
            server.SendUnreliable(MemoryMarshal.AsBytes("server (Unreliable) message".AsSpan()));
        }

        static void SendMessagesFromTesting()
        {
            Client client = new();
            client.RegisterUnreliableTransport(new UDPTransport());
            client.RegisterReliableTransport(new TCPTransport());
            client.RegisterTransportAsBoth(new PipeTransport());
            client.Start(IPAddress.Loopback, 25001);

            Server server = new();
            server.RegisterUnreliableTransport(new UDPTransport());
            server.RegisterReliableTransport(new TCPTransport());
            server.RegisterTransportAsBoth(new PipeTransport());
            server.Start(25000);

            client.Connect(IPAddress.Loopback, 25000);

            server.SendReliable(MemoryMarshal.AsBytes(new string('a', 16).AsSpan()));
            server.SendReliable<TCPTransport>(MemoryMarshal.AsBytes(new string('a', 16).AsSpan()));
            server.SendUnreliable(MemoryMarshal.AsBytes(new string('a', 16).AsSpan()));
            server.SendUnreliable<UDPTransport>(MemoryMarshal.AsBytes(new string('a', 16).AsSpan()));

            client.SendReliable(MemoryMarshal.AsBytes(new string('a', 32).AsSpan()));
            client.SendReliable<LoopbackTransport>(MemoryMarshal.AsBytes(new string('a', 32).AsSpan()));
            client.SendUnreliable(MemoryMarshal.AsBytes(new string('a', 32).AsSpan()));
            client.SendUnreliable<LoopbackTransport>(MemoryMarshal.AsBytes(new string('a', 32).AsSpan()));

            client.Disconnect();
            client.Stop();
            server.Stop();
        }
    }
}
