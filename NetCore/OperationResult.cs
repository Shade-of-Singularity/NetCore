using Cysharp.Threading.Tasks;

namespace NetCore
{
    /// <summary>
    /// Result of the operation execution.
    /// </summary>
    public enum OperationResult : byte
    {
        /// <summary>
        /// This state should be ignored.
        /// Operation was either cancelled by another operation, or operation is a completed <see cref="UniTask.FromResult{T}(T)"/> task.
        /// </summary>
        CancelledOrInvalid,
        /// <summary>
        /// Operation succeeded.
        /// </summary>
        Success,
        /// <summary>
        /// Operation failed due to errors. See console for more info.
        /// </summary>
        Failed,
    }
}
