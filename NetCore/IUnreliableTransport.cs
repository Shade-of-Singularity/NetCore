using System;

namespace NetCore
{
    /// <summary>
    /// Transport for sending messages unreliably.
    /// </summary>
    /// TODO: Server should support implementing other transportation modes, like "Notify" from Riptide.
    public interface IUnreliableTransport : ITransport
    {
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a target connection.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to send the data to. Nothing should be sent if transport doesn't manage this connection.</param>
        public void SendUnreliable(ReadOnlySpan<byte> datagram, int connection);
    }
}
