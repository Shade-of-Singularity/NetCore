using System;
using System.Net;
using System.Runtime.Serialization;

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
        /// Unreliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="datagram">Datagram to send.</param>
        public void SendUnreliable<TTransport>(ReadOnlySpan<byte> datagram) where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                GetUnreliableTransport<TTransport>()?.SendUnreliable(datagram);
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a all connections, excluding specific <paramref name="connection"/>.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendUnreliableExcluding(ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            lock (_lock)
            {
                foreach (var transport in UnreliableTransports)
                {
                    if (transport.HasConnection(connection))
                    {
                        // Only run method with expensive checks if transport manages excluded connection.
                        transport.SendUnreliableExcluding(datagram, toExclude: connection);
                    }
                    else
                    {
                        transport.SendUnreliable(datagram);
                    }
                }
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a all connections, excluding specific <paramref name="connection"/>,
        /// using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendUnreliableExcluding<TTransport>(ReadOnlySpan<byte> datagram, ConnectionID connection) where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                GetUnreliableTransport<TTransport>()?.SendUnreliableExcluding(datagram, connection);
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a specific connection.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to send a <paramref name="datagram"/> to.</param>
        public void SendUnreliableTo(ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            lock (_lock)
            {
                foreach (var transport in UnreliableTransports)
                    transport.SendUnreliableTo(datagram, connection);
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a specific connection using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to send a <paramref name="datagram"/> to.</param>
        public void SendUnreliableTo<TTransport>(ReadOnlySpan<byte> datagram, ConnectionID connection) where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                GetUnreliableTransport<TTransport>()?.SendUnreliableTo(datagram, connection);
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
        /// Reliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="datagram">Datagram to send.</param>
        public void SendReliable<TTransport>(ReadOnlySpan<byte> datagram) where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                GetReliableTransport<TTransport>()?.SendReliable(datagram);
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a all connections, excluding specific <paramref name="connection"/>.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendReliableExcluding(ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            lock (_lock)
            {
                foreach (var transport in ReliableTransports)
                {
                    if (transport.HasConnection(connection))
                    {
                        // Only run method with expensive checks if transport manages excluded connection.
                        transport.SendReliableExcluding(datagram, toExclude: connection);
                    }
                    else
                    {
                        transport.SendReliable(datagram);
                    }
                }
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a all connections, excluding specific <paramref name="connection"/>,
        /// using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendReliableExcluding<TTransport>(ReadOnlySpan<byte> datagram, ConnectionID connection) where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                GetReliableTransport<TTransport>()?.SendReliableExcluding(datagram, connection);
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a specific connection.
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to send a <paramref name="datagram"/> to.</param>
        public void SendReliableTo(ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            lock (_lock)
            {
                foreach (var transport in ReliableTransports)
                    transport.SendReliableTo(datagram, connection);
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a specific connection using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to send a <paramref name="datagram"/> to.</param>
        public void SendReliableTo<TTransport>(ReadOnlySpan<byte> datagram, ConnectionID connection) where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                GetReliableTransport<TTransport>()?.SendReliableTo(datagram, connection);
            }
        }
        #endregion
    }
}
