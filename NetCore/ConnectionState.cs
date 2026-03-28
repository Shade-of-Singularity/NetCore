using NetCore.Transports;

namespace NetCore
{
    /// <summary>
    /// Current connection state of an network member.
    /// </summary>
    public enum ConnectionState : byte
    {
        /// <summary>
        /// An network member is not connected to a remote host and is now idling.
        /// </summary>
        Idle,
        /// <summary>
        /// An network member is now connecting to a remote host with own <see cref="ITransport"/>s (or by itself).
        /// </summary>
        Connecting,
        /// <summary>
        /// An network member is connected to a remote host.
        /// Indicated by at least one <see cref="ITransport"/> (or member itself) having success in communicating with a remote host.
        /// </summary>
        Connected,
        /// <summary>
        /// An network member is disconnecting all its <see cref="ITransport"/>s from a remote host (if any).
        /// </summary>
        Disconnecting,
    }
}
