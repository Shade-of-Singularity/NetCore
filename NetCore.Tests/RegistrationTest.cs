using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace NetCore.Tests
{
    [TestClass]
    public sealed class BasicUsageTest
    {
        [TestMethod]
        public void RegisterTransports()
        {
            Client client = new();
            client.RegisterUnreliableTransport(new UDP.UDPTransport());
            client.RegisterReliableTransport(new TCP.TCPTransport());
            client.RegisterTransportAsBoth(new Pipes.PipeTransport());

            Server server = new();
            server.RegisterUnreliableTransport(new UDP.UDPTransport());
            server.RegisterReliableTransport(new TCP.TCPTransport());
            server.RegisterTransportAsBoth(new Pipes.PipeTransport());
        }

        [TestMethod]
        public void UtilizeTransports()
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

            if (!client.Connect(new IPEndPoint(IPAddress.Loopback, 25000)))
            {
                throw new Exception("Failed to connect.");
            }

            server.SendReliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));
            server.SendReliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));
            server.SendUnreliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));
            server.SendUnreliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));

            client.SendReliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));
            client.SendReliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));
            client.SendUnreliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));
            client.SendUnreliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));

            client.Disconnect();
            client.Stop();
            server.Stop();
        }
    }
}
