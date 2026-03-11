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
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendUnreliable(ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                foreach (var transport in UnreliableTransports)
                {
                    transport.SendUnreliable(datagram);
                }
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendUnreliable<TTransport>(ReadOnlySpan<byte> datagram) where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                GetUnreliableTransport<TTransport>()?.SendUnreliable(datagram);
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendReliable(ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                foreach (var transport in ReliableTransports)
                {
                    transport.SendReliable(datagram);
                }
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendReliable<TTransport>(ReadOnlySpan<byte> datagram) where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                GetReliableTransport<TTransport>()?.SendReliable(datagram);
            }
        }
    }
}
