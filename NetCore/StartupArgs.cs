using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace NetCore
{
    /// <summary>
    /// Handler providing common arguments for a startup of an <see cref="NetworkMember"/>.
    /// </summary>
    /// <param name="args">Args to setup.</param>
    public delegate void StartupArgsProvider(StartupArgs args);

    /// <summary>
    /// Class, storing common data for starting-up the <see cref="NetworkMember"/>.
    /// </summary>
    /// TODO: Revisit and decide whether using <see cref="object"/> as a key is good enough performance-wise and flexibility-wise.
    public sealed class StartupArgs : Dictionary<object, object?>, IReadOnlyStartupArgs
    {
        /// <summary>
        /// Key for <see cref="LocalIPEndPoint"/>.
        /// </summary>
        public static readonly RuntimeTypeHandle LocalIPEndPointKey = typeof(IPEndPoint).TypeHandle;
        /// <summary>
        /// Key for <see cref="LocalUnixEndPoint"/>.
        /// </summary>
        public static readonly RuntimeTypeHandle LocalUnixEndPointKey = typeof(UnixDomainSocketEndPoint).TypeHandle;
        /// <summary>
        /// Key for <see cref="ServerName"/>
        /// </summary>
        public const string ServerNameKey = "ServerName";
        
        /// <inheritdoc/>
        public IPEndPoint? LocalIPEndPoint
        {
            get => (IPEndPoint?)this.GetValueOrDefault(LocalIPEndPointKey);
            set => this[LocalIPEndPointKey] = value;
        }
        
        /// <inheritdoc/>
        public UnixDomainSocketEndPoint? LocalUnixEndPoint
        {
            get => (UnixDomainSocketEndPoint?)this.GetValueOrDefault(LocalUnixEndPointKey);
            set => this[LocalUnixEndPointKey] = value;
        }
        
        /// <inheritdoc/>
        public string? ServerName
        {
            get => (string?)this.GetValueOrDefault(ServerNameKey);
            set => this[ServerNameKey] = value;
        }
    }
}
