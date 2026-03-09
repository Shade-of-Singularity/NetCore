using System;

namespace NetCore
{
    /// <summary>
    /// Transport for sending messages reliably.
    /// </summary>
    /// TODO: Server should support implementing other transportation modes, like "Notify" from Riptide.
    public interface IReliableTransport : ITransport
    {
        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a target connection.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to send the data to. Nothing should be sent if transport doesn't manage this connection.</param>
        public void SendReliable(ReadOnlySpan<byte> datagram, int connection);
    }
}
