using System;

namespace NetCore.Transports
{
    /// <summary>
    /// Transport for sending messages unreliably.
    /// </summary>
    /// TODO: Server should support implementing other transportation modes, like "Notify" from Riptide.
    public interface INotifyTransport : ITransport
    {
        /// <summary>
        /// Notifies all connections using <paramref name="datagram"/> this <see cref="ITransport"/> manages.
        /// </summary>
        /// <param name="header">Header of the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        public void SendNotify(Header header, ReadOnlySpan<byte> datagram);
        /// <summary>
        /// Notifies all connections using <paramref name="datagram"/> to this <see cref="ITransport"/> manages, excluding <paramref name="toExclude"/>.
        /// </summary>
        /// <param name="header">Header of the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="toExclude">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendNotifyExcluding(Header header, ReadOnlySpan<byte> datagram, ConnectionID toExclude);
        /// <summary>
        /// Notifies target connection using <paramref name="datagram"/>.
        /// </summary>
        /// <param name="header">Header of the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="target">Connection to send a <paramref name="datagram"/> to. Nothing should be sent if transport doesn't manage this connection.</param>
        public void SendNotifyTo(Header header, ReadOnlySpan<byte> datagram, ConnectionID target);
        /// <summary>
        /// Notifies a connection outside of a current remote host using <paramref name="datagram"/>.
        /// </summary>
        /// <param name="header">Header of the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="args">Temporary connection args used for this connection in particular.</param>
        public void SendNotifyTo(Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args);
        /// <summary>
        /// Handles raw <paramref name="datagram"/> of a unreliable message.
        /// </summary>
        /// <param name="header">Header of the message.</param>
        /// <param name="datagram">Datagram from a remote connection.</param>
        /// <param name="source">Connection ID from which <paramref name="datagram"/> has arrived.</param>
        public void HandleNotify(Header header, ReadOnlySpan<byte> datagram, ConnectionID source);
    }
}
