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
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        public void SendUnreliable(HeaderReader header, ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                foreach (var transport in UnreliableTransports)
                {
                    transport.SendUnreliable(header, datagram);
                }
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        public void SendUnreliable<TTransport>(HeaderReader header, ReadOnlySpan<byte> datagram) where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                GetUnreliableTransport<TTransport>()?.SendUnreliable(header, datagram);
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a all connections, excluding specific <paramref name="connection"/>.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendUnreliableExcluding(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                foreach (var transport in UnreliableTransports)
                {
                    if (transport.HasConnection(connection))
                    {
                        // Only run method with expensive checks if transport manages excluded connection.
                        transport.SendUnreliableExcluding(header, datagram, toExclude: connection);
                    }
                    else
                    {
                        transport.SendUnreliable(header, datagram);
                    }
                }
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a all connections, excluding specific <paramref name="connection"/>,
        /// using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendUnreliableExcluding<TTransport>(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID connection) where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                GetUnreliableTransport<TTransport>()?.SendUnreliableExcluding(header, datagram, connection);
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a specific connection.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to send a <paramref name="datagram"/> to.</param>
        public void SendUnreliableTo(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                foreach (var transport in UnreliableTransports)
                {
                    transport.SendUnreliableTo(header, datagram, connection);
                }
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a specific connection using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to send a <paramref name="datagram"/> to.</param>
        public void SendUnreliableTo<TTransport>(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID connection) where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                GetUnreliableTransport<TTransport>()?.SendUnreliableTo(header, datagram, connection);
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        public void SendReliable(HeaderReader header, ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                foreach (var transport in ReliableTransports)
                {
                    transport.SendReliable(header, datagram);
                }
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        public void SendReliable<TTransport>(HeaderReader header, ReadOnlySpan<byte> datagram) where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                GetReliableTransport<TTransport>()?.SendReliable(header, datagram);
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a all connections, excluding specific <paramref name="connection"/>.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendReliableExcluding(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            lock (_lock)
            {
                foreach (var transport in ReliableTransports)
                {
                    if (transport.HasConnection(connection))
                    {
                        // Only run method with expensive checks if transport manages excluded connection.
                        transport.SendReliableExcluding(header, datagram, toExclude: connection);
                    }
                    else
                    {
                        transport.SendReliable(header, datagram);
                    }
                }
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a all connections, excluding specific <paramref name="connection"/>,
        /// using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public void SendReliableExcluding<TTransport>(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID connection) where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                GetReliableTransport<TTransport>()?.SendReliableExcluding(header, datagram, connection);
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a specific connection.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to send a <paramref name="datagram"/> to.</param>
        public void SendReliableTo(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                foreach (var transport in ReliableTransports)
                {
                    transport.SendReliableTo(header, datagram, connection);
                }
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a specific connection using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to send a <paramref name="datagram"/> to.</param>
        public void SendReliableTo<TTransport>(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID connection) where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                // TODO: Consider adding a check for 0 transports being present.
                GetReliableTransport<TTransport>()?.SendReliableTo(header, datagram, connection);
            }
        }
        #endregion
    }
}
