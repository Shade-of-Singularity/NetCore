namespace NetCore
{
    /// <summary>
    /// Result of the operation execution.
    /// </summary>
    public enum OperationResult : byte
    {
        /// <summary>
        /// Operation succeeded.
        /// </summary>
        Success,
        /// <summary>
        /// Operation failed due to errors. See console for more info.
        /// </summary>
        Failed,
        /// <summary>
        /// Operation was cancelled by another operation.
        /// </summary>
        Cancelled,
    }
}
