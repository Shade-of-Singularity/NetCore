using System;

namespace NetCore.Transports
{
    /// <summary>
    /// Transport for sending messages unreliably.
    /// </summary>
    /// TODO: Server should support implementing other transportation modes, like "Notify" from Riptide.
    public interface IUnreliableTransport : ITransport
    {
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to all connections this <see cref="ITransport"/> manages.
        /// </summary>
        /// <param name="header">Header of the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        public void SendUnreliable(Header header, ReadOnlySpan<byte> datagram);

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to all connections this <see cref="ITransport"/> manages, excluding <paramref name="toExclude"/>.
        /// </summary>
        /// <param name="header">Header of the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="toExclude">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendUnreliableExcluding(Header header, ReadOnlySpan<byte> datagram, ConnectionID toExclude);

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a target connection.
        /// </summary>
        /// <param name="header">Header of the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="target">Connection to send a <paramref name="datagram"/> to. Nothing should be sent if transport doesn't manage this connection.</param>
        public void SendUnreliableTo(Header header, ReadOnlySpan<byte> datagram, ConnectionID target);

        /// <summary>
        /// Handles raw <paramref name="datagram"/> of a unreliable message.
        /// </summary>
        /// <param name="header">Header of the message.</param>
        /// <param name="datagram">Datagram from a remote connection.</param>
        /// <param name="source">Connection ID from which <paramref name="datagram"/> has arrived.</param>
        public void HandleUnreliable(Header header, ReadOnlySpan<byte> datagram, ConnectionID source);
    }
}
