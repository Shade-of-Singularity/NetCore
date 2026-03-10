using NetCore.Collections.Generic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;

namespace NetCore
{
    /// <summary>
    /// Describes servers currently running within current <see cref="AppDomain"/>.
    /// Needed mainly for <see cref="Loopback.LoopbackTransport"/>.
    /// </summary>
    public static class Servers
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private static readonly ConcurrentDictionary<ushort, Server> m_ServerMap = [];




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Attempts to add <paramref name="server"/> to global <see cref="Map"/>.
        /// </summary>
        /// <param name="server">Server to add.</param>
        /// <returns>
        /// <c>true</c> if server was added successfully.
        /// <c>false</c> if <paramref name="server"/> has <see cref="NetworkMember.LocalEndPoint"/> set to null,
        /// or another <see cref="Server"/> exist under the same port in the <see cref="Map"/>.
        /// </returns>
        public static bool TryAdd(Server server)
        {
            IPEndPoint? endPoint = server.LocalEndPoint;
            return endPoint is not null && m_ServerMap.TryAdd((ushort)endPoint.Port, server);
        }

        /// <summary>
        /// Adds <paramref name="server"/> to global <see cref="Map"/>.
        /// </summary>
        /// <param name="server">Server to add.</param>
        /// <exception cref="Exception"><see cref="Server"/> under the same port was already added to the <see cref="Map"/>.</exception>
        public static void Add(Server server)
        {
            IPEndPoint? endPoint = server.LocalEndPoint ?? throw new Exception("Cannot add a server with a null end-point!");
            if (!m_ServerMap.TryAdd((ushort)endPoint.Port, server))
            {
                throw new Exception("Server under the same port was already registered in a global server map!");
            }
        }

        /// <summary>
        /// Attempts to retrieve <see cref="Server"/> from an internal server map using a <paramref name="localEndPoint"/> (only port is used).
        /// </summary>
        /// <param name="localEndPoint">Local end-point of a server (only port will be used).</param>
        /// <param name="server">Retrieved server or <c>null</c> if not found.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="server"/> instance was found.
        /// <c>false</c> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGet(IPEndPoint localEndPoint, [NotNullWhen(true)] out Server? server)
        {
            return TryGet((ushort)localEndPoint.Port, out server);
        }

        /// <summary>
        /// Attempts to retrieve <see cref="Server"/> from an internal server map using a <paramref name="localPort"/>.
        /// </summary>
        /// <param name="localPort">Local port on which target server runs.</param>
        /// <param name="server">Retrieved <see cref="Server"/> instance or <c>null</c> if not found.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="server"/> instance was found.
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool TryGet(ushort localPort, [NotNullWhen(true)] out Server? server)
        {
            return m_ServerMap.TryGetValue(localPort, out server);
        }

        /// <summary>
        /// Retrieves <see cref="Server"/> from an internal server map using a <paramref name="localEndPoint"/> (only port is used).
        /// </summary>
        /// <param name="localEndPoint">Local end-point of a server (only port will be used).</param>
        /// <returns>Retrieved <see cref="Server"/> instance or <c>null</c> if not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Server? Get(IPEndPoint localEndPoint)
        {
            return Get((ushort)localEndPoint.Port);
        }

        /// <summary>
        /// Retrieves <see cref="Server"/> from an internal server map using a <paramref name="localPort"/>.
        /// </summary>
        /// <param name="localPort">Local port on which target server runs.</param>
        /// <returns>Retrieved <see cref="Server"/> instance or <c>null</c> if not found.</returns>
        public static Server? Get(ushort localPort)
        {
            return m_ServerMap.GetValueOrDefault(localPort);
        }

        /// <summary>
        /// Attempts to remove a <paramref name="server"/> from a global server <see cref="Map"/>.
        /// </summary>
        /// <param name="server">Server to remove from a global server <see cref="Map"/>.</param>
        /// <returns>
        /// <c>true</c> if server was removed successfully.
        /// <c>false</c> if <paramref name="server"/> has <see cref="NetworkMember.LocalEndPoint"/> set to null,
        /// or another <see cref="Server"/> exist under the same port in the <see cref="Map"/>.
        /// </returns>
        public static bool TryRemove(Server server)
        {
            IPEndPoint? endPoint = server.LocalEndPoint;
            return endPoint is not null && m_ServerMap.TryRemove((ushort)endPoint.Port, out Server _);
        }

        /// <summary>
        /// Removes a <paramref name="server"/> from a global server <see cref="Map"/>.
        /// </summary>
        /// <param name="server">Server to remove from a global server <see cref="Map"/>.</param>
        /// <exception cref="Exception"><paramref name="server"/> has <see cref="NetworkMember.LocalEndPoint"/> set to null.</exception>
        /// <returns>
        /// <c>true</c> if <paramref name="server"/> was removed successfully.
        /// <c>false</c> otherwise.
        /// </returns>
        public static bool Remove(Server server)
        {
            IPEndPoint? endPoint = server.LocalEndPoint ?? throw new Exception("Tried to remove a server with a null end-point!");
            return m_ServerMap.TryRemove((ushort)endPoint.Port, out Server _);
        }
    }
}
