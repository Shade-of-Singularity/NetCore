using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace NetCore
{
    /// <summary>
    /// Read-only interface for <see cref="StartupArgs"/>.
    /// </summary>
    public interface IReadOnlyStartupArgs : IReadOnlyNetworkingArgs
    {
        /// <summary>
        /// Local end-point, on which <see cref="NetworkMember"/> needs to start.
        /// </summary>
        IPEndPoint? LocalIPEndPoint { get; }
        /// <summary>
        /// Local Unix end-point, on which <see cref="NetworkMember"/> needs to start.
        /// </summary>
        UnixDomainSocketEndPoint? LocalUnixEndPoint { get; }
        /// <summary>
        /// Host name used for public indexing.
        /// </summary>
        string? ServerName { get; }

        /// <summary>
        /// Attempts to retrieve <see cref="LocalIPEndPoint"/> from <see cref="IReadOnlyStartupArgs"/>.
        /// </summary>
        /// <param name="endPoint">Valid <see cref="LocalIPEndPoint"/>.</param>
        /// <returns>
        /// <c>true</c> - if <see cref="LocalIPEndPoint"/> was found and it is not <c>null</c>.
        /// <c>false</c> - otherwise.
        /// </returns>
        public bool TryGetLocalIPEndPoint([NotNullWhen(true)] out IPEndPoint? endPoint)
        {
            return (endPoint = LocalIPEndPoint) is not null;
        }
        /// <summary>
        /// Attempts to retrieve <see cref="LocalUnixEndPoint"/> from <see cref="IReadOnlyStartupArgs"/>.
        /// </summary>
        /// <param name="endPoint">Valid <see cref="LocalUnixEndPoint"/>.</param>
        /// <returns>
        /// <c>true</c> - if <see cref="LocalUnixEndPoint"/> was found and it is not <c>null</c>.
        /// <c>false</c> - otherwise.
        /// </returns>
        public bool TryGetLocalUnixEndPoint([NotNullWhen(true)] out UnixDomainSocketEndPoint? endPoint)
        {
            return (endPoint = LocalUnixEndPoint) is not null;
        }
        /// <summary>
        /// Attempts to retrieve <see cref="LocalIPEndPoint"/> from <see cref="IReadOnlyStartupArgs"/>.
        /// </summary>
        /// <param name="serverName">Server name to use.</param>
        /// <returns>
        /// <c>true</c> - if <see cref="LocalIPEndPoint"/> was found and it is not <c>null</c>.
        /// <c>false</c> - otherwise.
        /// </returns>
        public bool TryGetServerName([NotNullWhen(true)] out string? serverName)
        {
            return (serverName = ServerName) is not null;
        }
    }
}
