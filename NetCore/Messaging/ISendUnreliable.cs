using NetCore.Transports;

namespace NetCore.Messaging
{
    /// <summary>
    /// Declares methods for sending datagrams unreliably.
    /// </summary>
    /// <seealso cref="SendingMode.Unreliable"/>
    public interface ISendUnreliable
    {
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to all connections this <see cref="ITransport"/> manages.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public void SendUnreliable(scoped ReadOnlySpan<byte> datagram, in Header header, in Flags flags);
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to all connections this <see cref="ITransport"/> manages, excluding <paramref name="toExclude"/>.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="toExclude">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendUnreliableExcluding(scoped ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude);
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a <paramref name="target"/> connection.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="target">Connection to send a <paramref name="datagram"/> to. Nothing should be sent if transport doesn't manage this connection.</param>
        public void SendUnreliableTo(scoped ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target);
    }
}
