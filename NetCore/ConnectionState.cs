using NetCore.Transports;

namespace NetCore
{
    /// <summary>
    /// Current connection state of a <see cref="NetworkMember"/>.
    /// </summary>
    public enum ConnectionState : byte
    {
        /// <summary>
        /// <see cref="NetworkMember"/> is not connected to a remote host and is now idling.
        /// </summary>
        Idle,
        /// <summary>
        /// <see cref="NetworkMember"/> is now connecting to a remote host with own <see cref="ITransport"/>s.
        /// </summary>
        Connecting,
        /// <summary>
        /// <see cref="NetworkMember"/> is connected to a remote host.
        /// Indicated by at least one <see cref="ITransport"/> having success in communicating with a remote host.
        /// </summary>
        Connected,
        /// <summary>
        /// <see cref="NetworkMember"/> is disconnecting all its <see cref="ITransport"/>s from a remote host.
        /// </summary>
        Disconnecting,
    }
}
