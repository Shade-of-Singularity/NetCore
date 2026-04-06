using NetCore.Transports.TCP;
using NetCore.Transports.UDP;

namespace NetCore.Transports.Special
{
    /// <summary>
    /// Transport, using <see cref="TCPTransport"/> for reliable messages, and <see cref="UDPTransport"/> for unreliable.
    /// </summary>
    /// TODO: Implement manually, as a standalone transport.
    public sealed class TCPUDPTransport : Transport, IReliableTransport, IUnreliableTransport
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private readonly TCPTransport m_TCPTransport = new();
        private readonly UDPTransport m_UDPTransport = new();




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        public override bool HasConnection(ConnectionID connection)
        {
            // TCP connects first, and only it holds connections.
            return m_TCPTransport.HasConnection(connection);
        }

        /// <inheritdoc/>
        public void HandleReliable(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void HandleUnreliable(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendReliable(ReadOnlySpan<byte> datagram, in Header header, in Flags flags)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendReliableExcluding(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendReliableTo(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendUnreliable(ReadOnlySpan<byte> datagram, in Header header, in Flags flags)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendUnreliableExcluding(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendReliableTo(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void HandleReliable(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void HandleUnreliable(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }
    }
}
