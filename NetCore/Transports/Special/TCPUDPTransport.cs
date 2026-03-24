using NetCore.Transports.TCP;
using NetCore.Transports.UDP;
using System;

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
        public void HandleReliable(in Header header, ReadOnlySpan<byte> datagram, ConnectionID source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void HandleUnreliable(in Header header, ReadOnlySpan<byte> datagram, ConnectionID source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendReliable(in Header header, ReadOnlySpan<byte> datagram)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendReliableExcluding(in Header header, ReadOnlySpan<byte> datagram, ConnectionID toExclude)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendReliableTo(in Header header, ReadOnlySpan<byte> datagram, ConnectionID target)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendUnreliable(in Header header, ReadOnlySpan<byte> datagram)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendUnreliableExcluding(in Header header, ReadOnlySpan<byte> datagram, ConnectionID toExclude)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(in Header header, ReadOnlySpan<byte> datagram, ConnectionID target)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendReliableTo(in Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void HandleReliable(in Header header, ReadOnlySpan<byte> datagram, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(in Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void HandleUnreliable(in Header header, ReadOnlySpan<byte> datagram, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }
    }
}
