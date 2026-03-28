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
        /// <seealso cref="SendReliable"/>
        /// <seealso cref="SendUnreliable"/>
        /// <seealso cref="SendSequential"/>
        /// <seealso cref="SendResilient"/>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, SendingMode mode)
        {
            switch (mode)
            {
                case SendingMode.Unreliable: SendUnreliable(ref header, datagram, ref flags); return;
                case SendingMode.Reliable: SendReliable(ref header, datagram, ref flags); return;
                case SendingMode.Sequential: SendSequential(ref header, datagram, ref flags); return;
                case SendingMode.Resilient: SendResilient(ref header, datagram, ref flags); return;
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
        /// <seealso cref="SendReliable"/>
        /// <seealso cref="SendUnreliable"/>
        /// <seealso cref="SendSequential"/>
        /// <seealso cref="SendResilient"/>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, SendingMode mode)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            switch (mode)
            {
                case SendingMode.Unreliable: SendUnreliable<TTransport>(ref header, datagram, ref flags); return;
                case SendingMode.Reliable: SendReliable<TTransport>(ref header, datagram, ref flags); return;
                case SendingMode.Sequential: SendSequential<TTransport>(ref header, datagram, ref flags); return;
                case SendingMode.Resilient: SendResilient<TTransport>(ref header, datagram, ref flags); return;
                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// If there are transports that can send the data:
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// </summary>
        /// <seealso cref="SendReliable"/>
        /// <seealso cref="SendUnreliable"/>
        /// <seealso cref="SendSequential"/>
        /// <seealso cref="SendResilient"/>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, SendingMode mode)
        {
            return mode switch
            {
                SendingMode.Unreliable => TrySendUnreliable(ref header, datagram, ref flags),
                SendingMode.Reliable => TrySendReliable(ref header, datagram, ref flags),
                SendingMode.Sequential => TrySendSequential(ref header, datagram, ref flags),
                SendingMode.Resilient => TrySendResilient(ref header, datagram, ref flags),
                _ => throw new SwitchExpressionException(mode),
            };
        }

        /// <summary>
        /// If there are transports that can send the data:
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// </summary>
        /// <seealso cref="SendReliable"/>
        /// <seealso cref="SendUnreliable"/>
        /// <seealso cref="SendSequential"/>
        /// <seealso cref="SendResilient"/>
        /// <typeparam name="TTransport"><see cref="ITransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags, SendingMode mode)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            return mode switch
            {
                SendingMode.Unreliable => TrySendUnreliable<TTransport>(ref header, datagram, ref flags),
                SendingMode.Reliable => TrySendReliable<TTransport>(ref header, datagram, ref flags),
                SendingMode.Sequential => TrySendSequential<TTransport>(ref header, datagram, ref flags),
                SendingMode.Resilient => TrySendResilient<TTransport>(ref header, datagram, ref flags),
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
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public virtual void SendUnreliable(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock())
                {
                    foreach (var transport in UnreliableTransports)
                    {
                        transport.SendUnreliable(header, datagram, flags);
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
        public bool TrySendUnreliable(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (UnreliableTransports.Count == 0)
                    return false;

                using (header.Lock()) using (flags.Lock())
                {
                    foreach (var transport in UnreliableTransports)
                    {
                        transport.SendUnreliable(header, datagram, flags);
                    }
                }

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
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public virtual void SendUnreliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock())
                {
                    GetUnreliableTransport<TTransport>().SendUnreliable(header, datagram, flags);
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
        public bool TrySendUnreliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock())
                {
                    if (UnreliableTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendUnreliable(header, datagram, flags);
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
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public virtual void SendReliable(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock())
                {
                    foreach (var transport in ReliableTransports)
                    {
                        transport.SendReliable(header, datagram, flags);
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
        public bool TrySendReliable(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                    return false;

                using (header.Lock()) using (flags.Lock())
                {
                    foreach (var transport in ReliableTransports)
                    {
                        transport.SendReliable(header, datagram, flags);
                    }
                }

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
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public virtual void SendReliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock())
                {
                    GetReliableTransport<TTransport>().SendReliable(header, datagram, flags);
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
        public bool TrySendReliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock())
                {
                    if (ReliableTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendReliable(header, datagram, flags);
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
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public virtual void SendSequential(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock())
                {
                    foreach (var transport in SequentialTransports)
                    {
                        transport.SendSequential(header, datagram, flags);
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
        public bool TrySendSequential(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                    return false;

                using (header.Lock()) using (flags.Lock())
                {
                    foreach (var transport in SequentialTransports)
                    {
                        transport.SendSequential(header, datagram, flags);
                    }
                }

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
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public virtual void SendSequential<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock())
                {
                    GetSequentialTransport<TTransport>().SendSequential(header, datagram, flags);
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
        public bool TrySendSequential<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock())
                {
                    if (SequentialTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendSequential(header, datagram, flags);
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
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public virtual void SendResilient(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock())
                {
                    foreach (var transport in ResilientTransports)
                    {
                        transport.SendResilient(header, datagram, flags);
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
        public bool TrySendResilient(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (ResilientTransports.Count == 0)
                    return false;

                using (header.Lock()) using (flags.Lock())
                {
                    foreach (var transport in ResilientTransports)
                    {
                        transport.SendResilient(header, datagram, flags);
                    }
                }

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
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public virtual void SendResilient<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock())
                {
                    GetResilientTransport<TTransport>().SendResilient(header, datagram, flags);
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
        public bool TrySendResilient<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock())
                {
                    if (ResilientTransports.TryGet(out TTransport? transport))
                    {
                        transport.SendResilient(header, datagram, flags);
                        return true;
                    }

                    return false;
                }
            }
        }
        #endregion
    }
}