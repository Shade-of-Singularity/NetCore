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

            //server.SendReliable(new string('a', ushort.MaxValue).AsByteSpan());
            //server.SendReliable(new string('a', ushort.MaxValue).AsByteSpan());
            //server.SendUnreliable(new string('a', ushort.MaxValue).AsByteSpan());
            //server.SendUnreliable(new string('a', ushort.MaxValue).AsByteSpan());

            client.SendReliable(new string('a', ushort.MaxValue).AsByteSpan());
            client.SendReliable(new string('a', ushort.MaxValue).AsByteSpan());
            client.SendUnreliable(new string('a', ushort.MaxValue).AsByteSpan());
            client.SendUnreliable(new string('a', ushort.MaxValue).AsByteSpan());

            await client.Disconnect();
            await client.Stop();
            await server.Stop();
        }
    }
}
