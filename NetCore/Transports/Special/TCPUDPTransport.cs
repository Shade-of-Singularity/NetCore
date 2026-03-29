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
