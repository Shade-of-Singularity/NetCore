using NetCore.Transports;
using System;

namespace NetCore
{
    /// <summary>
    /// Base class working with different <see cref="ITransport"/>s.
    /// </summary>
    /// <inheritdoc cref="NetworkMember"/>
    /// TODO: Consider adding a check for 0 transports being present.
    public class Client(int transports) : NetworkMember<Client>(transports)
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Default parameter-less .ctor.
        /// Pre-allocates some space for transports with <see cref="NetworkMember.DefaultInitialTransportCapacity"/>.
        /// </summary>
        public Client() : this(DefaultInitialTransportCapacity) { }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to the server.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendUnreliable(ref Header header, ReadOnlySpan<byte> datagram)
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    foreach (var transport in UnreliableTransports)
                    {
                        transport.SendUnreliable(header, datagram);
                    }
                }
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendUnreliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IUnreliableTransport
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    GetUnreliableTransport<TTransport>()?.SendUnreliable(header, datagram);
                }
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendReliable(ref Header header, ReadOnlySpan<byte> datagram)
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    foreach (var transport in ReliableTransports)
                    {
                        transport.SendReliable(header, datagram);
                    }
                }
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendReliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    GetReliableTransport<TTransport>()?.SendReliable(header, datagram);
                }
            }
        }
    }
}
