using NetCore.Transports;
using System;
using System.Net;

namespace NetCore
{
    /// <summary>
    /// Base class working with different <see cref="ITransport"/>s.
    /// </summary>
    /// <inheritdoc cref="NetworkMember"/>
    /// TODO: Consider adding a check for 0 transports being present.
    public class Server(int transports) : NetworkMember<Server>(transports)
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Default parameter-less .ctor.
        /// Pre-allocates some space for transports with <see cref="NetworkMember.DefaultInitialTransportCapacity"/>.
        /// </summary>
        public Server() : this(DefaultInitialTransportCapacity) { }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Starts a server and binds all registered transports to a provided <paramref name="localEndPoint"/> <see cref="IPEndPoint"/>.
        /// </summary>
        /// <inheritdoc/>
        protected override bool Start()
        {
            if (base.Start())
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
        public virtual void SendUnreliable(ref Header header, ReadOnlySpan<byte> datagram)
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    foreach (var transport in UnreliableTransports)
                    {
                        transport.SendUnreliable(header, datagram);
                    }
                }
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendUnreliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IUnreliableTransport
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    GetUnreliableTransport<TTransport>()?.SendUnreliable(header, datagram);
                }
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a all connections, excluding specific <paramref name="connection"/>.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public virtual void SendUnreliableExcluding(ref Header header, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            using (header.Lock())
            {
                lock (_lock)
                {
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
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a all connections, excluding specific <paramref name="connection"/>,
        /// using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public virtual void SendUnreliableExcluding<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ConnectionID connection)
            where TTransport : class, IUnreliableTransport
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    GetUnreliableTransport<TTransport>()?.SendUnreliableExcluding(header, datagram, connection);
                }
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a specific connection.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to send a <paramref name="datagram"/> to.</param>
        public virtual void SendUnreliableTo(ref Header header, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    foreach (var transport in UnreliableTransports)
                    {
                        transport.SendUnreliableTo(header, datagram, connection);
                    }
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
        public virtual void SendUnreliableTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ConnectionID connection)
            where TTransport : class, IUnreliableTransport
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    GetUnreliableTransport<TTransport>()?.SendUnreliableTo(header, datagram, connection);
                }
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendReliable(ref Header header, ReadOnlySpan<byte> datagram)
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    foreach (var transport in ReliableTransports)
                    {
                        transport.SendReliable(header, datagram);
                    }
                }
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendReliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    GetReliableTransport<TTransport>()?.SendReliable(header, datagram);
                }
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a all connections, excluding specific <paramref name="connection"/>.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public virtual void SendReliableExcluding(ref Header header, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            using (header.Lock())
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
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a all connections, excluding specific <paramref name="connection"/>,
        /// using specified <typeparamref name="TTransport"/> (if it exist).
        /// </summary>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public virtual void SendReliableExcluding<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ConnectionID connection)
            where TTransport : class, IReliableTransport
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    GetReliableTransport<TTransport>()?.SendReliableExcluding(header, datagram, connection);
                }
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a specific connection.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="connection">Connection to send a <paramref name="datagram"/> to.</param>
        public virtual void SendReliableTo(ref Header header, ReadOnlySpan<byte> datagram, ConnectionID connection)
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    foreach (var transport in ReliableTransports)
                    {
                        transport.SendReliableTo(header, datagram, connection);
                    }
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
        public virtual void SendReliableTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ConnectionID connection)
            where TTransport : class, IReliableTransport
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    GetReliableTransport<TTransport>()?.SendReliableTo(header, datagram, connection);
                }
            }
        }
        #endregion
    }
}
