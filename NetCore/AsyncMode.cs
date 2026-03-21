namespace NetCore
{
    /// <summary>
    /// Describes supported async modes.
    /// </summary>
    public enum AsyncMode : byte
    {
        /// <summary>
        /// No async modes are supported.
        /// </summary>
        Synced = 0,
        /// <summary>
        /// Single-threaded async mode is supported.
        /// </summary>
        AsyncSingleThreaded = 1,
        /// <summary>
        /// Multi-threaded async mode is supported.
        /// </summary>
        AsyncMultiThreaded = 2,
    }
}
