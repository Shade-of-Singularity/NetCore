using NetCore.Transports.UDP;
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace NetCore.Transports.TCP
{
    /// <summary>
    /// Transport for TCP messages.
    /// </summary>
    /// <remarks>
    /// Does not implement networking functionality at the moment.
    /// </remarks>
    public class TCPTransport : Transport, IReliableTransport
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Public Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// <see cref="TCPTransport"/> pair this <see cref="UDP.UDPTransport"/> with.
        /// </summary>
        public virtual UDPTransport? UDPTransport
        {
            get => m_UDPTransport;
            set
            {
                lock (_lock)
                {
                    if (IsAttached || value?.IsAttached == true)
                    {
                        throw new Exception($"Cannot pair UDP with TCP after any of the transports were initialized/attached to a {nameof(NetworkMember)}.");
                    }

                    m_UDPTransport = value;
                }
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// <see cref="UDP.UDPTransport"/> to pair this <see cref="TCPTransport"/> with.
        /// </summary>
        private UDPTransport? m_UDPTransport;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc/>
        public override bool HasConnection(ConnectionID connection)
        {
            return true;
        }

        /// <inheritdoc/>
        public void SendReliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags)
        {
#if DEBUG
            Console.WriteLine($"{nameof(TCPTransport)}.{nameof(SendReliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
        }

        /// <inheritdoc/>
        public void SendReliableExcluding(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude)
        {
#if DEBUG
            Console.WriteLine($"{nameof(TCPTransport)}.{nameof(SendReliableExcluding)}(exclude: ({toExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
        }

        /// <inheritdoc/>
        public void SendReliableTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target)
        {
#if DEBUG
            Console.WriteLine($"{nameof(TCPTransport)}.{nameof(SendReliableTo)}(target: ({target}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
        }

        /// <inheritdoc/>
        public void HandleReliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(TCPTransport)}.{nameof(HandleReliable)}(source: ({source}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
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
    }
}
