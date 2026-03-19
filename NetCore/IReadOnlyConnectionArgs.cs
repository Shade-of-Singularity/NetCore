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
    }
}
