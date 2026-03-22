using Cysharp.Threading.Tasks;
using NetCore.Common;
using NetCore.Transports;
using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace NetCore
{
    /// <summary>
    /// Extensions for faster prototyping when working with <see cref="Client"/>.
    /// </summary>
    public static class ClientExtensions
    {
        /// <summary>
        /// Starts <paramref name="client"/> on a <see cref="IPAddress.Any"/> with provided <paramref name="localPort"/>.
        /// </summary>
        /// <param name="client"><see cref="Client"/> to provide an <see cref="IPEndPoint"/> to.</param>
        /// <param name="localPort">
        /// Port to bind all transports to. Transports that rely on UID (such as SteamNetworking) might use another port instead.
        /// </param>
        public static UniTask<bool> Start(this Client client, ushort localPort)
        {
            return client.Start(args => args.LocalIPEndPoint = new IPEndPoint(IPAddress.Any, localPort));
        }

        /// <summary>
        /// Starts <paramref name="client"/> on a provided <paramref name="localAddress"/> with <paramref name="localPort"/>.
        /// </summary>
        /// <param name="client"><see cref="Client"/> to provide an <see cref="IPEndPoint"/> to.</param>
        /// <param name="localAddress">Local address to start a <paramref name="client"/> on.</param>
        /// <param name="localPort">
        /// Port to bind all transports to. Transports that rely on UID (such as SteamNetworking) might use another port instead.
        /// </param>
        public static UniTask<bool> Start(this Client client, IPAddress localAddress, ushort localPort)
        {
            return client.Start(args => args.LocalIPEndPoint = new(localAddress, localPort));
        }

        /// <summary>
        /// Starts <paramref name="client"/> and binds all registered transports to a provided <paramref name="localEndPoint"/>.
        /// </summary>
        /// <param name="client"><see cref="Client"/> to provide an <see cref="IPEndPoint"/> to.</param>
        /// <param name="localEndPoint">Local end-point to bind all transports to.</param>
        public static UniTask<bool> Start(this Client client, IPEndPoint localEndPoint)
        {
            return client.Start(args => args.LocalIPEndPoint = localEndPoint);
        }

        /// <summary>
        /// Connects <paramref name="client"/> to a provided <paramref name="remoteAddress"/> with <paramref name="remotePort"/>.
        /// </summary>
        /// <param name="client"><see cref="Client"/> to connect to remote end-point.</param>
        /// <param name="remoteAddress">Remote address to connect a <paramref name="client"/> to.</param>
        /// <param name="remotePort">Remote port to connect to.</param>
        public static UniTask<bool> Connect(this Client client, IPAddress remoteAddress, ushort remotePort)
        {
            return client.Connect(args => args.RemoteIPEndPoint = new(remoteAddress, remotePort));
        }

        /// <summary>
        /// Connects <paramref name="client"/> to a provided <paramref name="remoteEndPoint"/>.
        /// </summary>
        /// <param name="client"><see cref="Client"/> to connect to remote end-point.</param>
        /// <param name="remoteEndPoint">Remote end-point to bind all transports to.</param>
        public static UniTask<bool> Connect(this Client client, IPEndPoint remoteEndPoint)
        {
            return client.Connect(args => args.RemoteIPEndPoint = remoteEndPoint);
        }
    }
}
