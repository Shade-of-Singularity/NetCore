using System;

namespace NetCore.Transports.Unix
{
    public sealed class UnixTransport : Transport, IReliableTransport, IUnreliableTransport
    {
        /// <inheritdoc/>
        public override bool HasConnection(ConnectionID connection)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void HandleReliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void HandleUnreliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendReliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendReliableExcluding(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendReliableTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendUnreliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendUnreliableExcluding(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendReliableTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void HandleReliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void HandleUnreliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }
    }
}
