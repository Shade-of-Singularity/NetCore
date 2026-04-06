namespace NetCore
{
    /// <summary>
    /// Describes target message sending mode.
    /// </summary>
    /// Note (for maintainers): Packs to 2 bits, and is used in <see cref="HeaderFlagsHelpers"/> extension class.
    public enum SendingMode : byte
    {
        /// <summary>
        /// (Unreliable, Unordered) message sending mode.
        /// </summary>
        Unreliable = 0b00, // -> 0
        /// <summary>
        /// (Reliable, Unordered) message sending mode.
        /// </summary>
        Reliable = 0b01, // -> 1
        /// <summary>
        /// (Unreliable, Ordered) message sending mode.
        /// </summary>
        Sequential = 0b10, // -> 2
        /// <summary>
        /// (Reliable, Ordered) message sending mode.
        /// </summary>
        Resilient = 0b11, // -> 3
    }
}
