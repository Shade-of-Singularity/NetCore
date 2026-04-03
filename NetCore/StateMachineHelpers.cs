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
        public static readonly OperationResultTask CompletedTask = AsyncTask.FromResult(OperationResult.CancelledOrInvalid);
    }
}
