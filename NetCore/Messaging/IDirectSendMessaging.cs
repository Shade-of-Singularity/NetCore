namespace NetCore.Messaging
{
    /// <summary>
    /// Declares datagram sending methods, for Reliable and Unreliable communication.
    /// </summary>
    [Obsolete("Not in use. We are considering implementing such methods as lightweight wrappers around built-in sending methods.")]
    public interface IDirectSendMessaging
    {
        /// <summary>
        /// Sends a <paramref name="datagram"/> to a remote host, specified in <paramref name="args"/>.
        /// </summary>
        /// <param name="datagram"></param>
        /// <param name="header"></param>
        /// <param name="flags"></param>
        /// <param name="args"></param>
        /// <param name="reliable">Whether to re-send messages </param>
        void SendTo(scoped ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args, bool reliable = true);
        /// <summary>
        /// Handles <paramref name="datagram"/>, received from a remote host, specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="datagram"></param>
        /// <param name="header"></param>
        /// <param name="flags"></param>
        /// <param name="args"></param>
        void HandleFrom(scoped ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args);
    }
}
