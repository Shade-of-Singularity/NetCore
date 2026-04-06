namespace NetCore.Messaging
{
    /// <summary>
    /// Defines methods for receiving the datagrams, sent resiliently.
    /// </summary>
    /// <seealso cref="SendingMode.Resilient"/>
    public interface IHandleResilient
    {
        /// <summary>
        /// Handles raw <paramref name="datagram"/> of a resilient message.
        /// </summary>
        /// <param name="datagram">Datagram from a remote connection.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="source">Connection ID from which <paramref name="datagram"/> has arrived.</param>
        public void HandleResilient(scoped ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source);
    }
}
