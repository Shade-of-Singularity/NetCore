namespace NetCore
{
    /// <summary>
    /// Current operation type of an <see cref="StatefulOperation"/>
    /// </summary>
    public enum OperationType : byte
    {
        /// <summary>
        /// Operation was not started yet.
        /// </summary>
        Idle,
        /// <summary>
        /// <see cref="StartState.Starting"/> or <see cref="ConnectionState.Connecting"/>.
        /// </summary>
        /// <remarks>
        /// When set in <see cref="StatefulOperation"/> - only guarantees <see cref="StatefulOperation.Activation"/> to be valid.
        /// </remarks>
        Activating,
        /// <summary>
        /// Combines both operation types.
        /// Operation order is strictly <see cref="Deactivating"/> and then <see cref="Activating"/>.
        /// </summary>
        /// <remarks>
        /// When set in <see cref="StatefulOperation"/> - indicates that both tasks are provided and valid.
        /// </remarks>
        Restarting,
        /// <summary>
        /// <see cref="StartState.Stopping"/> or <see cref="ConnectionState.Disconnecting"/>.
        /// </summary>
        /// <remarks>
        /// When set in <see cref="StatefulOperation"/> - only guarantees <see cref="StatefulOperation.Deactivation"/> to be valid.
        /// </remarks>
        Deactivating,
        /// <summary>
        /// Operation was completed successfully.
        /// </summary>
        Completed,
    }
}
