using System;

namespace NetCore.Transports.Unix
{
    public sealed class UnixTransport : Transport, IReliableTransport, IUnreliableTransport
    {
        public override bool HasConnection(ConnectionID connection)
        {
            throw new NotImplementedException();
        }

        public void HandleReliable(in Header header, ReadOnlySpan<byte> datagram, ConnectionID source)
        {
            throw new NotImplementedException();
        }

        public void HandleUnreliable(in Header header, ReadOnlySpan<byte> datagram, ConnectionID source)
        {
            throw new NotImplementedException();
        }

        public void SendReliable(in Header header, ReadOnlySpan<byte> datagram)
        {
            throw new NotImplementedException();
        }

        public void SendReliableExcluding(in Header header, ReadOnlySpan<byte> datagram, ConnectionID toExclude)
        {
            throw new NotImplementedException();
        }

        public void SendReliableTo(in Header header, ReadOnlySpan<byte> datagram, ConnectionID target)
        {
            throw new NotImplementedException();
        }

        public void SendUnreliable(in Header header, ReadOnlySpan<byte> datagram)
        {
            throw new NotImplementedException();
        }

        public void SendUnreliableExcluding(in Header header, ReadOnlySpan<byte> datagram, ConnectionID toExclude)
        {
            throw new NotImplementedException();
        }

        public void SendUnreliableTo(in Header header, ReadOnlySpan<byte> datagram, ConnectionID target)
        {
            throw new NotImplementedException();
        }

        public void SendReliableTo(in Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }

        public void HandleReliable(in Header header, ReadOnlySpan<byte> datagram, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }

        public void SendUnreliableTo(in Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }

        public void HandleUnreliable(in Header header, ReadOnlySpan<byte> datagram, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }
    }
}
