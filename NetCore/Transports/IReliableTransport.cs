using System;

namespace NetCore.Transports
{
    /// <summary>
    /// (Reliable, Unordered) Transport for sending messages.
    /// </summary>
    public interface IReliableTransport : ITransport
    {
        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to all connections this <see cref="ITransport"/> manages.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public void SendReliable(ReadOnlySpan<byte> datagram, in Header header, in Flags flags);
        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to all connections this <see cref="ITransport"/> manages, excluding <paramref name="toExclude"/>.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="toExclude">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendReliableExcluding(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude);
        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a <paramref name="target"/> connection.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="target">Connection to send a <paramref name="datagram"/> to. Nothing should be sent if transport doesn't manage this connection.</param>
        public void SendReliableTo(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target);
        /// <summary>
        /// Handles raw <paramref name="datagram"/> of a reliable message.
        /// </summary>
        /// <param name="datagram">Datagram from a remote connection.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="source">Connection ID from which <paramref name="datagram"/> has arrived.</param>
        public void HandleReliable(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source);



        /// <summary>
        /// Handles raw <paramref name="datagram"/> of a reliable message from a source, specified in <paramref name="args"/>.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">Temporary connection args used for this connection in particular.</param>
        public void SendReliableTo(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args);
        /// <summary>
        /// Handles raw <paramref name="datagram"/> of a reliable message.
        /// </summary>
        /// <param name="datagram">Datagram from a remote connection.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="source">Temporary connection args from which <paramref name="datagram"/> has arrived.</param>
        public void HandleReliable(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs source);
    }
}
