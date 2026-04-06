namespace NetCore.Messaging
{
    /// <summary>
    /// Defines methods for receiving the datagrams, sent sequentially.
    /// </summary>
    /// <seealso cref="SendingMode.Sequential"/>
    public interface IHandleSequential
    {
        /// <summary>
        /// Handles raw <paramref name="datagram"/> of a sequential message.
        /// </summary>
        /// <param name="datagram">Datagram from a remote connection.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="source">Connection ID from which <paramref name="datagram"/> has arrived.</param>
        public void HandleSequential(scoped ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source);
    }
}
