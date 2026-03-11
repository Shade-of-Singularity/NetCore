using NetCore.Loopback;
using NetCore.UDP;
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace NetCore.TCP
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
                    if (IsInitialized || value?.IsInitialized == true)
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
        /// Lock, used when interacting with everything - <see cref="Socket"/>, <see cref="Source"/>, <see cref="Clients"/>, etc.
        /// </summary>
        protected readonly object _lock = new();
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
        public void SendReliable(ReadOnlySpan<byte> datagram)
        {
#if DEBUG
            Console.WriteLine($"{nameof(TCPTransport)}.{nameof(SendReliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
        }

        /// <inheritdoc/>
        public void SendReliableExcluding(ReadOnlySpan<byte> datagram, ConnectionID toExclude)
        {
#if DEBUG
            Console.WriteLine($"{nameof(TCPTransport)}.{nameof(SendReliableExcluding)}(exclude: ({toExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
        }

        /// <inheritdoc/>
        public void SendReliableTo(ReadOnlySpan<byte> datagram, ConnectionID target)
        {
#if DEBUG
            Console.WriteLine($"{nameof(TCPTransport)}.{nameof(SendReliableTo)}(target: ({target}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
        }

        /// <inheritdoc/>
        public void HandleReliable(ReadOnlySpan<byte> datagram, ConnectionID source)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(TCPTransport)}.{nameof(HandleReliable)}(source: ({source}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
