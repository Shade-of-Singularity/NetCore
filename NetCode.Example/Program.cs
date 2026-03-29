using Cysharp.Threading.Tasks;
using NetCore.Transports.Loopback;
using NetCore.Transports.TCP;
using NetCore.Transports.UDP;
using NetCore.Transports.Unix;
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
            UDPSending().Forget();

            Console.WriteLine("Press any key to stop UDP server...");
            Console.ReadKey(); // This block until user hits any key.
        }

        static async UniTaskVoid UDPSending()
        {
            Server server = new();
            server.RegisterUnreliableTransport(new UDPTransport());
            Client client = new();
            client.RegisterUnreliableTransport(new UDPTransport());

            const ushort ServerPort = 27001;
            await server.Start(IPAddress.Loopback, ServerPort);
            await Task.Delay(10);

            await client.Start(IPAddress.Any, 0);
            await Task.Delay(10);
            await client.Connect(IPAddress.Loopback, ServerPort);
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
            client.SendUnreliable("test".AsByteSpan());
        }

        static void RegisterTransports()
        {
            Client client = new();
            client.RegisterUnreliableTransport(new UDPTransport());
            client.RegisterReliableTransport(new TCPTransport());
            client.RegisterTransportAsBoth(new UnixTransport());

            Server server = new();
            server.RegisterUnreliableTransport(new UDPTransport());
            server.RegisterReliableTransport(new TCPTransport());
            server.RegisterTransportAsBoth(new UnixTransport());

            client.SendReliable("data #1 (from client)".AsByteSpan());
            //server.SendReliable("data #1 (from server)".AsByteSpan());
            Console.WriteLine();
            client.SendUnreliable("data #2 (from client)".AsByteSpan());
            //server.SendUnreliable("data #2 (from server)".AsByteSpan());
            Console.WriteLine();
            //server.SendUnreliableTo("data #3 (from server)".AsByteSpan(), (ConnectionID)32);
            //server.SendReliableTo("data #3 (from server)".AsByteSpan(), (ConnectionID)32);
            //server.SendReliableExcluding("data #3 (from server)".AsByteSpan(), (ConnectionID)32);
            //server.SendUnreliableExcluding("data #3 (from server)".AsByteSpan(), (ConnectionID)32);
        }

        static async UniTaskVoid SendMessages()
        {
            Client client = new();
            Server server = new();

            const int ServerPort = 25000;
            const int ClientPort = 25001;
            await server.Start(ServerPort);
            await client.Start(IPAddress.Loopback, ClientPort);
            await client.Connect(IPAddress.Loopback, ServerPort);

            client.SendReliable("client (OrderedReliable) message".AsByteSpan());
            //server.SendReliable("server (OrderedReliable) message".AsByteSpan());
            client.SendUnreliable("client (Unreliable) message".AsByteSpan());
            //server.SendUnreliable("server (Unreliable) message".AsByteSpan());
        }

        static async UniTaskVoid SendMessagesFromTesting()
        {
            Client client = new();
            client.RegisterUnreliableTransport(new UDPTransport());
            client.RegisterReliableTransport(new TCPTransport());
            client.RegisterTransportAsBoth(new UnixTransport());
            await client.Start(IPAddress.Loopback, 25001);

            Server server = new();
            server.RegisterUnreliableTransport(new UDPTransport());
            server.RegisterReliableTransport(new TCPTransport());
            server.RegisterTransportAsBoth(new UnixTransport());
            await server.Start(25000);

            await client.Connect(IPAddress.Loopback, 25000);

            //server.SendReliable(new string('a', 16).AsByteSpan());
            //server.SendReliable<TCPTransport>(new string('a', 16).AsByteSpan());
            //server.SendUnreliable(new string('a', 16).AsByteSpan());
            //server.SendUnreliable<UDPTransport>(new string('a', 16).AsByteSpan());

            client.SendReliable(new string('a', 32).AsByteSpan());
            client.SendReliable<LoopbackTransport>(new string('a', 32).AsByteSpan());
            client.SendUnreliable(new string('a', 32).AsByteSpan());
            client.SendUnreliable<LoopbackTransport>(new string('a', 32).AsByteSpan());

            await client.Disconnect();
            await client.Stop();
            await server.Stop();
        }
    }
}
