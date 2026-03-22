namespace NetCore
{
    /// <summary>
    /// Describes supported async modes.
    /// </summary>
    public enum AsyncMode : byte
    {
        /// <summary>
        /// No async modes are supported.
        /// Assumed to be always supported.
        /// </summary>
        Synced = 0,
        /// <summary>
        /// Single-threaded async mode is supported.
        /// </summary>
        AsyncSingleThreaded = 0b01,
        /// <summary>
        /// Multi-threaded async mode is supported.
        /// </summary>
        AsyncMultiThreaded = 0b10,
        /// <summary>
        /// Supports any and all async modes.
        /// </summary>
        Any = 0b11,
    }
}
