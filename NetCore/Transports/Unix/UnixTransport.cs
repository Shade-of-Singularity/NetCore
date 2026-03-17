using System;

namespace NetCore.Transports.Unix
{
    public sealed class UnixTransport : Transport, IReliableTransport, IUnreliableTransport
    {
        public override bool HasConnection(ConnectionID connection)
        {
            throw new NotImplementedException();
        }

        public void HandleReliable(Header header, ReadOnlySpan<byte> datagram, ConnectionID source)
        {
            throw new NotImplementedException();
        }

        public void HandleUnreliable(Header header, ReadOnlySpan<byte> datagram, ConnectionID source)
        {
            throw new NotImplementedException();
        }

        public void SendReliable(Header header, ReadOnlySpan<byte> datagram)
        {
            throw new NotImplementedException();
        }

        public void SendReliableExcluding(Header header, ReadOnlySpan<byte> datagram, ConnectionID toExclude)
        {
            throw new NotImplementedException();
        }

        public void SendReliableTo(Header header, ReadOnlySpan<byte> datagram, ConnectionID target)
        {
            throw new NotImplementedException();
        }

        public void SendUnreliable(Header header, ReadOnlySpan<byte> datagram)
        {
            throw new NotImplementedException();
        }

        public void SendUnreliableExcluding(Header header, ReadOnlySpan<byte> datagram, ConnectionID toExclude)
        {
            throw new NotImplementedException();
        }

        public void SendUnreliableTo(Header header, ReadOnlySpan<byte> datagram, ConnectionID target)
        {
            throw new NotImplementedException();
        }
    }
}
