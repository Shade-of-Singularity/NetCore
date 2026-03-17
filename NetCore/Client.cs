using NetCore.Transports;
using System;

namespace NetCore
{
    /// <summary>
    /// Base class working with different <see cref="ITransport"/>s.
    /// </summary>
    public class Client : NetworkMember
    {
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to the server.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendUnreliable(Header header, ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                foreach (var transport in UnreliableTransports)
                {
                    transport.SendUnreliable(header, datagram);
                }
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendUnreliable<TTransport>(Header header, ReadOnlySpan<byte> datagram) where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                GetUnreliableTransport<TTransport>()?.SendUnreliable(header, datagram);
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendReliable(Header header, ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                foreach (var transport in ReliableTransports)
                {
                    transport.SendReliable(header, datagram);
                }
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendReliable<TTransport>(Header header, ReadOnlySpan<byte> datagram) where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                GetReliableTransport<TTransport>()?.SendReliable(header, datagram);
            }
        }
    }
}
