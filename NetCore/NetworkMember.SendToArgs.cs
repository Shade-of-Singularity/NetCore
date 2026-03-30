using NetCore.Transports;
using System;
using System.Runtime.CompilerServices;

namespace NetCore
{
    public abstract partial class NetworkMember
    {




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               SendTo Messages
        /// .                                A.K.A.: Good luck translating documentation.
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        #region General SendTo args methods
        /// <summary>
        /// Sends <paramref name="datagram"/> to the end-point, specified with <paramref name="args"/>.
        /// </summary>
        /// <param name="header">Header to encode in the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">Arguments specifying an end-point.</param>
        /// <param name="mode">Sending mode to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendTo(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, ConnectionArgs args, SendingMode mode)
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

        #region Narrow SendTo args methods - Unreliable
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
                        transport.SendUnreliableTo(datagram, header, flags, args);
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
                        transport.SendUnreliableTo(datagram, header, flags, args);
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
                    GetUnreliableTransport<TTransport>().SendUnreliableTo(datagram, header, flags, args);
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
                        transport.SendUnreliableTo(datagram, header, flags, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion

        #region Narrow SendTo args methods - Reliable
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
                        transport.SendReliableTo(datagram, header, flags, args);
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
                        transport.SendReliableTo(datagram, header, flags, args);
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
                    GetReliableTransport<TTransport>().SendReliableTo(datagram, header, flags, args);
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
                        transport.SendReliableTo(datagram, header, flags, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion

        #region Narrow SendTo args methods - Sequential
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
                        transport.SendSequentialTo(datagram, header, flags, args);
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
                        transport.SendSequentialTo(datagram, header, flags, args);
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
                    GetSequentialTransport<TTransport>().SendSequentialTo(datagram, header, flags, args);
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
                        transport.SendSequentialTo(datagram, header, flags, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion

        #region Narrow SendTo args methods - Resilient
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
                        transport.SendResilientTo(datagram, header, flags, args);
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
                        transport.SendResilientTo(datagram, header, flags, args);
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
                    GetResilientTransport<TTransport>().SendResilientTo(datagram, header, flags, args);
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
                        transport.SendResilientTo(datagram, header, flags, args);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion
    }
}
