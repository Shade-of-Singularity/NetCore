using System;
using System.Net;

namespace NetCore
{
    /// <summary>
    /// Base class working with different <see cref="ITransport"/>s.
    /// </summary>
    public class Server : NetworkMember
    {
        /// <summary>
        /// Starts a server and binds all registered transports to a provided <paramref name="localPort"/> and IPv4 address <see cref="IPAddress.Any"/>.
        /// </summary>
        /// <param name="localPort">Port to bind all transports to. Transports that rely on UID (such as SteamNetworking) might use another port instead.</param>
        public void Start(ushort localPort) => Start(new IPEndPoint(IPAddress.Any, localPort));

        /// <summary>
        /// Starts a server and binds all registered transports to a provided <paramref name="localAddress"/> and <paramref name="localPort"/>.
        /// </summary>
        /// <param name="localAddress">Address to bind all transports to.</param>
        /// <param name="localPort">Port to bind all transports to. Transports that rely on UID (such as SteamNetworking) might use another port instead.</param>
        public void Start(IPAddress localAddress, ushort localPort) => Start(new IPEndPoint(localAddress, localPort));

        /// <summary>
        /// Starts a server and binds all registered transports to a provided <paramref name="localEndPoint"/> <see cref="IPEndPoint"/>.
        /// </summary>
        /// <inheritdoc/>
        public override bool Start(IPEndPoint localEndPoint)
        {
            if (base.Start(localEndPoint))
            {
                Servers.Add(this);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Disconnects all the players, stops the server, and unbinds all transports.
        /// </summary>
        /// <inheritdoc/>
        public override void Stop()
        {
            Servers.Remove(this);
            base.Stop();
        }

        #region Datagram Transporting
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to the server.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        public void SendUnreliable(ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                foreach (var transport in UnreliableTransports)
                    transport.SendUnreliable(datagram);
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a all connections, excluding specific <paramref name="connection"/>.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendUnreliableExcluding(ReadOnlySpan<byte> datagram, uint connection)
        {
            lock (_lock)
            {
                foreach (var transport in UnreliableTransports)
                {
                    if (transport.HasCID(connection))
                    {
                        // Only run method with expensive checks if transport manages excluded connection.
                        transport.SendUnreliableExclusive(datagram, CIDToExclude: connection);
                    }
                    else
                    {
                        transport.SendUnreliable(datagram);
                    }
                }
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a specific connection.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to send a <paramref name="datagram"/> to.</param>
        public void SendUnreliableTo(ReadOnlySpan<byte> datagram, uint connection)
        {
            lock (_lock)
            {
                foreach (var transport in UnreliableTransports)
                    transport.SendUnreliableTo(datagram, connection);
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        public void SendReliable(ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                foreach (var transport in ReliableTransports)
                    transport.SendReliable(datagram);
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a all connections, excluding specific <paramref name="connection"/>.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendReliableExcluding(ReadOnlySpan<byte> datagram, uint connection)
        {
            lock (_lock)
            {
                foreach (var transport in ReliableTransports)
                {
                    if (transport.HasCID(connection))
                    {
                        // Only run method with expensive checks if transport manages excluded connection.
                        transport.SendReliableExclusive(datagram, CIDToExclude: connection);
                    }
                    else
                    {
                        transport.SendReliable(datagram);
                    }
                }
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a specific connection.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to send a <paramref name="datagram"/> to.</param>
        public void SendReliableTo(ReadOnlySpan<byte> datagram, uint connection)
        {
            lock (_lock)
            {
                foreach (var transport in ReliableTransports)
                    transport.SendReliableTo(datagram, connection);
            }
        }
        #endregion
    }
}
