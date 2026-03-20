using System;

namespace NetCore.Transports
{
    /// <summary>
    /// Transport type for (Ordered, Reliable) message transporting.
    /// </summary>
    public interface IResilientTransport : ITransport
    {
        /// <summary>
        /// Resiliently sends <paramref name="datagram"/> to all connections this <see cref="ITransport"/> manages.
        /// </summary>
        /// <param name="header">Header of the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        public void SendResilient(Header header, ReadOnlySpan<byte> datagram);
        /// <summary>
        /// Resiliently sends <paramref name="datagram"/> to all connections this <see cref="ITransport"/> manages, excluding <paramref name="toExclude"/>.
        /// </summary>
        /// <param name="header">Header of the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="toExclude">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendResilientExcluding(Header header, ReadOnlySpan<byte> datagram, ConnectionID toExclude);
        /// <summary>
        /// Resiliently sends <paramref name="datagram"/> to a target connection.
        /// </summary>
        /// <param name="header">Header of the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="target">Connection to send a <paramref name="datagram"/> to. Nothing should be sent if transport doesn't manage this connection.</param>
        public void SendResilientTo(Header header, ReadOnlySpan<byte> datagram, ConnectionID target);
        /// <summary>
        /// Resiliently sends <paramref name="datagram"/> to a target connection, outside of a current host.
        /// </summary>
        /// <param name="header">Header of the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="args">Temporary connection args used for this connection in particular.</param>
        public void SendResilientTo(Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args);
        /// <summary>
        /// Handles raw <paramref name="datagram"/> of a reliable message.
        /// </summary>
        /// <param name="header">Header of the message.</param>
        /// <param name="datagram">Datagram from a remote connection.</param>
        /// <param name="source">Connection ID from which <paramref name="datagram"/> has arrived.</param>
        public void HandleResilient(Header header, ReadOnlySpan<byte> datagram, ConnectionID source);
    }
}
