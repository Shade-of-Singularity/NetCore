using NetCore.Transports.Loopback;
using NetCore.Transports.TCP;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace NetCore.Transports.UDP
{
    /// <summary>
    /// Transport for UDP messages.
    /// </summary>
    /// <remarks>
    /// Does not implement networking functionality at the moment.
    /// </remarks>
    public partial class UDPTransport : Transport, IUnreliableTransport
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                 Constants
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// How large a UDP message should be for it to get fragmented.
        /// </summary>
        /// <remarks>
        /// Only used with <see cref="FragmentPackets"/> enabled.
        /// </remarks>
        public const int FragmentationThreshold = 1225;




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

        /// <summary>
        /// Whether or not to support packet fragmentation.
        /// When message becomes too large.
        /// </summary>
        public virtual bool FragmentPackets
        {
            get => throw new NotSupportedException($"Packet fragmentation is not supported as of yet.");
            set => throw new NotSupportedException($"Packet fragmentation is not supported as of yet.");
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Protected Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Indicated a remote end-point of the client.
        /// </summary>
        protected static readonly IPEndPoint RemoteAnyIPv4 = new(IPAddress.Any, 0);
        /// <summary>
        /// Lock, used when interacting with everything - <see cref="Socket"/>, <see cref="Source"/>, <see cref="Buffer"/>, <see cref="Clients"/>, etc.
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
        protected Dictionary<ClientID, ClientData> Clients = [];
        /// <summary>
        /// Byte buffer used for sending and retrieving data (since .NET Standard 2.1 doesn't support spans with one Socket connection)
        /// </summary>
        protected byte[] Buffer = new byte[Settings.MaxUnreliablePacketSize]; // Note: Maybe use a buffer from shared array pool?
        /// <summary>
        /// <see cref="TCP.TCPTransport"/> to pair this <see cref="UDPTransport"/> with.
        /// </summary>
        private TCPTransport? m_TCPTransport;




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
                Socket = new(SocketType, SocketProtocol)
                {
                    EnableBroadcast = true
                };
            }
        }

        /// <inheritdoc/>
        public override void Terminate(NetworkMember member)
        {
            lock (_lock)
            {
                Socket!.Dispose();
                Socket = null;
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
            if (localEndPoint.Address.Equals(IPAddress.Loopback) && Holder!.HasTransport<LoopbackTransport>())
            {
                return; // Loopbacks to itself are handled by a special class in NetCore.
            }

            lock (_lock)
            {
                base.Start(localEndPoint);
                Socket!.Bind(localEndPoint);
                Source = new();
                SocketAsyncEventArgs args = new()
                {
                    RemoteEndPoint = RemoteAnyIPv4
                };
                args.SetBuffer(Buffer);
                args.Completed += ReceiveRawData;
                Socket.ReceiveFromAsync(args);
            }
        }

        /// <summary>
        /// Method for handling incoming UDP data.
        /// </summary>
        protected virtual void ReceiveRawData(object socket, SocketAsyncEventArgs args)
        {
            lock (_lock)
            {
                //int stored = 0;
                //ClientData? sender = null;
                //do
                //{
                //    if (args.SocketError != SocketError.Success)
                //        return;

                //    if (args.RemoteEndPoint is not IPEndPoint ip)
                //        throw new NotSupportedException($"Non-IP end-points are not supported. Provided type: {args.RemoteEndPoint}");

                //    if (!Clients.TryGetValue(new ClientID(ip), out ClientData client))
                //        continue;

                //    if (sender is null || sender == client)
                //    {
                //        sender = client;
                //        int length = args.BytesTransferred;
                //        var span = args.Buffer.AsSpan(0, length);
                //        ResizeIfNeeded(ref Buffer, target: stored + length, BufferSizeIncrement);
                //        span.CopyTo(Buffer.AsSpan(stored, length));
                //        stored += length;
                //    }
                //    else // Sender changed between reads.
                //    {
                //        // Previously written data should be handled and reading should reset.
                //        HandleUnreliable(args.Buffer.AsSpan(0, stored), sender.ConnectionID);
                //        stored = 0;
                //    }
                //}
                //while (Socket is not null && !Socket.ReceiveFromAsync(args));
                //if (sender is not null)
                //{
                //    HandleUnreliable(Buffer.AsSpan(0, stored), sender.ConnectionID);
                //}
            }
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
                Socket!.Connect(remoteEndPoint);
            }
        }

        /// <inheritdoc/>
        public override void Disconnect()
        {
            lock (_lock)
            {
                try
                {
                    Socket!.Disconnect(reuseSocket: true);
                }
                catch (SocketException ex) when (ex.ErrorCode == 10057) { } // Attempted to disconnect a non-connected socket.
            }

            base.Disconnect();
        }

        /// <inheritdoc/>
        public override bool HasConnection(ConnectionID connection)
        {
            return true;
        }

        /// <inheritdoc/>
        public void SendUnreliable(Header header, ReadOnlySpan<byte> datagram)
        {
#if DEBUG
            Console.WriteLine($"{nameof(UDPTransport)}.{nameof(SendUnreliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
            lock (_lock)
            {
                if (!Socket!.Connected)
                {
                    return;
                }

                //ResizeIfNeeded(ref Buffer, datagram.Length, BufferSizeIncrement);
                //datagram.CopyTo(Buffer.AsSpan());
                //foreach (var client in Clients.Values)
                //{
                //    Socket.SendTo(Buffer, client.RemoteEndPoint);
                //}
            }
        }

        /// <inheritdoc/>
        public void SendUnreliableExcluding(Header header, ReadOnlySpan<byte> datagram, ConnectionID toExclude)
        {
#if DEBUG
            Console.WriteLine($"{nameof(UDPTransport)}.{nameof(SendUnreliableExcluding)}(exclude: ({toExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
            lock (_lock)
            {
                if (!Socket!.Connected)
                {
                    return;
                }

                //ResizeIfNeeded(ref Buffer, datagram.Length, BufferSizeIncrement);
                //datagram.CopyTo(Buffer.AsSpan());
                //foreach (var client in Clients.Values)
                //{
                //    if (client.ConnectionID != toExclude)
                //        Socket.SendTo(Buffer, client.RemoteEndPoint);
                //}
            }
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(Header header, ReadOnlySpan<byte> datagram, ConnectionID target)
        {
#if DEBUG
            Console.WriteLine($"{nameof(UDPTransport)}.{nameof(SendUnreliableTo)}(target: ({target}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
            lock (_lock)
            {
                if (!Socket!.Connected)
                {
                    return;
                }

                //if (Clients.TryGetValue(new ClientID(target), out ClientData client))
                //{
                //    ResizeIfNeeded(ref Buffer, datagram.Length, BufferSizeIncrement);
                //    datagram.CopyTo(Buffer.AsSpan());
                //    Socket.SendTo(Buffer, client.RemoteEndPoint);
                //}
            }
        }

        /// <inheritdoc/>
        public void HandleUnreliable(Header header, ReadOnlySpan<byte> datagram, ConnectionID source)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(UDPTransport)}.{nameof(HandleUnreliable)}(source: ({source}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;

            // TODO: Lock the _lock and route the data onwards.
            //  with ComputerysBitStream, try using a separate byte buffer for reading,
            //  lock specific segments of that array, and make sure that lock in a transport is released as soon as data is copied over.
        }
    }
}
