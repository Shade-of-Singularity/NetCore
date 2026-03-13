using System;
using System.Net;

namespace NetCore.UDP
{
public partial class UDPTransport
    {
        /// <summary>
        /// Stores info about currently connected client.
        /// </summary>
        protected sealed class ClientData(ConnectionID id, IPEndPoint remoteEndPoint)
        {
            /// <summary>
            /// Amount of ticks which passed since the last time this <see cref="ClientData"/> was used.
            /// </summary>
            public int InactiveTickDelta => Environment.TickCount - LastActiveTick;

            /// <summary>
            /// Connection ID of our client.
            /// </summary>
            public readonly ConnectionID ConnectionID = id;
            /// <summary>
            /// Remote end-point
            /// </summary>
            public readonly IPEndPoint RemoteEndPoint = remoteEndPoint;
            /// <summary>
            /// Last tick on which client was active.
            /// </summary>
            public int LastActiveTick = Environment.TickCount;

            /// <summary>
            /// Updates <see cref="LastActiveTick"/> to avoid timeouts.
            /// </summary>
            public void NoSleep() => LastActiveTick = Environment.TickCount;
        }
    }
}
