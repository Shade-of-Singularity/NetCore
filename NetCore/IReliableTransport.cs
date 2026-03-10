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
        /// Reliably sends <paramref name="datagram"/> to all connections this <see cref="ITransport"/> manages.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        public void SendReliable(ReadOnlySpan<byte> datagram);

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to all connections this <see cref="ITransport"/> manages, excluding <paramref name="CIDToExclude"/>.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="CIDToExclude">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendReliableExclusive(ReadOnlySpan<byte> datagram, ulong CIDToExclude);

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a target connection.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="targetCID">Connection to send a <paramref name="datagram"/> to. Nothing should be sent if transport doesn't manage this connection.</param>
        public void SendReliableTo(ReadOnlySpan<byte> datagram, ulong targetCID);

        /// <summary>
        /// Handles raw <paramref name="datagram"/> of a reliable message.
        /// </summary>
        /// <param name="datagram">Datagram from a remote connection.</param>
        /// <param name="sourceCID">Connection ID from which <paramref name="datagram"/> has arrived.</param>
        public void HandleReliable(ReadOnlySpan<byte> datagram, ulong sourceCID);
    }
}
