using Cysharp.Threading.Tasks;
using System.Threading;

namespace NetCore
{
    /// <summary>
    /// State operation within an <see cref="NetworkMember"/> or <see cref="Transports.ITransport"/>.
    /// </summary>
    /// <param name="source"><see cref="CancellationTokenSource"/> for cancelling an <paramref name="activation"/> task.</param>
    /// <param name="activation">An activation task</param>
    /// <param name="deactivation">An deactivation task.</param>
    /// <param name="type">Type of the operation.</param>
    public readonly struct StatefulOperation(CancellationTokenSource? source, UniTask<OperationResult> activation, UniTask<OperationResult> deactivation, OperationType type)
    {
        /// <summary>
        /// Completed task returning an <see cref="OperationResult.Cancelled"/>.
        /// </summary>
        public static readonly UniTask<OperationResult> CompletedTask = UniTask.FromResult(OperationResult.Cancelled);
        /// <summary>
        /// Completed operation with all tasks completed.
        /// </summary>
        public static readonly StatefulOperation CompletedOperation = new(null, CompletedTask, CompletedTask, OperationType.Completed);
        /// <summary>
        /// <see cref="CancellationTokenSource"/> for cancelling an <see cref="Activation"/> task.
        /// </summary>
        public readonly CancellationTokenSource? ActivationTokenSource = source;
        /// <summary>
        /// Activation task of an operation (e.g. <see cref="NetworkMember.StartOperation"/> or <see cref="NetworkMember.ConnectOperation"/>)
        /// </summary>
        /// <remarks>
        /// It is expected to be cancellable using <see cref="ActivationTokenSource"/>.
        /// </remarks>
        public readonly UniTask<OperationResult> Activation = activation;
        /// <summary>
        /// Deactivation task of an operation (e.g. <see cref="NetworkMember.StopOperation"/> or <see cref="NetworkMember.DisconnectOperation"/>)
        /// </summary>
        public readonly UniTask<OperationResult> Deactivation = deactivation;
        /// <summary>
        /// Which operations this <see cref="StatefulOperation"/> holds.
        /// </summary>
        public readonly OperationType Type = type;
        /// <summary>
        /// Cancels the <see cref="Activation"/> task from this <see cref="StatefulOperation"/>.
        /// </summary>
        /// <returns>Now cancelled <see cref="Activation"/> operation to await a cancellation for.</returns>
        public UniTask<OperationResult> CancelActivation()
        {
            if (ActivationTokenSource is not null && !ActivationTokenSource.IsCancellationRequested)
            {
                ActivationTokenSource.Cancel();
                ActivationTokenSource.Dispose();
            }

            return Activation;
        }
    }
}
