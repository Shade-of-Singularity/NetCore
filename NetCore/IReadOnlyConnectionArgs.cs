using System;
using System.Net;
using System.Net.Sockets;

namespace NetCore
{
    /// <summary>
    /// Read-only interface for <see cref="ConnectionArgs"/>.
    /// </summary>
    public interface IReadOnlyConnectionArgs : IReadOnlyNetworkingArgs
    {
        /// <summary>
        /// Remote IP end-point to connect to.
        /// </summary>
        IPEndPoint? RemoteIPEndPoint { get; }
        /// <summary>
        /// Remote Unix end-point to connect to.
        /// </summary>
        UnixDomainSocketEndPoint? RemoteUnixEndPoint { get; }
        /// <summary>
        /// Temporary identifier, used for identifying a connection source.
        /// Usually used to discard connections over secondary transports, if another transport was already active.
        /// </summary>
        /// <remarks>
        /// Identifier *does not* persist between sessions - use centralized server instead.
        /// </remarks>
        public Guid TemporaryIdentifier { get; }

        /// <summary>
        /// Attempts to retrieve <see cref="RemoteIPEndPoint"/> from <see cref="IReadOnlyStartupArgs"/>.
        /// </summary>
        /// <param name="endPoint">Valid <see cref="RemoteIPEndPoint"/>.</param>
        /// <returns>
        /// <c>true</c> - if <see cref="RemoteIPEndPoint"/> was found and it is not <c>null</c>.
        /// <c>false</c> - otherwise.
        /// </returns>
        public bool TryGetRemoteIPEndPoint(out IPEndPoint? endPoint)
        {
            return (endPoint = RemoteIPEndPoint) is not null;
        }
        /// <summary>
        /// Attempts to retrieve <see cref="RemoteUnixEndPoint"/> from <see cref="IReadOnlyStartupArgs"/>.
        /// </summary>
        /// <param name="endPoint">Valid <see cref="RemoteUnixEndPoint"/>.</param>
        /// <returns>
        /// <c>true</c> - if <see cref="RemoteUnixEndPoint"/> was found and it is not <c>null</c>.
        /// <c>false</c> - otherwise.
        /// </returns>
        public bool TryGetRemoteUnixEndPoint(out UnixDomainSocketEndPoint? endPoint)
        {
            return (endPoint = RemoteUnixEndPoint) is not null;
        }
        /// <summary>
        /// Attempts to retrieve <see cref="TemporaryIdentifier"/> from <see cref="IReadOnlyStartupArgs"/>.
        /// </summary>
        /// <param name="identifier">Temporary identifier or <see cref="Guid.Empty"/></param>
        /// <returns>
        /// <c>true</c> - if <see cref="TemporaryIdentifier"/> was found and it is not <see cref="Guid.Empty"/>.
        /// <c>false</c> - otherwise.
        /// </returns>
        public bool TryGetTemporaryIdentifier(out Guid identifier)
        {
            return (identifier = TemporaryIdentifier) != Guid.Empty;
        }
    }
}
