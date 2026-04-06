using NetCore.Transports;

namespace NetCore.Messaging
{
    /// <summary>
    /// Declares methods for sending datagrams sequentially.
    /// </summary>
    /// <seealso cref="SendingMode.Sequential"/>
    public interface ISendSequential
    {
        /// <summary>
        /// Sequentially sends <paramref name="datagram"/> to all connections this <see cref="ITransport"/> manages.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public void SendSequential(scoped ReadOnlySpan<byte> datagram, in Header header, in Flags flags);
        /// <summary>
        /// Sequentially sends <paramref name="datagram"/> to all connections this <see cref="ITransport"/> manages, excluding <paramref name="toExclude"/>.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="toExclude">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendSequentialExcluding(scoped ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude);
        /// <summary>
        /// Sequentially sends <paramref name="datagram"/> to a <paramref name="target"/> connection.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="target">Connection to send a <paramref name="datagram"/> to. Nothing should be sent if transport doesn't manage this connection.</param>
        public void SendSequentialTo(scoped ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target);
    }
}
