namespace NetCore
{
    /// <summary>
    /// Current state of the member.
    /// </summary>
    public enum MemberState : byte
    {
        /// <summary>
        /// Indicates that member is completely stopped and disconnected.
        /// </summary>
        Stopped,
        /// <summary>
        /// Member is currently starting.
        /// </summary>
        Starting,
        /// <summary>
        /// Member is currently stopping.
        /// </summary>
        Stopping,
        /// <summary>
        /// Member is started, with no established connection with a remote host.
        /// </summary>
        /// <remarks>
        /// With <see cref="Server"/> - indicates that <see cref="Server"/> is not connected to a relay node.
        /// </remarks>
        Started_Idle,
        /// <summary>
        /// Member is started and currently connecting starting a connection attempt with a remote host.
        /// </summary>
        /// <remarks>
        /// With <see cref="Server"/> - indicates that <see cref="Server"/> is connecting to a relay node.
        /// </remarks>
        Started_Connecting,
        /// <summary>
        /// Member is started and currently disconnecting from a remote host.
        /// </summary>
        /// <remarks>
        /// With <see cref="Server"/> - indicates that <see cref="Server"/> is disconnecting from a relay node.
        /// </remarks>
        Started_Disconnecting,
        /// <summary>
        /// Member is started and last connection operation completed successfully.
        /// In <see cref="NetworkMember"/> only indicate that managed transports has completed their connection operation.
        /// Additional check is required to verify that at least one of the transports is actually connected to a remote host.
        /// </summary>
        /// <remarks>
        /// With <see cref="Server"/> - indicates that <see cref="Server"/> is currently connected to a relay node.
        /// </remarks>
        Started_Connected,
    }
}
