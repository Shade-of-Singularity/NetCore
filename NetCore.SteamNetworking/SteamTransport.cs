using NetCore.Transports;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace NetCore.SteamNetworking
{
    /// <summary>
    /// Transport over <see cref="SteamNetworkingSockets"/>.
    /// </summary>
    public class SteamTransport : Transport, IGeneralTransport
    {
        readonly struct SteamConnection
        {
            public readonly HSteamNetConnection connection;
            public readonly ConnectionID id;
        }

        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private static bool IsRelayAccessInitialized;

        // General:
        private readonly Dictionary<ConnectionID, SteamConnection> m_Connections = [];
        private readonly byte[] Bytes = new byte[2048]; // Simple implementation for a test.

        // Server-side:
        private HSteamListenSocket m_ServerSocket;
        private Callback<SteamNetConnectionStatusChangedCallback_t>? m_ConnectionCallback;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc/>
        public override bool HasConnection(ConnectionID connection) => false;

        /// <inheritdoc/>
        public override void Attach(NetworkMember member)
        {
            base.Attach(member);
            if (!IsRelayAccessInitialized)
            {
                SteamNetworkingUtils.InitRelayNetworkAccess();
                IsRelayAccessInitialized = true;
            }
        }

        /// <inheritdoc/>
        public override AsyncTask Start(IReadOnlyStartupArgs args, CancellationToken token)
        {
            if (IsServerSide)
            {
                m_ServerSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, 0, default);
            }

            m_ConnectionCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnConnectionChanged);
            return base.Start(args, token);
        }

        private void OnConnectionChanged(SteamNetConnectionStatusChangedCallback_t args)
        {
            if (args.m_info.m_eState == ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting)
            {
                SteamNetworkingSockets.AcceptConnection(args.m_hConn);
            }
        }

        /// <inheritdoc/>
        public override AsyncTask Stop()
        {
            return base.Stop();
        }

        /// <inheritdoc/>
        public override AsyncTask Connect(IReadOnlyConnectionArgs args, CancellationToken token)
        {
            return base.Connect(args, token);
        }

        /// <inheritdoc/>
        public override AsyncTask Disconnect()
        {
            return base.Disconnect();
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                  Handling
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        public void HandleReliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source)
        {
            throw new NotImplementedException();
        }

        public void HandleReliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }

        public void HandleResilient(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source)
        {
            throw new NotImplementedException();
        }

        public void HandleResilient(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }

        public void HandleSequential(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source)
        {
            throw new NotImplementedException();
        }

        public void HandleSequential(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }

        public void HandleUnreliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source)
        {
            throw new NotImplementedException();
        }

        public void HandleUnreliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                  Sending
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>

        public void SendReliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags)
        {
            throw new NotImplementedException();
        }

        public void SendReliableExcluding(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude)
        {
            throw new NotImplementedException();
        }

        public void SendReliableTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target)
        {
            lock (_lock)
            {
                if (m_Connections.TryGetValue(target, out SteamConnection client))
                {
                    datagram.CopyTo(Bytes);
                    var array = GCHandle.Alloc(Bytes);
                    var pointer = array.AddrOfPinnedObject();

                    try
                    {
                        int mode = flags.HasFlag<NoDelay>()
                            ? Constants.k_nSteamNetworkingSend_ReliableNoNagle
                            : Constants.k_nSteamNetworkingSend_Reliable;

                        var result = SteamNetworkingSockets.SendMessageToConnection(
                            client.connection, pointer, (uint)datagram.Length, mode, out long messageNumber);
                        GD.Print($"Message ({messageNumber}), Result: {result}");
                    }
                    finally
                    {
                        array.Free();
                    }
                }
            }
        }

        public void SendReliableTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }

        public void SendResilient(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags)
        {
            throw new NotImplementedException();
        }

        public void SendResilientExcluding(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude)
        {
            throw new NotImplementedException();
        }

        public void SendResilientTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target)
        {
            throw new NotImplementedException();
        }

        public void SendResilientTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }

        public void SendSequential(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags)
        {
            throw new NotImplementedException();
        }

        public void SendSequentialExcluding(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude)
        {
            throw new NotImplementedException();
        }

        public void SendSequentialTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target)
        {
            throw new NotImplementedException();
        }

        public void SendSequentialTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }

        public void SendUnreliable(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags)
        {
            throw new NotImplementedException();
        }

        public void SendUnreliableExcluding(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude)
        {
            throw new NotImplementedException();
        }

        public void SendUnreliableTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target)
        {
            throw new NotImplementedException();
        }

        public void SendUnreliableTo(in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
