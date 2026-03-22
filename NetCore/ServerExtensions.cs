using Cysharp.Threading.Tasks;
using System.Net;

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
        public static UniTask<bool> Start(this Server server, ushort localPort)
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
        public static UniTask<bool> Start(this Server server, IPAddress localAddress, ushort localPort)
        {
            return server.Start(args => args.LocalIPEndPoint = new(localAddress, localPort));
        }

        /// <summary>
        /// Starts a server and binds all registered transports to a provided <paramref name="localEndPoint"/>.
        /// </summary>
        /// <param name="server">Server to provide an <see cref="IPEndPoint"/> to.</param>
        /// <param name="localEndPoint">Local end-point to bind all transports to.</param>
        public static UniTask<bool> Start(this Server server, IPEndPoint localEndPoint)
        {
            return server.Start(args => args.LocalIPEndPoint = localEndPoint);
        }

        /// <summary>
        /// Connects <paramref name="server"/> to a provided <paramref name="remoteAddress"/> with <paramref name="remotePort"/>.
        /// </summary>
        /// <param name="server"><see cref="Server"/> to connect to remote end-point.</param>
        /// <param name="remoteAddress">Remote address to connect a <paramref name="server"/> to.</param>
        /// <param name="remotePort">Remote port to connect to.</param>
        public static UniTask<bool> Connect(this Server server, IPAddress remoteAddress, ushort remotePort)
        {
            return server.Connect(args => args.RemoteIPEndPoint = new(remoteAddress, remotePort));
        }

        /// <summary>
        /// Connects <paramref name="server"/> to a provided <paramref name="remoteEndPoint"/>.
        /// </summary>
        /// <param name="server"><see cref="Server"/> to connect to remote end-point.</param>
        /// <param name="remoteEndPoint">Remote end-point to bind all transports to.</param>
        public static UniTask<bool> Connect(this Server server, IPEndPoint remoteEndPoint)
        {
            return server.Connect(args => args.RemoteIPEndPoint = remoteEndPoint);
        }
    }
}
