using NetCore.Transports;

namespace NetCore
{
    /// <summary>
    /// Current starting state of an <see cref="NetworkMember"/>.
    /// </summary>
    public enum StartState : byte
    {
        /// <summary>
        /// <see cref="NetworkMember"/> is fully stopped.
        /// </summary>
        Stopped,
        /// <summary>
        /// <see cref="NetworkMember"/> and its <see cref="ITransport"/>s are starting.
        /// </summary>
        Starting,
        /// <summary>
        /// <see cref="NetworkMember"/> and its <see cref="ITransport"/>s have started.
        /// </summary>
        Started,
        /// <summary>
        /// <see cref="NetworkMember"/> and its <see cref="ITransport"/>s are stopping.
        /// </summary>
        Stopping,
    }
}
