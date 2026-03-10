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
        public void SendUnreliable(ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                foreach (var transport in UnreliableTransports)
                {
                    transport.SendUnreliable(datagram);
                }
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        public void SendReliable(ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                foreach (var transport in ReliableTransports)
                {
                    transport.SendReliable(datagram);
                }
            }
        }
    }
}
