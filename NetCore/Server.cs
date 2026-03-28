using Cysharp.Threading.Tasks;
using NetCore.Transports;
using System;
using System.Runtime.CompilerServices;
using System.Threading;

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
        /// Starts a server and binds all registered transports to a provided <see cref="StartupArgs.LocalIPEndPoint"/>.
        /// </summary>
        /// <inheritdoc/>
        protected override async UniTask<OperationResult> StartOperation(StartupArgs args, CancellationToken token)
        {
            OperationResult result = await base.StartOperation(args, token);
            if (result == OperationResult.Success)
                Servers.Add(this);

            return result;
        }

        /// <summary>
        /// Disconnects all the players, stops the server, and unbinds all transports.
        /// </summary>
        /// <inheritdoc/>
        protected override UniTask<OperationResult> StopOperation()
        {
            Servers.Remove(this);
            return base.StopOperation();
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               SendTo Messages
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        #region General sending methods
        /// <summary>
        /// Sends <paramref name="datagram"/> to the end-point, specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode in the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">Arguments specifying an end-point.</param>
        /// <param name="mode">Sending mode to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args, SendingMode mode)
        {
            switch (mode)
            {
                case SendingMode.Unreliable: SendUnreliableTo(ref header, datagram, ref flags, args); return;
                case SendingMode.Reliable: SendReliableTo(ref header, datagram, ref flags, args); return;
                case SendingMode.Sequential: SendSequentialTo(ref header, datagram, ref flags, args); return;
                case SendingMode.Resilient: SendResilientTo(ref header, datagram, ref flags, args); return;
                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// Sends <paramref name="datagram"/> to the end-point, specified with <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        /// This sending method requires target transport to define all transportation methods.
        /// To use transports with only specific transportation methods, please use specialized methods instead.
        /// </remarks>
        /// <param name="header">Header to encode in the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">Arguments specifying an end-point, outside of a current connection.</param>
        /// <param name="mode">Sending mode to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args, SendingMode mode)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            switch (mode)
            {
                case SendingMode.Unreliable: SendUnreliableTo<TTransport>(ref header, datagram, ref flags, args); return;
                case SendingMode.Reliable: SendReliableTo<TTransport>(ref header, datagram, ref flags, args); return;
                case SendingMode.Sequential: SendSequentialTo<TTransport>(ref header, datagram, ref flags, args); return;
                case SendingMode.Resilient: SendResilientTo<TTransport>(ref header, datagram, ref flags, args); return;
                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// If there are transports that can send the data:
        /// Sends <paramref name="datagram"/> to the end-point, specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode in the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">Arguments specifying an end-point.</param>
        /// <param name="mode">Sending mode to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args, SendingMode mode)
        {
            return mode switch
            {
                SendingMode.Unreliable => TrySendUnreliableTo(ref header, datagram, ref flags, args),
                SendingMode.Reliable => TrySendReliableTo(ref header, datagram, ref flags, args),
                SendingMode.Sequential => TrySendSequentialTo(ref header, datagram, ref flags, args),
                SendingMode.Resilient => TrySendResilientTo(ref header, datagram, ref flags, args),
                _ => throw new SwitchExpressionException(mode),
            };
        }

        /// <summary>
        /// If there are transports that can send the data:
        /// Sends <paramref name="datagram"/> to the end-point, specified with <paramref name="args"/>.
        /// </summary>
        /// <remarks>
        /// This sending method requires target transport to define all transportation methods.
        /// To use transports with only specific transportation methods, please use specialized methods instead.
        /// </remarks>
        /// <param name="header">Header to encode in the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">Arguments specifying an end-point, outside of a current connection.</param>
        /// <param name="mode">Sending mode to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args, SendingMode mode)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            return mode switch
            {
                SendingMode.Unreliable => TrySendUnreliableTo<TTransport>(ref header, datagram, ref flags, args),
                SendingMode.Reliable => TrySendReliableTo<TTransport>(ref header, datagram, ref flags, args),
                SendingMode.Sequential => TrySendSequentialTo<TTransport>(ref header, datagram, ref flags, args),
                SendingMode.Resilient => TrySendResilientTo<TTransport>(ref header, datagram, ref flags, args),
                _ => throw new SwitchExpressionException(mode),
            };
        }
        #endregion

        #region Narrow sending methods - Unreliable
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendUnreliableTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            using (header.Lock()) using (flags.Lock()) using (args.Lock())
            {
                lock (_lock)
                {
                    foreach (var transport in UnreliableTransports)
                    {
                        transport.SendUnreliableTo(header, datagram, flags, args);
                    }
                }
            }
        }
        /// <summary>
        /// Tries to unreliably send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendUnreliableTo"/>
        public bool TrySendUnreliableTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            lock (_lock)
            {
                if (UnreliableTransports.Count == 0)
                    return false;

                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    foreach (var transport in UnreliableTransports)
                    {
                        transport.SendUnreliableTo(header, datagram, flags, args);
                    }
                }

                return true;
            }
        }
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendUnreliableTo{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendUnreliableTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    GetUnreliableTransport<TTransport>().SendUnreliableTo(header, datagram, flags, args);
                }
            }
        }
        /// <summary>
        /// Tries to unreliably send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Returns <c>false</c> if there is no <typeparamref name="TTransport"/> registered.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendUnreliableTo{TTransport}"/>
        public bool TrySendUnreliableTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    if (UnreliableTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendUnreliableTo(header, datagram, flags, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion

        #region Narrow sending methods - Reliable
        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendReliableTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    foreach (var transport in ReliableTransports)
                    {
                        transport.SendReliableTo(header, datagram, flags, args);
                    }
                }
            }
        }
        /// <summary>
        /// Tries to reliably send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendReliableTo"/>
        public bool TrySendReliableTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                    return false;

                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    foreach (var transport in ReliableTransports)
                    {
                        transport.SendReliableTo(header, datagram, flags, args);
                    }
                }

                return true;
            }
        }
        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendReliableTo{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendReliableTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    GetReliableTransport<TTransport>().SendReliableTo(header, datagram, flags, args);
                }
            }
        }
        /// <summary>
        /// Tries to reliably send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Returns <c>false</c> if there is no <typeparamref name="TTransport"/> registered.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendReliableTo{TTransport}"/>
        public bool TrySendReliableTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    if (ReliableTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendReliableTo(header, datagram, flags, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion

        #region Narrow sending methods - Sequential
        /// <summary>
        /// Sequentially sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendSequentialTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    foreach (var transport in SequentialTransports)
                    {
                        transport.SendSequentialTo(header, datagram, flags, args);
                    }
                }
            }
        }
        /// <summary>
        /// Tries to sequentially send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendSequentialTo"/>
        public bool TrySendSequentialTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            lock (_lock)
            {
                if (SequentialTransports.Count == 0)
                    return false;

                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    foreach (var transport in SequentialTransports)
                    {
                        transport.SendSequentialTo(header, datagram, flags, args);
                    }
                }

                return true;
            }
        }
        /// <summary>
        /// Sequentially sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendSequentialTo{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendSequentialTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    GetSequentialTransport<TTransport>().SendSequentialTo(header, datagram, flags, args);
                }
            }
        }
        /// <summary>
        /// Tries to sequentially send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Returns <c>false</c> if there is no <typeparamref name="TTransport"/> registered.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendSequentialTo{TTransport}"/>
        public bool TrySendSequentialTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    if (SequentialTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendSequentialTo(header, datagram, flags, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion

        #region Narrow sending methods - Resilient
        /// <summary>
        /// Resiliently sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendResilientTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    foreach (var transport in ResilientTransports)
                    {
                        transport.SendResilientTo(header, datagram, flags, args);
                    }
                }
            }
        }
        /// <summary>
        /// Tries to resiliently send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendResilientTo"/>
        public bool TrySendResilientTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
        {
            lock (_lock)
            {
                if (ResilientTransports.Count == 0)
                    return false;

                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    foreach (var transport in ResilientTransports)
                    {
                        transport.SendResilientTo(header, datagram, flags, args);
                    }
                }

                return true;
            }
        }
        /// <summary>
        /// Resiliently sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendResilientTo{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendResilientTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    GetResilientTransport<TTransport>().SendResilientTo(header, datagram, flags, args);
                }
            }
        }
        /// <summary>
        /// Tries to resiliently send <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Returns <c>false</c> if there is no <typeparamref name="TTransport"/> registered.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendResilientTo{TTransport}"/>
        public bool TrySendResilientTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock()) using (args.Lock())
                {
                    if (ResilientTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendResilientTo(header, datagram, flags, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion


        #region Datagram Transporting
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to the server.
        /// </summary>
        /// <param name="header">Header to encode with the message.</param>
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
        /// <param name="header">Header to encode with the message.</param>
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
        /// <param name="header">Header to encode with the message.</param>
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
        /// <param name="header">Header to encode with the message.</param>
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
        /// <param name="header">Header to encode with the message.</param>
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
        /// <param name="header">Header to encode with the message.</param>
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
        /// <param name="header">Header to encode with the message.</param>
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
        /// <param name="header">Header to encode with the message.</param>
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
        /// <param name="header">Header to encode with the message.</param>
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
        /// <param name="header">Header to encode with the message.</param>
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
        /// <param name="header">Header to encode with the message.</param>
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
        /// <param name="header">Header to encode with the message.</param>
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
