using System.Net;

namespace NetCore.Transports.UDP
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
            public int InactiveTickDelta => System.Environment.TickCount - m_LastActiveTick;
            /// <summary>
            /// Last tick on which this client was active.
            /// </summary>
            public int LastActiveTick => m_LastActiveTick;

            /// <summary>
            /// Connection ID of our client.
            /// </summary>
            public readonly ConnectionID ConnectionID = id;
            /// <summary>
            /// Remote end-point
            /// </summary>
            public readonly IPEndPoint RemoteEndPoint = remoteEndPoint;
            /// <inheritdoc cref="LastActiveTick"/>
            private int m_LastActiveTick = System.Environment.TickCount;

            /// <summary>
            /// Updates <see cref="LastActiveTick"/> to avoid timeouts.
            /// </summary>
            public void NoSleep() => m_LastActiveTick = System.Environment.TickCount;
        }
    }
}
