using Cysharp.Threading.Tasks;

namespace NetCore
{
    /// <summary>
    /// Helper class for working with state machines.
    /// </summary>
    public static class StateMachineHelpers
    {
        /// <summary>
        /// Completed task returning <see cref="OperationResult.CancelledOrInvalid"/> value.
        /// </summary>
        public static readonly UniTask<OperationResult> CompletedTask = UniTask.FromResult(OperationResult.CancelledOrInvalid);
    }
}
