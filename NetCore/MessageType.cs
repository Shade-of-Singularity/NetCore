namespace NetCore
{
    /// <summary>
    /// Type of a message which is being sent.
    /// Sits in a very front of the message, to identify how special headers should be formed.
    /// </summary>
    public enum MessageType : byte
    {
        /// <summary>
        /// System message, like ACK, and similar.
        /// </summary>
        /// <remarks>
        /// System messages does not contain <see cref="CustomHeaders"/>.
        /// </remarks>
        System = 0,
        /// <summary>
        /// Ordered reliable message type.
        /// </summary>
        OrderedReliable = 1,
        /// <summary>
        /// Unordered reliable message type.
        /// </summary>
        UnorderedReliable = 2,
        /// <summary>
        /// Ordered reliable message type.
        /// </summary>
        OrderedUnreliable = 3,
        /// <summary>
        /// Unordered reliable message type.
        /// </summary>
        UnorderedUnreliable = 4,
        /// <summary>
        /// Indicates whether message has a custom header.
        /// </summary>
        HasCustomHeader = 0b1000000,
    }
}
