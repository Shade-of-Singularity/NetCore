using NetCore.Transports;
using System;
using System.Runtime.CompilerServices;

namespace NetCore
{
    /// <summary>
    /// Base class working with different <see cref="ITransport"/>s.
    /// </summary>
    /// <inheritdoc cref="NetworkMember"/>
    /// TODO: Consider adding a check for 0 transports being present.
    public class Client(int transports) : NetworkMember<Client>(transports)
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
        public Client() : this(DefaultInitialTransportCapacity) { }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        #region General sending methods
        /// <summary>
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// </summary>
        /// <seealso cref="SendReliable(ref Header, ReadOnlySpan{byte})"/>
        /// <seealso cref="SendUnreliable(ref Header, ReadOnlySpan{byte})"/>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(ref Header header, ReadOnlySpan<byte> datagram, SendingMode mode)
        {
            switch (mode)
            {
                case SendingMode.Unreliable: SendUnreliable(ref header, datagram); return;
                case SendingMode.Reliable: SendReliable(ref header, datagram); return;
                case SendingMode.Sequential: SendSequential(ref header, datagram); return;
                case SendingMode.Resilient: SendResilient(ref header, datagram); return;
                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// Throws if no suitable transports were found.
        /// </summary>
        /// <remarks>
        /// This sending method requires target transport to define all transportation methods.
        /// To use transports with only specific transportation methods, please use specialized methods instead.
        /// </remarks>
        /// <seealso cref="SendReliable(ref Header, ReadOnlySpan{byte})"/>
        /// <seealso cref="SendUnreliable(ref Header, ReadOnlySpan{byte})"/>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, SendingMode mode)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            switch (mode)
            {
                case SendingMode.Unreliable: SendUnreliable<TTransport>(ref header, datagram); return;
                case SendingMode.Reliable: SendReliable<TTransport>(ref header, datagram); return;
                case SendingMode.Sequential: SendSequential<TTransport>(ref header, datagram); return;
                case SendingMode.Resilient: SendResilient<TTransport>(ref header, datagram); return;
                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// Sends <paramref name="datagram"/> to the end-point, specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode in the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="args">Arguments specifying an end-point.</param>
        /// <param name="mode">Sending mode to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendTo(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args, SendingMode mode)
        {
            switch (mode)
            {
                case SendingMode.Unreliable: SendUnreliableTo(ref header, datagram, args); return;
                case SendingMode.Reliable: SendReliableTo(ref header, datagram, args); return;
                case SendingMode.Sequential: SendSequentialTo(ref header, datagram, args); return;
                case SendingMode.Resilient: SendResilientTo(ref header, datagram, args); return;
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
        /// <param name="args">Arguments specifying an end-point, outside of a current connection.</param>
        /// <param name="mode">Sending mode to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args, SendingMode mode)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            switch (mode)
            {
                case SendingMode.Unreliable: SendUnreliableTo<TTransport>(ref header, datagram, args); return;
                case SendingMode.Reliable: SendReliableTo<TTransport>(ref header, datagram, args); return;
                case SendingMode.Sequential: SendSequentialTo<TTransport>(ref header, datagram, args); return;
                case SendingMode.Resilient: SendResilientTo<TTransport>(ref header, datagram, args); return;
                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// If there are transports that can send the data:
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// </summary>
        /// <seealso cref="SendReliable(ref Header, ReadOnlySpan{byte})"/>
        /// <seealso cref="SendUnreliable(ref Header, ReadOnlySpan{byte})"/>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend(ref Header header, ReadOnlySpan<byte> datagram, SendingMode mode)
        {
            return mode switch
            {
                SendingMode.Unreliable => TrySendUnreliable(ref header, datagram),
                SendingMode.Reliable => TrySendReliable(ref header, datagram),
                SendingMode.Sequential => TrySendSequential(ref header, datagram),
                SendingMode.Resilient => TrySendResilient(ref header, datagram),
                _ => throw new SwitchExpressionException(mode),
            };
        }

        /// <summary>
        /// If there are transports that can send the data:
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// </summary>
        /// <seealso cref="SendReliable(ref Header, ReadOnlySpan{byte})"/>
        /// <seealso cref="SendUnreliable(ref Header, ReadOnlySpan{byte})"/>
        /// <typeparam name="TTransport"><see cref="ITransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, SendingMode mode)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            return mode switch
            {
                SendingMode.Unreliable => TrySendUnreliable<TTransport>(ref header, datagram),
                SendingMode.Reliable => TrySendReliable<TTransport>(ref header, datagram),
                SendingMode.Sequential => TrySendSequential<TTransport>(ref header, datagram),
                SendingMode.Resilient => TrySendResilient<TTransport>(ref header, datagram),
                _ => throw new SwitchExpressionException(mode),
            };
        }

        /// <summary>
        /// If there are transports that can send the data:
        /// Sends <paramref name="datagram"/> to the end-point, specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode in the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="args">Arguments specifying an end-point.</param>
        /// <param name="mode">Sending mode to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendTo(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args, SendingMode mode)
        {
            return mode switch
            {
                SendingMode.Unreliable => TrySendUnreliableTo(ref header, datagram, args),
                SendingMode.Reliable => TrySendReliableTo(ref header, datagram, args),
                SendingMode.Sequential => TrySendSequentialTo(ref header, datagram, args),
                SendingMode.Resilient => TrySendResilientTo(ref header, datagram, args),
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
        /// <param name="args">Arguments specifying an end-point, outside of a current connection.</param>
        /// <param name="mode">Sending mode to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args, SendingMode mode)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            return mode switch
            {
                SendingMode.Unreliable => TrySendUnreliableTo<TTransport>(ref header, datagram, args),
                SendingMode.Reliable => TrySendReliableTo<TTransport>(ref header, datagram, args),
                SendingMode.Sequential => TrySendSequentialTo<TTransport>(ref header, datagram, args),
                SendingMode.Resilient => TrySendResilientTo<TTransport>(ref header, datagram, args),
                _ => throw new SwitchExpressionException(mode),
            };
        }
        #endregion

        #region Narrow sending methods - Unreliable
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
        /// Tries to unreliably send <paramref name="datagram"/> to the server.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendUnreliable"/>
        public bool TrySendUnreliable(ref Header header, ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (UnreliableTransports.Count == 0)
                    return false;

                SendUnreliable(ref header, datagram);
                return true;
            }
        }
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendUnreliableTo(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
        {
            using (header.Lock()) using (args.Lock())
            {
                lock (_lock)
                {
                    foreach (var transport in UnreliableTransports)
                    {
                        transport.SendUnreliableTo(header, datagram, args);
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
        public bool TrySendUnreliableTo(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
        {
            lock (_lock)
            {
                if (UnreliableTransports.Count == 0)
                    return false;

                SendUnreliableTo(ref header, datagram, args);
                return true;
            }
        }
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendUnreliable{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
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
                    GetUnreliableTransport<TTransport>().SendUnreliable(header, datagram);
                }
            }
        }
        /// <summary>
        /// Tries to unreliably send <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Returns <c>false</c> if there is no <typeparamref name="TTransport"/> registered.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendUnreliable{TTransport}"/>
        public bool TrySendUnreliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock())
                {
                    if (UnreliableTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendUnreliable(header, datagram);
                        return true;
                    }

                    return false;
                }
            }
        }
        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendUnreliable{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendUnreliableTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
            where TTransport : class, IUnreliableTransport
        {
            using (header.Lock()) using (args.Lock())
            {
                lock (_lock)
                {
                    GetUnreliableTransport<TTransport>().SendUnreliableTo(header, datagram, args);
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
        public bool TrySendUnreliableTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (args.Lock())
                {
                    if (UnreliableTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendUnreliableTo(header, datagram, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion

        #region Narrow sending methods - Reliable
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
        /// Tries to reliably send <paramref name="datagram"/> to the server.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendReliable"/>
        public bool TrySendReliable(ref Header header, ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                    return false;

                SendReliable(ref header, datagram);
                return true;
            }
        }
        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendReliableTo(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
        {
            using (header.Lock()) using (args.Lock())
            {
                lock (_lock)
                {
                    foreach (var transport in ReliableTransports)
                    {
                        transport.SendReliableTo(header, datagram, args);
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
        public bool TrySendReliableTo(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                    return false;

                SendReliableTo(ref header, datagram, args);
                return true;
            }
        }
        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendReliable{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
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
                    GetReliableTransport<TTransport>().SendReliable(header, datagram);
                }
            }
        }
        /// <summary>
        /// Tries to reliably send <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Returns <c>false</c> if there is no <typeparamref name="TTransport"/> registered.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendReliable{TTransport}"/>
        public bool TrySendReliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock())
                {
                    if (ReliableTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendReliable(header, datagram);
                        return true;
                    }

                    return false;
                }
            }
        }
        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendReliable{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendReliableTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
            where TTransport : class, IReliableTransport
        {
            using (header.Lock()) using (args.Lock())
            {
                lock (_lock)
                {
                    GetReliableTransport<TTransport>().SendReliableTo(header, datagram, args);
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
        public bool TrySendReliableTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (args.Lock())
                {
                    if (ReliableTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendReliableTo(header, datagram, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion

        #region Narrow sending methods - Sequential
        /// <summary>
        /// Sequentially sends <paramref name="datagram"/> to the server.
        /// </summary>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendSequential(ref Header header, ReadOnlySpan<byte> datagram)
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    foreach (var transport in SequentialTransports)
                    {
                        transport.SendSequential(header, datagram);
                    }
                }
            }
        }
        /// <summary>
        /// Tries to sequentially send <paramref name="datagram"/> to the server.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendSequential"/>
        public bool TrySendSequential(ref Header header, ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                    return false;

                SendReliable(ref header, datagram);
                return true;
            }
        }
        /// <summary>
        /// Sequentially sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendSequentialTo(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
        {
            using (header.Lock()) using (args.Lock())
            {
                lock (_lock)
                {
                    foreach (var transport in SequentialTransports)
                    {
                        transport.SendSequentialTo(header, datagram, args);
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
        public bool TrySendSequentialTo(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
        {
            lock (_lock)
            {
                if (SequentialTransports.Count == 0)
                    return false;

                SendSequentialTo(ref header, datagram, args);
                return true;
            }
        }
        /// <summary>
        /// Sequentially sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendSequential{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendSequential<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, ISequentialTransport
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    GetSequentialTransport<TTransport>().SendSequential(header, datagram);
                }
            }
        }
        /// <summary>
        /// Tries to sequentially send <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Returns <c>false</c> if there is no <typeparamref name="TTransport"/> registered.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendSequential{TTransport}"/>
        public bool TrySendSequential<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock())
                {
                    if (SequentialTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendSequential(header, datagram);
                        return true;
                    }

                    return false;
                }
            }
        }
        /// <summary>
        /// Sequentially sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendSequential{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendSequentialTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
            where TTransport : class, ISequentialTransport
        {
            using (header.Lock()) using (args.Lock())
            {
                lock (_lock)
                {
                    GetSequentialTransport<TTransport>().SendSequentialTo(header, datagram, args);
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
        public bool TrySendSequentialTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (args.Lock())
                {
                    if (SequentialTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendSequentialTo(header, datagram, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion

        #region Narrow sending methods - Resilient
        /// <summary>
        /// Resiliently sends <paramref name="datagram"/> to the server.
        /// </summary>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendResilient(ref Header header, ReadOnlySpan<byte> datagram)
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    foreach (var transport in ResilientTransports)
                    {
                        transport.SendResilient(header, datagram);
                    }
                }
            }
        }
        /// <summary>
        /// Tries to resiliently send <paramref name="datagram"/> to the server.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendResilient"/>
        public bool TrySendResilient(ref Header header, ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (ResilientTransports.Count == 0)
                    return false;

                SendResilient(ref header, datagram);
                return true;
            }
        }
        /// <summary>
        /// Resiliently sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendResilientTo(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
        {
            using (header.Lock()) using (args.Lock())
            {
                lock (_lock)
                {
                    foreach (var transport in ResilientTransports)
                    {
                        transport.SendResilientTo(header, datagram, args);
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
        public bool TrySendResilientTo(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
        {
            lock (_lock)
            {
                if (ResilientTransports.Count == 0)
                    return false;

                SendResilientTo(ref header, datagram, args);
                return true;
            }
        }
        /// <summary>
        /// Resiliently sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendResilient{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        public virtual void SendResilient<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IResilientTransport
        {
            using (header.Lock())
            {
                lock (_lock)
                {
                    GetResilientTransport<TTransport>().SendResilient(header, datagram);
                }
            }
        }
        /// <summary>
        /// Tries to resiliently send <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Returns <c>false</c> if there is no <typeparamref name="TTransport"/> registered.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendResilient{TTransport}"/>
        public bool TrySendResilient<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock())
                {
                    if (ResilientTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendResilient(header, datagram);
                        return true;
                    }

                    return false;
                }
            }
        }
        /// <summary>
        /// Resiliently sends <paramref name="datagram"/> to a custom end-point specified with <paramref name="args"/>,
        /// using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendResilient{TTransport}"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="args">End-point args to use.</param>
        public virtual void SendResilientTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
            where TTransport : class, IResilientTransport
        {
            using (header.Lock()) using (args.Lock())
            {
                lock (_lock)
                {
                    GetResilientTransport<TTransport>().SendResilientTo(header, datagram, args);
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
        public bool TrySendResilientTo<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (args.Lock())
                {
                    if (ResilientTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendResilientTo(header, datagram, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion
    }
}