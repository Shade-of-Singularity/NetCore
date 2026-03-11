using NetCore.Loopback;
using NetCore.TCP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace NetCore.UDP
{
    /// <summary>
    /// Transport for UDP messages.
    /// </summary>
    /// <remarks>
    /// Does not implement networking functionality at the moment.
    /// </remarks>
    public class UDPTransport : Transport, IUnreliableTransport
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Public Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Socket type used by the internal <see cref="Socket"/>.
        /// </summary>
        protected virtual SocketType SocketType => SocketType.Dgram;

        /// <summary>
        /// Protocol used by the internal <see cref="Socket"/>.
        /// </summary>
        protected virtual ProtocolType SocketProtocol => ProtocolType.Udp;

        /// <summary>
        /// <see cref="TCP.TCPTransport"/> pair this <see cref="UDPTransport"/> with.
        /// </summary>
        public virtual TCPTransport? TCPTransport
        {
            get => m_TCPTransport;
            set
            {
                lock (_lock)
                {
                    if (IsInitialized || value?.IsInitialized == true)
                    {
                        throw new Exception($"Cannot pair UDP with TCP after any of the transports were initialized/attached to a {nameof(NetworkMember)}.");
                    }

                    m_TCPTransport = value;
                }
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Protected Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Lock, used when interacting with everything - <see cref="Socket"/>, <see cref="Source"/>, <see cref="Clients"/>, etc.
        /// </summary>
        protected readonly object _lock = new();
        /// <summary>
        /// Socket, used for receiving and sending of the data.
        /// </summary>
        protected Socket? Socket;
        /// <summary>
        /// <see cref="CancellationTokenSource"/> used to stop <see cref="Socket"/> from listening to incoming messages.
        /// </summary>
        protected CancellationTokenSource? Source;
        /// <summary>
        /// Stores data about active connections with clients.
        /// </summary>
        protected Dictionary<ConnectionID, ClientData> Clients = [];
        /// <summary>
        /// Byte buffer used for sending and retrieving data (since .NET Standard 2.1 doesn't support spans with one Socket connection)
        /// </summary>
        protected byte[] Buffer = new[];
        /// <summary>
        /// <see cref="TCP.TCPTransport"/> to pair this <see cref="UDPTransport"/> with.
        /// </summary>
        private TCPTransport? m_TCPTransport;

        /// <summary>
        /// Stores info about currently connected client.
        /// </summary>
        protected sealed class ClientData(IPEndPoint remoteEndPoint)
        {
            /// <summary>
            /// Amount of ticks which passed since the last time this <see cref="ClientData"/> was used.
            /// </summary>
            public int InactiveTickDelta => Environment.TickCount - LastActiveTick;

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




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc/>
        public override void Initialize(NetworkMember member)
        {
            lock (_lock)
            {
                base.Initialize(member);
                Socket = new(SocketType, SocketProtocol);
            }
        }

        /// <inheritdoc/>
        public override void Terminate(NetworkMember member)
        {
            lock (_lock)
            {
                Socket!.Dispose();
                base.Terminate(member);
            }
        }

        /// <inheritdoc/>
        public override void Start(IPEndPoint localEndPoint)
        {
            // Socket.Bind to either ANY address or ANY port changes local end-point.
            // To handle this, we update registered end-point as well.
            // Note: It is recommended to assign a port manually, rather than relying on ANY port.
            // Note: Socket.LocalEndPoint updates only when we complete Send() or Receive().
            //if (localEndPoint.Address == IPAddress.Any || localEndPoint.Port == 0)
            //{
            //    if (Socket.LocalEndPoint is IPEndPoint updatedLocalEndPoint)
            //    {
            //        base.Start(updatedLocalEndPoint);
            //        return;
            //    }
            //}

            lock (_lock)
            {
                base.Start(localEndPoint);
                Socket!.Bind(localEndPoint);
                Source = new();
                StartReceiveAsync(Source.Token);
            }
        }

        /// <summary>
        /// Reads input data from clients routes it onwards.
        /// </summary>
        protected async void StartReceiveAsync(CancellationToken token = default)
        {

        }


        /// <inheritdoc/>
        public override void Stop()
        {
            lock (_lock)
            {
                if (Source is not null)
                {
                    Source.Cancel();
                    Source.Dispose();
                    Source = null;
                }

                Socket!.Shutdown(SocketShutdown.Both);
                base.Stop();
            }
        }

        /// <inheritdoc/>
        public override void Connect(IPEndPoint remoteEndPoint)
        {
            lock (_lock)
            {
                base.Connect(remoteEndPoint);
                Socket.Connect(remoteEndPoint);
            }
        }

        /// <inheritdoc/>
        public override void Disconnect()
        {
            lock (_lock)
            {
                Socket!.Disconnect(reuseSocket: true);
            }

            base.Disconnect();
        }

        /// <inheritdoc/>
        public override bool HasConnection(ConnectionID connection)
        {
            return true;
        }

        /// <inheritdoc/>
        public void SendUnreliable(ReadOnlySpan<byte> datagram)
        {
            Console.WriteLine($"{nameof(UDPTransport)}.{nameof(SendUnreliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            lock (_lock)
            {
                foreach (var client in Clients.Values)
                {
                    Socket.SendTo();
                }
                if (Socket!.Connected)
                {
                    Socket.Send(datagram);
                }
            }
        }

        /// <inheritdoc/>
        public void SendUnreliableExcluding(ReadOnlySpan<byte> datagram, ConnectionID toExclude)
        {
            Console.WriteLine($"{nameof(UDPTransport)}.{nameof(SendUnreliableExcluding)}(exclude: ({toExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            lock (_lock)
            {
                if (Socket!.Connected)
                {
                    Socket.Send(datagram);
                }
            }
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(ReadOnlySpan<byte> datagram, ConnectionID target)
        {
            Console.WriteLine($"{nameof(UDPTransport)}.{nameof(SendUnreliableTo)}(target: ({target}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
        }

        /// <inheritdoc/>
        public void HandleUnreliable(ReadOnlySpan<byte> datagram, ConnectionID source)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(HandleUnreliable)}(source: ({source}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
