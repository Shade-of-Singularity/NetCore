using NetCore.Transports;

namespace NetCore
{
    /// <summary>
    /// Current starting state of an network member.
    /// </summary>
    public enum StartupState : byte
    {
        /// <summary>
        /// An network member is fully stopped.
        /// </summary>
        Stopped,
        /// <summary>
        /// An network member and its <see cref="ITransport"/>s (if any) are starting.
        /// </summary>
        Starting,
        /// <summary>
        /// An network member and its <see cref="ITransport"/>s (if any) have started.
        /// </summary>
        Started,
        /// <summary>
        /// An network member and its <see cref="ITransport"/>s (if any) are stopping.
        /// </summary>
        Stopping,
    }
}
