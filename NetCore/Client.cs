using NetCore.Transports;
using System;
using System.Runtime.CompilerServices;

namespace NetCore
{
    /// <summary>
    /// Base class working with different <see cref="ITransport"/>s.
    /// </summary>
    /// <inheritdoc cref="NetworkMember"/>
    /// TODO: Improve parallelism by caching transports of a target type when broadcasting a message.
    /// Note: Consider using <see langword="in"/> on all <see cref="ReadOnlySpan{T}"/> datagrams here.
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
        /// .                                                Send Methods
        /// .                                A.K.A.: Good luck translating documentation.
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        #region General sending methods
        /// <summary>
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// </summary>
        /// <seealso cref="SendUnreliable(ReadOnlySpan{byte}, HeaderConstructor?, FlagsConstructor?)"/>
        /// <seealso cref="SendReliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <seealso cref="SendSequential(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <seealso cref="SendResilient(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(SendingMode mode, ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
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

        /// <inheritdoc cref="Send(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(SendingMode mode, ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            Send(mode, ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="Send(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(SendingMode mode, ref Header header, ReadOnlySpan<byte> datagram)
        {
            Flags flags = Flags.Get();
            Send(mode, ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="Send(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(SendingMode mode, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            Send(mode, ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="Send(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="mode"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(SendingMode mode, ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            Send(mode, ref header, datagram, ref flags);
        }

        /// <summary>
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// Throws if no suitable transports were found.
        /// </summary>
        /// <remarks>
        /// This sending method requires target transport to define all transportation methods.
        /// To use transports with only specific transportation methods, please use specialized methods instead.
        /// </remarks>
        /// <seealso cref="SendUnreliable(ReadOnlySpan{byte}, HeaderConstructor?, FlagsConstructor?)"/>
        /// <seealso cref="SendReliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <seealso cref="SendSequential(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <seealso cref="SendResilient(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TTransport>(SendingMode mode, ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
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

        /// <inheritdoc cref="Send{TTransport}(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TTransport>(SendingMode mode, ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            Send<TTransport>(mode, ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="Send{TTransport}(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TTransport>(SendingMode mode, ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Flags flags = Flags.Get();
            Send<TTransport>(mode, ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="Send{TTransport}(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TTransport>(SendingMode mode, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Header header = Header.Get();
            Send<TTransport>(mode, ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="Send{TTransport}(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="mode"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TTransport>(SendingMode mode, ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            Send<TTransport>(mode, ref header, datagram, ref flags);
        }

        /// <summary>
        /// If there are transports that can send the data:
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// </summary>
        /// <seealso cref="SendUnreliable(ReadOnlySpan{byte}, HeaderConstructor?, FlagsConstructor?)"/>
        /// <seealso cref="SendReliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <seealso cref="SendSequential(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <seealso cref="SendResilient(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend(SendingMode mode, ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
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

        /// <inheritdoc cref="TrySend(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend(SendingMode mode, ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (!HasAnyTransport(mode))
                    return false;

                Header header = Header.Get();
                Flags flags = Flags.Get();

                // Uses method which can throw when transport is missing, because previous check get us covered.
                Send(mode, ref header, datagram, ref flags);
                return true;
            }
        }

        /// <inheritdoc cref="TrySend(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend(SendingMode mode, ref Header header, ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (!HasAnyTransport(mode))
                {
                    header.DisposeIfUnlocked();
                    return false;
                }

                Flags flags = Flags.Get();

                // Uses method which can throw when transport is missing, because previous check get us covered.
                Send(mode, ref header, datagram, ref flags);
                return true;
            }
        }

        /// <inheritdoc cref="TrySend(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend(SendingMode mode, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (!HasAnyTransport(mode))
                {
                    flags.DisposeIfUnlocked();
                    return false;
                }

                Header header = Header.Get();

                // Uses method which can throw when transport is missing, because previous check get us covered.
                Send(mode, ref header, datagram, ref flags);
                return true;
            }
        }

        /// <inheritdoc cref="TrySend(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="mode"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend(SendingMode mode, ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            lock (_lock)
            {
                if (!HasAnyTransport(mode))
                {
                    return false;
                }

                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);

                // Uses method which can throw when transport is missing, because previous check get us covered.
                Send(mode, ref header, datagram, ref flags);
                return true;
            }
        }

        /// <summary>
        /// If there are transports that can send the data:
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// </summary>
        /// <seealso cref="SendUnreliable(ReadOnlySpan{byte}, HeaderConstructor?, FlagsConstructor?)"/>
        /// <seealso cref="SendReliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <seealso cref="SendSequential(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <seealso cref="SendResilient(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
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
        public bool TrySend<TTransport>(SendingMode mode, ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
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

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend<TTransport>(SendingMode mode, ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            lock (_lock)
            {
                if (!HasTransport<TTransport>(mode))
                    return false;

                Header header = Header.Get();
                Flags flags = Flags.Get();

                // Uses method which can throw when transport is missing, because previous check get us covered.
                Send<TTransport>(mode, ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend<TTransport>(SendingMode mode, ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            lock (_lock)
            {
                if (!HasTransport<TTransport>(mode))
                {
                    header.DisposeIfUnlocked();
                    return false;
                }

                Flags flags = Flags.Get();

                // Uses method which can throw when transport is missing, because previous check get us covered.
                Send<TTransport>(mode, ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend<TTransport>(SendingMode mode, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            lock (_lock)
            {
                if (!HasTransport<TTransport>(mode))
                {
                    flags.DisposeIfUnlocked();
                    return false;
                }

                Header header = Header.Get();

                // Uses method which can throw when transport is missing, because previous check get us covered.
                Send<TTransport>(mode, ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="mode"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend<TTransport>(SendingMode mode, ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            lock (_lock)
            {
                if (!HasTransport<TTransport>(mode))
                    return false;

                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);

                // Uses method which can throw when transport is missing, because previous check get us covered.
                Send<TTransport>(mode, ref header, datagram, ref flags);
                return true;
            }
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

        /// <inheritdoc cref="SendUnreliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable(ref Header header, ReadOnlySpan<byte> datagram)
        {
            Flags flags = Flags.Get();
            SendUnreliable(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendUnreliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable(ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            SendUnreliable(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendUnreliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable(ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendUnreliable(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendUnreliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendUnreliable(ref header, datagram, ref flags);
        }

        /// <summary>
        /// Tries to unreliably send <paramref name="datagram"/> to the server.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendUnreliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        public bool TrySendUnreliable(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (UnreliableTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    flags.DisposeIfUnlocked();
                    return false;
                }

                SendUnreliable(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable(ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (UnreliableTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                Flags flags = Flags.Get();
                SendUnreliable(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable(ref Header header, ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (UnreliableTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    return false;
                }

                Flags flags = Flags.Get();
                SendUnreliable(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable(ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (UnreliableTransports.Count == 0)
                {
                    flags.DisposeIfUnlocked();
                    return false;
                }

                Header header = Header.Get();
                SendUnreliable(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            lock (_lock)
            {
                if (UnreliableTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);
                SendUnreliable(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendUnreliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/> to send message only if <typeparamref name="TTransport"/> is present.
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

        /// <inheritdoc cref="SendUnreliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable<TTransport>(ReadOnlySpan<byte> datagram)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendUnreliable<TTransport>(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendUnreliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IUnreliableTransport
        {
            Flags flags = Flags.Get();
            SendUnreliable<TTransport>(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendUnreliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable<TTransport>(ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            SendUnreliable<TTransport>(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendUnreliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable<TTransport>(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendUnreliable<TTransport>(ref header, datagram, ref flags);
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
        /// <inheritdoc cref="SendUnreliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        public bool TrySendUnreliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock())
                {
                    if (!UnreliableTransports.TryGet(out TTransport? transport))
                        return false;

                    transport.SendUnreliable(header, datagram, flags);
                    return true;
                }
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable<TTransport>(ReadOnlySpan<byte> datagram)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                if (!UnreliableTransports.TryGet(out TTransport? transport))
                    return false;

                using Header header = Header.GetLocked();
                using Flags flags = Flags.GetLocked();
                transport.SendUnreliable(header, datagram, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                using (header.Lock())
                {
                    if (!UnreliableTransports.TryGet(out TTransport? transport))
                        return false;

                    using Flags flags = Flags.GetLocked();
                    transport.SendUnreliable(header, datagram, flags);
                    return true;
                }
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable<TTransport>(ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                using (flags.Lock())
                {
                    if (!UnreliableTransports.TryGet(out TTransport? transport))
                        return false;

                    using Header header = Header.GetLocked();
                    transport.SendUnreliable(header, datagram, flags);
                    return true;
                }
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable<TTransport>(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                if (!UnreliableTransports.TryGet(out TTransport? transport))
                    return false;

                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);
                using (header.Lock()) using (flags.Lock())
                {
                    transport.SendUnreliable(header, datagram, flags);
                }

                return true;
            }
        }
        #endregion

        #region Narrow sending methods - Reliable
        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server.
        /// </summary>
        /// <remarks>
        /// Locks <paramref name="header"/> and <paramref name="flags"/> on usage.
        /// </remarks>
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

        /// <inheritdoc cref="SendReliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable(ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendReliable(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendReliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable(ref Header header, ReadOnlySpan<byte> datagram)
        {
            Flags flags = Flags.Get();
            SendReliable(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendReliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable(ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            SendReliable(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendReliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendReliable(ref header, datagram, ref flags);
        }

        /// <summary>
        /// Tries to reliably send <paramref name="datagram"/> to the server.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendReliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        public bool TrySendReliable(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    flags.DisposeIfUnlocked();
                    return false;
                }

                SendReliable(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable(ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                Flags flags = Flags.Get();
                SendReliable(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable(ref Header header, ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    return false;
                }

                Flags flags = Flags.Get();
                SendReliable(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable(ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                {
                    flags.DisposeIfUnlocked();
                    return false;
                }

                Header header = Header.Get();
                SendReliable(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);
                SendReliable(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendReliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/> to send message only if <typeparamref name="TTransport"/> is present.
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

        /// <inheritdoc cref="SendReliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable<TTransport>(ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendReliable<TTransport>(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendReliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport
        {
            Flags flags = Flags.Get();
            SendReliable<TTransport>(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendReliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable<TTransport>(ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            SendReliable<TTransport>(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendReliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable<TTransport>(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendReliable<TTransport>(ref header, datagram, ref flags);
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
        /// <inheritdoc cref="SendReliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        public bool TrySendReliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock())
                {
                    if (!ReliableTransports.TryGet(out TTransport? transport))
                        return false;

                    transport.SendReliable(header, datagram, flags);
                    return true;
                }
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable<TTransport>(ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                if (!ReliableTransports.TryGet(out TTransport? transport))
                    return false;

                using Header header = Header.GetLocked();
                using Flags flags = Flags.GetLocked();
                transport.SendReliable(header, datagram, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                if (!ReliableTransports.TryGet(out TTransport? transport))
                    return false;

                using Flags flags = Flags.GetLocked();
                transport.SendReliable(header, datagram, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable<TTransport>(ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                if (!ReliableTransports.TryGet(out TTransport? transport))
                    return false;

                using Header header = Header.GetLocked();
                transport.SendReliable(header, datagram, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable<TTransport>(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                if (!ReliableTransports.TryGet(out TTransport? transport))
                    return false;

                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);
                using (header.Lock()) using (flags.Lock())
                {
                    transport.SendReliable(header, datagram, flags);
                }

                return true;
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

        /// <inheritdoc cref="SendSequential(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential(ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendSequential(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendSequential(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential(ref Header header, ReadOnlySpan<byte> datagram)
        {
            Flags flags = Flags.Get();
            SendSequential(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendSequential(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential(ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            SendSequential(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendSequential(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendSequential(ref header, datagram, ref flags);
        }

        /// <summary>
        /// Tries to sequentially send <paramref name="datagram"/> to the server.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendSequential(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        public bool TrySendSequential(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (SequentialTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    flags.DisposeIfUnlocked();
                    return false;
                }

                SendSequential(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential(ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (SequentialTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                Flags flags = Flags.Get();
                SendSequential(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential(ref Header header, ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (SequentialTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    return false;
                }

                Flags flags = Flags.Get();
                SendSequential(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential(ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (SequentialTransports.Count == 0)
                {
                    flags.DisposeIfUnlocked();
                    return false;
                }

                Header header = Header.Get();
                SendSequential(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            lock (_lock)
            {
                if (SequentialTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);
                SendSequential(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <summary>
        /// Sequentially sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendSequential{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="ISequentialTransport"/> to use for sending of a message.</typeparam>
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

        /// <inheritdoc cref="SendSequential{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential<TTransport>(ReadOnlySpan<byte> datagram)
            where TTransport : class, ISequentialTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendSequential<TTransport>(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendSequential{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, ISequentialTransport
        {
            Flags flags = Flags.Get();
            SendSequential<TTransport>(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendSequential{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential<TTransport>(ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, ISequentialTransport
        {
            Header header = Header.Get();
            SendSequential<TTransport>(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendSequential{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential<TTransport>(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, ISequentialTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendSequential<TTransport>(ref header, datagram, ref flags);
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
        /// <inheritdoc cref="SendSequential{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        public bool TrySendSequential<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock())
                {
                    if (!SequentialTransports.TryGet(out TTransport? transport))
                    {
                        return false;
                    }

                    transport.SendSequential(header, datagram, flags);
                    return true;
                }
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential<TTransport>(ReadOnlySpan<byte> datagram)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                if (!SequentialTransports.TryGet(out TTransport? transport))
                    return false;

                using Header header = Header.GetLocked();
                using Flags flags = Flags.GetLocked();
                transport.SendSequential(header, datagram, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                if (!SequentialTransports.TryGet(out TTransport? transport))
                {
                    header.DisposeIfUnlocked();
                    return false;
                }

                using Flags flags = Flags.GetLocked();
                transport.SendSequential(header, datagram, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential<TTransport>(ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                if (!SequentialTransports.TryGet(out TTransport? transport))
                {
                    flags.DisposeIfUnlocked();
                    return false;
                }

                using Header header = Header.GetLocked();
                transport.SendSequential(header, datagram, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential<TTransport>(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                if (!SequentialTransports.TryGet(out TTransport? transport))
                    return false;

                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);
                using (header.Lock()) using (flags.Lock())
                {
                    transport.SendSequential(header, datagram, flags);
                }

                return true;
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

        /// <inheritdoc cref="SendResilient(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient(ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendResilient(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendResilient(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient(ref Header header, ReadOnlySpan<byte> datagram)
        {
            Flags flags = Flags.Get();
            SendResilient(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendResilient(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient(ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            SendResilient(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendResilient(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendResilient(ref header, datagram, ref flags);
        }

        /// <summary>
        /// Tries to resiliently send <paramref name="datagram"/> to the server.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendResilient(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        public bool TrySendResilient(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (ResilientTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    flags.DisposeIfUnlocked();
                    return false;
                }

                SendResilient(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient(ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (ResilientTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                Flags flags = Flags.Get();
                SendResilient(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient(ref Header header, ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (ResilientTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    return false;
                }

                Flags flags = Flags.Get();
                SendResilient(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient(ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (ResilientTransports.Count == 0)
                {
                    flags.DisposeIfUnlocked();
                    return false;
                }

                Header header = Header.Get();
                SendResilient(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            lock (_lock)
            {
                if (ResilientTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);
                SendResilient(ref header, datagram, ref flags);
                return true;
            }
        }

        /// <summary>
        /// Resiliently sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendResilient{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/> to send message only if <typeparamref name="TTransport"/> is present.
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

        /// <inheritdoc cref="SendResilient{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient<TTransport>(ReadOnlySpan<byte> datagram)
            where TTransport : class, IResilientTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendResilient<TTransport>(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendResilient{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IResilientTransport
        {
            Flags flags = Flags.Get();
            SendResilient<TTransport>(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendResilient{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient<TTransport>(ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IResilientTransport
        {
            Header header = Header.Get();
            SendResilient<TTransport>(ref header, datagram, ref flags);
        }

        /// <inheritdoc cref="SendResilient{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient<TTransport>(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IResilientTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendResilient<TTransport>(ref header, datagram, ref flags);
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
        /// <inheritdoc cref="SendResilient(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        public bool TrySendResilient<TTransport>(ref Header header, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock())
                {
                    if (!ResilientTransports.TryGet(out TTransport? transport))
                        return false;

                    transport.SendResilient(header, datagram, flags);
                    return true;
                }
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient<TTransport>(ReadOnlySpan<byte> datagram)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                if (!ResilientTransports.TryGet(out TTransport? transport))
                    return false;

                using Header header = Header.GetLocked();
                using Flags flags = Flags.GetLocked();
                transport.SendResilient(header, datagram, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient<TTransport>(ref Header header, ReadOnlySpan<byte> datagram)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                if (!ResilientTransports.TryGet(out TTransport? transport))
                {
                    header.DisposeIfUnlocked();
                    return false;
                }

                using Flags flags = Flags.GetLocked();
                transport.SendResilient(header, datagram, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient<TTransport>(ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                if (!ResilientTransports.TryGet(out TTransport? transport))
                {
                    flags.DisposeIfUnlocked();
                    return false;
                }

                using Header header = Header.GetLocked();
                transport.SendResilient(header, datagram, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient{TTransport}(ref Header, ReadOnlySpan{byte}, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient<TTransport>(ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                if (!ResilientTransports.TryGet(out TTransport? transport))
                    return false;

                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);
                using (header.Lock()) using (flags.Lock())
                {
                    transport.SendResilient(header, datagram, flags);
                }

                return true;
            }
        }
        #endregion
    }
}