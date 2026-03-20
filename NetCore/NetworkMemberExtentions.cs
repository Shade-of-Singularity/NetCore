using Cysharp.Threading.Tasks;
using NetCore.Transports;
using NetCore.Transports.Special;
using NetCore.Transports.TCP;
using NetCore.Transports.UDP;
using System;
using System.Runtime.CompilerServices;

namespace NetCore
{
    /// <summary>
    /// Extensions methods for working with <see cref="NetworkMember"/>s.
    /// </summary>
    public static class NetworkMemberExtensions
    {
        /// <summary>
        /// Registers specific <paramref name="transport"/> as both <see cref="IReliableTransport"/> and <see cref="IUnreliableTransport"/>
        /// in a target <see cref="NetworkMember"/> instance.
        /// </summary>
        /// <typeparam name="T">Specific type of <see cref="ITransport"/>.</typeparam>
        /// <param name="member">Member capable of working with <see cref="ITransport"/>s.</param>
        /// <param name="transport">Transport to register in a target <paramref name="member"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterTransportAsBoth<T>(this NetworkMember member, T transport) where T : class, IReliableTransport, IUnreliableTransport
        {
            member.RegisterReliableTransport(transport);
            member.RegisterUnreliableTransport(transport);
        }

        /// <summary>
        /// Checks if given <see cref="NetworkMember"/> (<paramref name="member"/>) managed transport of a type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Specific type of <see cref="ITransport"/>.</typeparam>
        /// <param name="member">Member capable of working with <see cref="ITransport"/>s.</param>
        /// <returns>
        /// <c>true</c> if transport was found.
        /// <c>false</c> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasTransport<T>(this NetworkMember member) where T : class, IReliableTransport, IUnreliableTransport
        {
            return member.HasReliableTransport<T>() || member.HasUnreliableTransport<T>();
        }

        /// <summary>
        /// Registers both <see cref="TCPTransport"/> and <see cref="UDPTransport"/>, and configures them to work together:
        /// <para><see cref="UDPTransport"/> - sends only unreliable messages.</para>
        /// <para><see cref="TCPTransport"/> - sends only reliable messages.</para>
        /// </summary>
        /// <param name="member">Member capable of working with <see cref="ITransport"/>s.</param>
        /// <param name="transport">Transport handing both reliable and unreliable messages.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RegisterTCPUDPTransport(this NetworkMember member, TCPUDPTransport transport)
        {
            member.RegisterReliableTransport(transport);
            member.RegisterUnreliableTransport(transport);
        }

        /// <summary>
        /// Instantiates and automatically registers given <typeparamref name="TTransport"/> in a given <see cref="NetworkMember"/>.
        /// </summary>
        /// <remarks>
        /// Use <see cref="NetworkMember.RegisterReliableTransport{T}(T)"/> directly if you want to assign transport to multiple transports.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to instantiate.</typeparam>
        /// <param name="member"><see cref="NetworkMember"/> </param>
        public static void RegisterReliableTransport<TTransport>(this NetworkMember member) where TTransport : class, IReliableTransport, new()
        {
            member.RegisterReliableTransport(new TTransport());
        }




        #region Client-side extensions
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                   Client
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc cref="Client.SendReliable(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliable(this Client client, ReadOnlySpan<byte> datagram, HeaderConstructor constructor)
        {
            Header header = Header.Get();
            constructor(ref header);
            client.SendReliable(ref header, datagram);
        }

        /// <inheritdoc cref="Client.SendReliable{TTransport}(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliable<TTransport>(this Client client, ReadOnlySpan<byte> datagram, HeaderConstructor constructor)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            constructor(ref header);
            client.SendReliable<TTransport>(ref header, datagram);
        }

        /// <inheritdoc cref="Client.SendUnreliable(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliable(this Client client, ReadOnlySpan<byte> datagram, HeaderConstructor constructor)
        {
            Header header = Header.Get();
            constructor(ref header);
            client.SendUnreliable(ref header, datagram);
        }

        /// <inheritdoc cref="Client.SendUnreliable{TTransport}(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliable<TTransport>(this Client client, ReadOnlySpan<byte> datagram, HeaderConstructor constructor)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            constructor(ref header);
            client.SendUnreliable<TTransport>(ref header, datagram);
        }




        /// <inheritdoc cref="Client.SendReliable(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliable(this Client client, ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            client.SendReliable(ref header, datagram);
        }

        /// <inheritdoc cref="Client.SendReliable{TTransport}(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliable<TTransport>(this Client client, ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            client.SendReliable<TTransport>(ref header, datagram);
        }

        /// <inheritdoc cref="Client.SendUnreliable(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliable(this Client client, ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            client.SendUnreliable(ref header, datagram);
        }

        /// <inheritdoc cref="Client.SendUnreliable{TTransport}(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliable<TTransport>(this Client client, ReadOnlySpan<byte> datagram)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            client.SendUnreliable<TTransport>(ref header, datagram);
        }
        #endregion

        #region Server-side extensions
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                   Server
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc cref="Server.SendUnreliable(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliable(this Server server, ReadOnlySpan<byte> datagram, HeaderConstructor constructor)
        {
            Header header = Header.Get();
            constructor(ref header);
            server.SendUnreliable(ref header, datagram);
        }

        /// <inheritdoc cref="Server.SendUnreliable{TTransport}(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliable<TTransport>(this Server server, ReadOnlySpan<byte> datagram, HeaderConstructor constructor)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            constructor(ref header);
            server.SendUnreliable<TTransport>(ref header, datagram);
        }

        /// <inheritdoc cref="Server.SendUnreliableExcluding(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableExcluding(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection, HeaderConstructor constructor)
        {
            Header header = Header.Get();
            constructor(ref header);
            server.SendUnreliableExcluding(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendUnreliableExcluding{TTransport}(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableExcluding<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection, HeaderConstructor constructor)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            constructor(ref header);
            server.SendUnreliableExcluding<TTransport>(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendUnreliableTo(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableTo(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection, HeaderConstructor constructor)
        {
            Header header = Header.Get();
            constructor(ref header);
            server.SendUnreliableTo(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendUnreliableTo{TTransport}(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableTo<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection, HeaderConstructor constructor)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            constructor(ref header);
            server.SendUnreliableTo<TTransport>(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendReliable(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliable(this Server server, ReadOnlySpan<byte> datagram, HeaderConstructor constructor)
        {
            Header header = Header.Get();
            constructor(ref header);
            server.SendReliable(ref header, datagram);
        }

        /// <inheritdoc cref="Server.SendReliable{TTransport}(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliable<TTransport>(this Server server, ReadOnlySpan<byte> datagram, HeaderConstructor constructor)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            constructor(ref header);
            server.SendReliable<TTransport>(ref header, datagram);
        }

        /// <inheritdoc cref="Server.SendReliableExcluding(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableExcluding(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection, HeaderConstructor constructor)
        {
            Header header = Header.Get();
            constructor(ref header);
            server.SendReliableExcluding(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendReliableExcluding{TTransport}(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableExcluding<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection, HeaderConstructor constructor)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            constructor(ref header);
            server.SendReliableExcluding<TTransport>(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendReliableTo(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableTo(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection, HeaderConstructor constructor)
        {
            Header header = Header.Get();
            constructor(ref header);
            server.SendUnreliableExcluding(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendReliableTo{TTransport}(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableTo<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection, HeaderConstructor constructor)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            constructor(ref header);
            server.SendReliableTo<TTransport>(ref header, datagram, connection);
        }




        /// <inheritdoc cref="Server.SendUnreliable(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliable(this Server server, ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            server.SendUnreliable(ref header, datagram);
        }

        /// <inheritdoc cref="Server.SendUnreliable{TTransport}(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliable<TTransport>(this Server server, ReadOnlySpan<byte> datagram)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            server.SendUnreliable<TTransport>(ref header, datagram);
        }

        /// <inheritdoc cref="Server.SendUnreliableExcluding(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableExcluding(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            Header header = Header.Get();
            server.SendUnreliableExcluding(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendUnreliableExcluding{TTransport}(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableExcluding<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            server.SendUnreliableExcluding<TTransport>(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendUnreliableTo(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableTo(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            Header header = Header.Get();
            server.SendUnreliableTo(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendUnreliableTo{TTransport}(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendUnreliableTo<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            server.SendUnreliableTo<TTransport>(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendReliable(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliable(this Server server, ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            server.SendReliable(ref header, datagram);
        }

        /// <inheritdoc cref="Server.SendReliable{TTransport}(ref Header, ReadOnlySpan{byte})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliable<TTransport>(this Server server, ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            server.SendReliable<TTransport>(ref header, datagram);
        }

        /// <inheritdoc cref="Server.SendReliableExcluding(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableExcluding(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            Header header = Header.Get();
            server.SendReliableExcluding(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendReliableExcluding{TTransport}(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableExcluding<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            server.SendReliableExcluding<TTransport>(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendReliableTo(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableTo(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            Header header = Header.Get();
            server.SendUnreliableExcluding(ref header, datagram, connection);
        }

        /// <inheritdoc cref="Server.SendReliableTo{TTransport}(ref Header, ReadOnlySpan{byte}, ConnectionID)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SendReliableTo<TTransport>(this Server server, ReadOnlySpan<byte> datagram, ConnectionID connection)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            server.SendReliableTo<TTransport>(ref header, datagram, connection);
        }
        #endregion
    }

    /// <summary>
    /// Constructor delegate providing essential <see cref="CustomHeaders"/> to the input <see cref="Header"/>.
    /// </summary>
    /// <param name="header">Header to modify.</param>
    public delegate void HeaderConstructor(ref Header header);
}
