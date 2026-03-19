using System;
using System.Net;

namespace NetCore.Transports.UDP
{
    public partial class UDPTransport
    {
        /// <summary>
        /// Internal Client identifier, which can be matched by either <see cref="ConnectionID"/> or <see cref="RemoteEndPoint"/>.
        /// </summary>
        protected readonly struct ClientID(ConnectionID id, IPEndPoint? remoteEndPoint) : IEquatable<ClientID>
        {
            /// <summary>
            /// ConnectionID of our client.
            /// </summary>
            public readonly ConnectionID ConnectionID = id;
            /// <summary>
            /// Remote end-point of our client.
            /// </summary>
            public readonly IPEndPoint? RemoteEndPoint = remoteEndPoint;
            /// <summary>
            /// Constructor for a <see cref="ConnectionID"/>-only.
            /// </summary>
            public ClientID(ConnectionID connection) : this(connection, null) { }
            /// <summary>
            /// Constructor for a <see cref="RemoteEndPoint"/>-only.
            /// </summary>
            public ClientID(IPEndPoint remoteEndPoint) : this(default, remoteEndPoint) { }

            /// <inheritdoc/>
            public override string ToString() => $"ID: ({ConnectionID}) RemoteIPEndPoint: (...:{RemoteEndPoint?.Port})";

            /// <inheritdoc/>
            public override bool Equals(object obj) => obj is ClientID id && Equals(id);

            /// <inheritdoc/>
            public override int GetHashCode() => ConnectionID.raw.GetHashCode();

            /// <inheritdoc/>
            public bool Equals(ClientID other)
            {
                return other.ConnectionID == ConnectionID || (other.RemoteEndPoint is not null && other.RemoteEndPoint.Equals(RemoteEndPoint));
            }
        }
    }
}
