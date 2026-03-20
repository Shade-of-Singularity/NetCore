namespace NetCore.Transports
{
    /// <summary>
    /// Result from calling some methods from <see cref="ITransport"/>
    /// </summary>
    public enum ConnectionResult : byte
    {
        /// <summary>
        /// Connection 
        /// </summary>
        Success = 0,
        /// <summary>
        /// Operation failed.
        /// </summary>
        Fail = 0,
        /// <summary>
        /// Operation skipped (for example, because <see cref="ITransport"/> was not provided all the data needed to connect).
        /// </summary>
        Skip = 1,
    }
}
