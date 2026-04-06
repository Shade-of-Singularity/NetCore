using NetCore.Transports;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace NetCore.SteamNetworking
{
    /// <summary>
    /// Transport over <see cref="SteamNetworkingSockets"/>.
    /// </summary>
    public class SteamTransport : Transport, IUnreliableTransport, IReliableTransport
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
        /// <inheritdoc/>
        public void HandleReliable(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void HandleReliable(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs source)
        {
            throw new NotSupportedException("Steam transport does not support direct message handling.");
        }

        /// <inheritdoc/>
        public void HandleUnreliable(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID source)
        {
            GD.Print($"Handled (from {source}): {datagram.ToString()}");
        }

        /// <inheritdoc/>
        public void HandleUnreliable(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs source)
        {
            throw new NotSupportedException("Steam transport does not support direct message handling.");
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                  Sending
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc/>
        public void SendReliable(ReadOnlySpan<byte> datagram, in Header header, in Flags flags)
        {
            int mode = Constants.k_nSteamNetworkingSend_Reliable | GetModeModifiers(in flags);
            lock (_lock)
            {
                foreach (var client in m_Connections.Values)
                {
                    SendToCore(client.connection, Bytes, in datagram, in header, mode);
                }
            }
        }

        /// <inheritdoc/>
        public void SendReliableExcluding(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude)
        {
            int mode = Constants.k_nSteamNetworkingSend_Reliable | GetModeModifiers(in flags);
            lock (_lock)
            {
                foreach (var client in m_Connections.Values)
                {
                    if (client.id == toExclude)
                        continue;

                    SendToCore(client.connection, Bytes, in datagram, in header, mode);
                }
            }
        }

        /// <inheritdoc/>
        public void SendReliableTo(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target)
        {
            int mode = Constants.k_nSteamNetworkingSend_Reliable | GetModeModifiers(in flags);
            lock (_lock)
            {
                if (m_Connections.TryGetValue(target, out SteamConnection client))
                {
                    SendToCore(client.connection, Bytes, in datagram, in header, mode);
                }
            }
        }

        /// <inheritdoc/>
        public void SendReliableTo(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args)
        {
            throw new NotSupportedException("Steam transport does not support direct message handling.");
        }

        /// <inheritdoc/>
        public void SendUnreliable(ReadOnlySpan<byte> datagram, in Header header, in Flags flags)
        {
            int mode = Constants.k_nSteamNetworkingSend_Unreliable | GetModeModifiers(in flags);
            lock (_lock)
            {
                foreach (var client in m_Connections.Values)
                {
                    SendToCore(client.connection, Bytes, in datagram, in header, mode);
                }
            }
        }

        /// <inheritdoc/>
        public void SendUnreliableExcluding(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude)
        {
            int mode = Constants.k_nSteamNetworkingSend_Unreliable | GetModeModifiers(in flags);
            lock (_lock)
            {
                foreach (var client in m_Connections.Values)
                {
                    if (client.id == toExclude)
                        continue;

                    SendToCore(client.connection, Bytes, in datagram, in header, mode);
                }
            }
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target)
        {
            int mode = Constants.k_nSteamNetworkingSend_Unreliable | GetModeModifiers(in flags);
            lock (_lock)
            {
                if (m_Connections.TryGetValue(target, out SteamConnection client))
                {
                    SendToCore(client.connection, Bytes, in datagram, in header, mode);
                }
            }
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args)
        {
            throw new NotSupportedException("Steam transport does not support direct message handling.");
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int GetModeModifiers(in Flags flags) => 0
        | (flags.HasFlag<NoDelay>() ? Constants.k_nSteamNetworkingSend_NoDelay : 0)
        | (flags.HasFlag<NoNagle>() ? Constants.k_nSteamNetworkingSend_NoNagle : 0)
        | (flags.HasFlag<UseCurrentThread>() ? Constants.k_nSteamNetworkingSend_UseCurrentThread : 0);

        private static void SendToCore(HSteamNetConnection to, byte[] buffer, ReadOnlySpan<byte> datagram, in Header header, int mode)
        {
            datagram.CopyTo(buffer);
            var array = GCHandle.Alloc(buffer);
            var pointer = array.AddrOfPinnedObject();

            try
            {
                var result = SteamNetworkingSockets.SendMessageToConnection(to, pointer, (uint)datagram.Length, mode, out long messageNumber);
#if GODOT
                GD.Print($"Message ({messageNumber}), Result: {result}");
#endif
            }
            finally
            {
                array.Free();
            }
        }
    }
}
