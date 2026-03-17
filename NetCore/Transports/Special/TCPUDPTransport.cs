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
