using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace NetCore
{
    /// <summary>
    /// Handler providing common arguments for a connection of an <see cref="NetworkMember"/>.
    /// </summary>
    /// <param name="args">Args to setup.</param>
    public delegate void ConnectionArgsHandler(ConnectionArgs args);

    /// <summary>
    /// Connection args provided to <see cref="NetworkMember"/> for connecting to a remote host.
    /// </summary>
    /// TODO: Revisit and decide whether using <see cref="object"/> as a key is good enough performance-wise and flexibility-wise.
    public sealed class ConnectionArgs : Dictionary<object, object?>, IReadOnlyConnectionArgs
    {
        /// <summary>
        /// Key for <see cref="RemoteIPEndPoint"/>
        /// </summary>
        public static readonly RuntimeTypeHandle RemoteIPEndPointKey = typeof(IPEndPoint).TypeHandle;
        /// <summary>
        /// Key for <see cref="RemoteUnixEndPoint"/>.
        /// </summary>
        public static readonly RuntimeTypeHandle RemoteUnixEndPointKey = typeof(UnixDomainSocketEndPoint).TypeHandle;
        /// <summary>
        /// Key for <see cref="TemporaryIdentifier"/>.
        /// </summary>
        public static readonly string TemporaryIdentifierKey = "TemporaryIdentifier";

        /// <inheritdoc/>
        public IPEndPoint? RemoteIPEndPoint
        {
            get => (IPEndPoint?)this.GetValueOrDefault(RemoteIPEndPointKey);
            set => this[RemoteIPEndPointKey] = value;
        }
        
        /// <inheritdoc/>
        public UnixDomainSocketEndPoint? RemoteUnixEndPoint
        {
            get => (UnixDomainSocketEndPoint?)this.GetValueOrDefault(RemoteUnixEndPointKey);
            set => this[RemoteUnixEndPointKey] = value;
        }

        /// <inheritdoc/>
        public Guid TemporaryIdentifier
        {
            get => this.TryGet(TemporaryIdentifierKey, out Guid result) ? result : Guid.Empty;
            set => this[TemporaryIdentifierKey] = value;
        }
    }
}
