namespace NetCore
{
    /// <summary>
    /// Interface for working with session-based message sending methods.
    /// </summary>
    /// Note: Internal AutoGen covered.
    public partial interface ISendNetworkMessaging
    {
        /// <summary>
        /// <para>Unreliably sends <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        void SendUnreliable(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags);
        /// <summary>
        /// <para>Reliably sends <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        void SendReliable(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags);
        /// <summary>
        /// <para>Sequentially sends <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        void SendSequential(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags);
        /// <summary>
        /// <para>Resiliently sends <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        void SendResilient(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags);
    }
}
