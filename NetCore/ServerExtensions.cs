using Cysharp.Threading.Tasks;
using NetCore.Transports;
using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace NetCore
{
    /// <summary>
    /// Extensions for faster prototyping when working with <see cref="Server"/>.
    /// </summary>
    public static class ServerExtensions
    {
        /// <summary>
        /// Starts a server and binds all registered transports to a provided <paramref name="localPort"/> and IPv4 address <see cref="IPAddress.Any"/>.
        /// </summary>
        /// <param name="server">Server to provide an <see cref="IPEndPoint"/> to.</param>
        /// <param name="localPort">
        /// Port to bind all transports to. Transports that rely on UID (such as SteamNetworking) might use another port instead.
        /// </param>
        public static UniTask<OperationResult> Start(this Server server, ushort localPort)
        {
            return server.Start(args => args.LocalIPEndPoint = new IPEndPoint(IPAddress.Any, localPort));
        }

        /// <summary>
        /// Starts a server and binds all registered transports to a provided <paramref name="localAddress"/> and <paramref name="localPort"/>.
        /// </summary>
        /// <param name="server">Server to provide an <see cref="IPEndPoint"/> to.</param>
        /// <param name="localAddress">Local address to start a <paramref name="server"/> on.</param>
        /// <param name="localPort">
        /// Port to bind all transports to. Transports that rely on UID (such as SteamNetworking) might use another port instead.
        /// </param>
        public static UniTask<OperationResult> Start(this Server server, IPAddress localAddress, ushort localPort)
        {
            return server.Start(args => args.LocalIPEndPoint = new(localAddress, localPort));
        }

        /// <summary>
        /// Starts a server and binds all registered transports to a provided <paramref name="localEndPoint"/>.
        /// </summary>
        /// <param name="server">Server to provide an <see cref="IPEndPoint"/> to.</param>
        /// <param name="localEndPoint">Local end-point to bind all transports to.</param>
        public static UniTask<OperationResult> Start(this Server server, IPEndPoint localEndPoint)
        {
            return server.Start(args => args.LocalIPEndPoint = localEndPoint);
        }

        /// <summary>
        /// Connects <paramref name="server"/> to a provided <paramref name="remoteAddress"/> with <paramref name="remotePort"/>.
        /// </summary>
        /// <param name="server"><see cref="Server"/> to connect to remote end-point.</param>
        /// <param name="remoteAddress">Remote address to connect a <paramref name="server"/> to.</param>
        /// <param name="remotePort">Remote port to connect to.</param>
        public static UniTask<OperationResult> Connect(this Server server, IPAddress remoteAddress, ushort remotePort)
        {
            return server.Connect(args => args.RemoteIPEndPoint = new(remoteAddress, remotePort));
        }

        /// <summary>
        /// Connects <paramref name="server"/> to a provided <paramref name="remoteEndPoint"/>.
        /// </summary>
        /// <param name="server"><see cref="Server"/> to connect to remote end-point.</param>
        /// <param name="remoteEndPoint">Remote end-point to bind all transports to.</param>
        public static UniTask<OperationResult> Connect(this Server server, IPEndPoint remoteEndPoint)
        {
            return server.Connect(args => args.RemoteIPEndPoint = remoteEndPoint);
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Send Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// TODO: Add <see cref="SendingMode"/> methods.
        /// <inheritdoc cref="Server.SendUnreliable"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliable(this Server server, ReadOnlySpan<byte> datagram,
            HeaderConstructor? header = null, FlagsConstructor? flags = null)
        {
            Header h = Header.Get();
            header?.Invoke(ref h);
            Flags f = Flags.Get();
            flags?.Invoke(ref f);
            server.SendUnreliable(ref h, datagram, ref f);
        }

        /// <inheritdoc cref="Server.SendUnreliable{TTransport}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliable<TTransport>(this Server server, ReadOnlySpan<byte> datagram,
            HeaderConstructor? header = null, FlagsConstructor? flags = null)
            where TTransport : class, IUnreliableTransport
        {
            Header h = Header.Get();
            header?.Invoke(ref h);
            Flags f = Flags.Get();
            flags?.Invoke(ref f);
            server.SendUnreliable<TTransport>(ref h, datagram, ref f);
        }

        /// <inheritdoc cref="Server.SendUnreliableExcluding"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableExcluding(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection,
            HeaderConstructor? header = null, FlagsConstructor? flags = null)
        {
            Header h = Header.Get();
            header?.Invoke(ref h);
            Flags f = Flags.Get();
            flags?.Invoke(ref f);
            server.SendUnreliableExcluding(ref h, datagram, ref f, connection);
        }

        /// <inheritdoc cref="Server.SendUnreliableExcluding{TTransport}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableExcluding<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection,
            HeaderConstructor? header = null, FlagsConstructor? flags = null)
            where TTransport : class, IUnreliableTransport
        {
            Header h = Header.Get();
            header?.Invoke(ref h);
            Flags f = Flags.Get();
            flags?.Invoke(ref f);
            server.SendUnreliableExcluding<TTransport>(ref h, datagram, ref f, connection);
        }

        /// <inheritdoc cref="Server.SendUnreliableTo"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableTo(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection,
            HeaderConstructor? header = null, FlagsConstructor? flags = null)
        {
            Header h = Header.Get();
            header?.Invoke(ref h);
            Flags f = Flags.Get();
            flags?.Invoke(ref f);
            server.SendUnreliableTo(ref h, datagram, ref f, connection);
        }

        /// <inheritdoc cref="Server.SendUnreliableTo{TTransport}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableTo<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection,
            HeaderConstructor? header = null, FlagsConstructor? flags = null)
            where TTransport : class, IUnreliableTransport
        {
            Header h = Header.Get();
            header?.Invoke(ref h);
            Flags f = Flags.Get();
            flags?.Invoke(ref f);
            server.SendUnreliableTo<TTransport>(ref h, datagram, ref f, connection);
        }

        /// <inheritdoc cref="Server.SendReliable"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliable(this Server server, ReadOnlySpan<byte> datagram,
            HeaderConstructor? header = null, FlagsConstructor? flags = null)
        {
            Header h = Header.Get();
            header?.Invoke(ref h);
            Flags f = Flags.Get();
            flags?.Invoke(ref f);
            server.SendReliable(ref h, datagram, ref f);
        }

        /// <inheritdoc cref="Server.SendReliable{TTransport}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliable<TTransport>(this Server server, ReadOnlySpan<byte> datagram,
            HeaderConstructor? header = null, FlagsConstructor? flags = null)
            where TTransport : class, IReliableTransport
        {
            Header h = Header.Get();
            header?.Invoke(ref h);
            Flags f = Flags.Get();
            flags?.Invoke(ref f);
            server.SendReliable<TTransport>(ref h, datagram, ref f);
        }

        /// <inheritdoc cref="Server.SendReliableExcluding"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableExcluding(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection,
            HeaderConstructor? header = null, FlagsConstructor? flags = null)
        {
            Header h = Header.Get();
            header?.Invoke(ref h);
            Flags f = Flags.Get();
            flags?.Invoke(ref f);
            server.SendReliableExcluding(ref h, datagram, ref f, connection);
        }

        /// <inheritdoc cref="Server.SendReliableExcluding{TTransport}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableExcluding<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection,
            HeaderConstructor? header = null, FlagsConstructor? flags = null)
            where TTransport : class, IReliableTransport
        {
            Header h = Header.Get();
            header?.Invoke(ref h);
            Flags f = Flags.Get();
            flags?.Invoke(ref f);
            server.SendReliableExcluding<TTransport>(ref h, datagram, ref f, connection);
        }

        /// <inheritdoc cref="Server.SendReliableTo"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableTo(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection,
            HeaderConstructor? header = null, FlagsConstructor? flags = null)
        {
            Header h = Header.Get();
            header?.Invoke(ref h);
            Flags f = Flags.Get();
            flags?.Invoke(ref f);
            server.SendUnreliableExcluding(ref h, datagram, ref f, connection);
        }

        /// <inheritdoc cref="Server.SendReliableTo{TTransport}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableTo<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection,
            HeaderConstructor? header = null, FlagsConstructor? flags = null)
            where TTransport : class, IReliableTransport
        {
            Header h = Header.Get();
            header?.Invoke(ref h);
            Flags f = Flags.Get();
            flags?.Invoke(ref f);
            server.SendReliableTo<TTransport>(ref h, datagram, ref f, connection);
        }




        /// <inheritdoc cref="Server.SendUnreliable"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliable(this Server server, ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            server.SendUnreliable(ref header, datagram);
        }

        /// <inheritdoc cref="Server.SendUnreliable{TTransport}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliable<TTransport>(this Server server, ReadOnlySpan<byte> datagram)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            server.SendUnreliable<TTransport>(ref header, datagram);
        }

        /// <inheritdoc cref="Server.SendUnreliableExcluding"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableExcluding(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            Header header = Header.Get();
            server.SendUnreliableExcluding(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendUnreliableExcluding{TTransport}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableExcluding<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            server.SendUnreliableExcluding<TTransport>(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendUnreliableTo"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableTo(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            Header header = Header.Get();
            server.SendUnreliableTo(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendUnreliableTo{TTransport}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableTo<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            server.SendUnreliableTo<TTransport>(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendReliable"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliable(this Server server, ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            server.SendReliable(ref header, datagram);
        }

        /// <inheritdoc cref="Server.SendReliable{TTransport}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliable<TTransport>(this Server server, ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            server.SendReliable<TTransport>(ref header, datagram);
        }

        /// <inheritdoc cref="Server.SendReliableExcluding"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableExcluding(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            Header header = Header.Get();
            server.SendReliableExcluding(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendReliableExcluding{TTransport}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableExcluding<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            server.SendReliableExcluding<TTransport>(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendReliableTo"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableTo(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            Header header = Header.Get();
            server.SendUnreliableExcluding(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendReliableTo{TTransport}"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableTo<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            server.SendReliableTo<TTransport>(ref header, datagram, connection);
        }
    }
}
