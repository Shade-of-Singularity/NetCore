using NetCore.Transports.Loopback;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                _ = ListenForMessages(Socket, Source.Token); // TODO: Activate only if transport is used for messages (?)
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

        private async Task ListenForMessages(Socket socket, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var result = await socket.ReceiveFromAsync(Buffer, SocketFlags.None, RemoteAnyIPv4);
                    if (result.ReceivedBytes == 0)
                    {
                        continue; // Note: [CPU] To burn or not to burn?
                    }

                    if (result.RemoteEndPoint is not IPEndPoint ip)
                        continue;
                    //throw new NotSupportedException($"Non-IP end-points are not supported. Provided type: {result.RemoteEndPoint}");

                    if (!Clients.TryGetValue(new ClientID(ip), out ClientData client))
#if !DEBUG
                        continue;
#else
                    {
                        Clients[new ClientID(ip)] = client = new ClientData(Holder!.CIDProvider.NextCID(), ip);
                    }
#endif

                    try
                    {
                        HandleUnreliable(default, Buffer.AsSpan(0, result.ReceivedBytes), default);
                    }
#if DEBUG
                    catch (Exception ex) { Console.WriteLine($"{ex.Message}\n{ex.StackTrace}"); }
#else
                    catch { }
#endif
                }
            }
            catch (OperationCanceledException) { } // Graceful shutdown.
            catch (SocketException) { } // Socket closed.
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
            Console.WriteLine($"{nameof(UDPTransport)}.{nameof(SendUnreliable)}(datagram: {Encoding.UTF8.GetString(datagram)})");
#endif
            lock (_lock)
            {
                if (!Socket!.Connected)
                {
                    return;
                }

                datagram.CopyTo(Buffer.AsSpan());
#if DEBUG
                Socket.SendTo(Buffer, datagram.Length, SocketFlags.None, new IPEndPoint(IPAddress.Loopback, 27001));
#endif
                foreach (var client in Clients.Values)
                {
                    Socket.SendTo(Buffer, client.RemoteEndPoint);
                }
            }
        }

        /// <inheritdoc/>
        public void SendUnreliableExcluding(Header header, ReadOnlySpan<byte> datagram, ConnectionID toExclude)
        {
#if DEBUG
            Console.WriteLine($"{nameof(UDPTransport)}.{nameof(SendUnreliableExcluding)}(exclude: ({toExclude}) datagram: {Encoding.UTF8.GetString(datagram)})");
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
            Console.WriteLine($"{nameof(UDPTransport)}.{nameof(SendUnreliableTo)}(target: ({target}) datagram: {Encoding.UTF8.GetString(datagram)})");
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
            Console.WriteLine($"{nameof(UDPTransport)}.{nameof(HandleUnreliable)}(source: ({source}) datagram: {Encoding.UTF8.GetString(datagram)})");
            Console.ForegroundColor = ConsoleColor.White;

            // TODO: Lock the _lock and route the data onwards.
            //  with ComputerysBitStream, try using a separate byte buffer for reading,
            //  lock specific segments of that array, and make sure that lock in a transport is released as soon as data is copied over.
        }
    }
}
