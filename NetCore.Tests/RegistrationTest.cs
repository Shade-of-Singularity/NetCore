using NetCore.Transports.TCP;
using NetCore.Transports.UDP;
using NetCore.Transports.Unix;
using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NetCore.Tests
{
    [TestClass]
    public sealed class BasicUsageTest
    {
        [TestMethod]
        public void RegisterTransports()
        {
            Client client = new();
            client.RegisterUnreliableTransport(new UDPTransport());
            client.RegisterReliableTransport(new TCPTransport());
            client.RegisterTransportAsBoth(new UnixTransport());

            Server server = new();
            server.RegisterUnreliableTransport(new UDPTransport());
            server.RegisterReliableTransport(new TCPTransport());
            server.RegisterTransportAsBoth(new UnixTransport());
        }

        [TestMethod]
        public async Task UtilizeTransports()
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

            if (await client.Connect(IPAddress.Loopback, 25000) == OperationResult.Success)
            {
                throw new Exception("Fail to connect.");
            }

            server.SendReliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));
            server.SendReliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));
            server.SendUnreliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));
            server.SendUnreliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));

            client.SendReliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));
            client.SendReliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));
            client.SendUnreliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));
            client.SendUnreliable(MemoryMarshal.AsBytes(new string('a', ushort.MaxValue).AsSpan()));

            await client.Disconnect();
            await client.Stop();
            await server.Stop();
        }
    }
}
