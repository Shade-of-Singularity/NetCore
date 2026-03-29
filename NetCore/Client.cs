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
        /// <seealso cref="SendUnreliable(in ReadOnlySpan{byte}, HeaderConstructor?, FlagsConstructor?)"/>
        /// <seealso cref="SendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            switch (mode)
            {
                case SendingMode.Unreliable: SendUnreliable(datagram, ref header, ref flags); return;
                case SendingMode.Reliable: SendReliable(datagram, ref header, ref flags); return;
                case SendingMode.Sequential: SendSequential(datagram, ref header, ref flags); return;
                case SendingMode.Resilient: SendResilient(datagram, ref header, ref flags); return;
                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <inheritdoc cref="Send(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(SendingMode mode, ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            Send(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header)
        {
            Flags flags = Flags.Get();
            Send(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(SendingMode mode, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            Send(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="mode"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send(SendingMode mode, in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            Send(mode, datagram, ref header, ref flags);
        }

        /// <summary>
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// Throws if no suitable transports were found.
        /// </summary>
        /// <remarks>
        /// This sending method requires target transport to define all transportation methods.
        /// To use transports with only specific transportation methods, please use specialized methods instead.
        /// </remarks>
        /// <seealso cref="SendUnreliable(in ReadOnlySpan{byte}, HeaderConstructor?, FlagsConstructor?)"/>
        /// <seealso cref="SendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            switch (mode)
            {
                case SendingMode.Unreliable: SendUnreliable<TTransport>(datagram, ref header, ref flags); return;
                case SendingMode.Reliable: SendReliable<TTransport>(datagram, ref header, ref flags); return;
                case SendingMode.Sequential: SendSequential<TTransport>(datagram, ref header, ref flags); return;
                case SendingMode.Resilient: SendResilient<TTransport>(datagram, ref header, ref flags); return;
                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <inheritdoc cref="Send{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TTransport>(SendingMode mode, ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            Send<TTransport>(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Flags flags = Flags.Get();
            Send<TTransport>(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TTransport>(SendingMode mode, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Header header = Header.Get();
            Send<TTransport>(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="mode"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Send<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            Send<TTransport>(mode, datagram, ref header, ref flags);
        }

        /// <summary>
        /// If there are transports that can send the data:
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// </summary>
        /// <seealso cref="SendUnreliable(in ReadOnlySpan{byte}, HeaderConstructor?, FlagsConstructor?)"/>
        /// <seealso cref="SendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            return mode switch
            {
                SendingMode.Unreliable => TrySendUnreliable(datagram, ref header, ref flags),
                SendingMode.Reliable => TrySendReliable(datagram, ref header, ref flags),
                SendingMode.Sequential => TrySendSequential(datagram, ref header, ref flags),
                SendingMode.Resilient => TrySendResilient(datagram, ref header, ref flags),
                _ => throw new SwitchExpressionException(mode),
            };
        }

        /// <inheritdoc cref="TrySend(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
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
                Send(mode, datagram, ref header, ref flags);
                return true;
            }
        }

        /// <inheritdoc cref="TrySend(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header)
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
                Send(mode, datagram, ref header, ref flags);
                return true;
            }
        }

        /// <inheritdoc cref="TrySend(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
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
                Send(mode, datagram, ref header, ref flags);
                return true;
            }
        }

        /// <inheritdoc cref="TrySend(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="mode"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend(SendingMode mode, in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
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
                Send(mode, datagram, ref header, ref flags);
                return true;
            }
        }

        /// <summary>
        /// If there are transports that can send the data:
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// </summary>
        /// <seealso cref="SendUnreliable(in ReadOnlySpan{byte}, HeaderConstructor?, FlagsConstructor?)"/>
        /// <seealso cref="SendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
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
        public bool TrySend<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            return mode switch
            {
                SendingMode.Unreliable => TrySendUnreliable<TTransport>(datagram, ref header, ref flags),
                SendingMode.Reliable => TrySendReliable<TTransport>(datagram, ref header, ref flags),
                SendingMode.Sequential => TrySendSequential<TTransport>(datagram, ref header, ref flags),
                SendingMode.Resilient => TrySendResilient<TTransport>(datagram, ref header, ref flags),
                _ => throw new SwitchExpressionException(mode),
            };
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
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
                Send<TTransport>(mode, datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header)
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
                Send<TTransport>(mode, datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
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
                Send<TTransport>(mode, datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="mode"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySend<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
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
                Send<TTransport>(mode, datagram, ref header, ref flags);
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
        public virtual void SendUnreliable(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock) SendUnreliable(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        void SendUnreliableUnlocked(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            using (header.Lock()) using (flags.Lock())
            {
                foreach (var transport in UnreliableTransports)
                {
                    transport.SendUnreliable(datagram, header, flags);
                }
            }
        }

        /// <inheritdoc cref="SendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            Flags flags = Flags.Get();
            SendUnreliable(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            SendUnreliable(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable(in ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendUnreliable(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendUnreliable(datagram, ref header, ref flags);
        }

        /// <summary>
        /// Tries to unreliably send <paramref name="datagram"/> to the server.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendUnreliable(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock)
            {
                if (UnreliableTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    flags.DisposeIfUnlocked();
                    return false;
                }

                SendUnreliable(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable(in ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (UnreliableTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                Flags flags = Flags.Get();
                SendUnreliable(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            lock (_lock)
            {
                if (UnreliableTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    return false;
                }

                Flags flags = Flags.Get();
                SendUnreliable(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (UnreliableTransports.Count == 0)
                {
                    flags.DisposeIfUnlocked();
                    return false;
                }

                Header header = Header.Get();
                SendUnreliable(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            lock (_lock)
            {
                if (UnreliableTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);
                SendUnreliable(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <summary>
        /// Unreliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public virtual void SendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock())
                {
                    GetUnreliableTransport<TTransport>().SendUnreliable(datagram, header, flags);
                }
            }
        }

        /// <inheritdoc cref="SendUnreliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendUnreliable<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendUnreliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IUnreliableTransport
        {
            Flags flags = Flags.Get();
            SendUnreliable<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendUnreliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            SendUnreliable<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendUnreliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendUnreliable<TTransport>(datagram, ref header, ref flags);
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
        /// <inheritdoc cref="SendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock())
                {
                    if (!UnreliableTransports.TryGet(out TTransport? transport))
                        return false;

                    transport.SendUnreliable(datagram, header, flags);
                    return true;
                }
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                if (!UnreliableTransports.TryGet(out TTransport? transport))
                    return false;

                using Header header = Header.GetLocked();
                using Flags flags = Flags.GetLocked();
                transport.SendUnreliable(datagram, header, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                using (header.Lock())
                {
                    if (!UnreliableTransports.TryGet(out TTransport? transport))
                        return false;

                    using Flags flags = Flags.GetLocked();
                    transport.SendUnreliable(datagram, header, flags);
                    return true;
                }
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                using (flags.Lock())
                {
                    if (!UnreliableTransports.TryGet(out TTransport? transport))
                        return false;

                    using Header header = Header.GetLocked();
                    transport.SendUnreliable(datagram, header, flags);
                    return true;
                }
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendUnreliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
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
                    transport.SendUnreliable(datagram, header, flags);
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
        public virtual void SendReliable(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock())
                {
                    foreach (var transport in ReliableTransports)
                    {
                        transport.SendReliable(datagram, header, flags);
                    }
                }
            }
        }

        /// <inheritdoc cref="SendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable(in ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendReliable(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            Flags flags = Flags.Get();
            SendReliable(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            SendReliable(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendReliable(datagram, ref header, ref flags);
        }

        /// <summary>
        /// Tries to reliably send <paramref name="datagram"/> to the server.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendReliable(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    flags.DisposeIfUnlocked();
                    return false;
                }

                SendReliable(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable(in ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                Flags flags = Flags.Get();
                SendReliable(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    return false;
                }

                Flags flags = Flags.Get();
                SendReliable(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                {
                    flags.DisposeIfUnlocked();
                    return false;
                }

                Header header = Header.Get();
                SendReliable(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            lock (_lock)
            {
                if (ReliableTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);
                SendReliable(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <summary>
        /// Reliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public virtual void SendReliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock())
                {
                    GetReliableTransport<TTransport>().SendReliable(datagram, header, flags);
                }
            }
        }

        /// <inheritdoc cref="SendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendReliable<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IReliableTransport
        {
            Flags flags = Flags.Get();
            SendReliable<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            SendReliable<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendReliable<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendReliable<TTransport>(datagram, ref header, ref flags);
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
        /// <inheritdoc cref="SendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendReliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock())
                {
                    if (!ReliableTransports.TryGet(out TTransport? transport))
                        return false;

                    transport.SendReliable(datagram, header, flags);
                    return true;
                }
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                if (!ReliableTransports.TryGet(out TTransport? transport))
                    return false;

                using Header header = Header.GetLocked();
                using Flags flags = Flags.GetLocked();
                transport.SendReliable(datagram, header, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                if (!ReliableTransports.TryGet(out TTransport? transport))
                    return false;

                using Flags flags = Flags.GetLocked();
                transport.SendReliable(datagram, header, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                if (!ReliableTransports.TryGet(out TTransport? transport))
                    return false;

                using Header header = Header.GetLocked();
                transport.SendReliable(datagram, header, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendReliable<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
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
                    transport.SendReliable(datagram, header, flags);
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
        public virtual void SendSequential(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock())
                {
                    foreach (var transport in SequentialTransports)
                    {
                        transport.SendSequential(datagram, header, flags);
                    }
                }
            }
        }

        /// <inheritdoc cref="SendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential(in ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendSequential(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            Flags flags = Flags.Get();
            SendSequential(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            SendSequential(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendSequential(datagram, ref header, ref flags);
        }

        /// <summary>
        /// Tries to sequentially send <paramref name="datagram"/> to the server.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendSequential(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock)
            {
                if (SequentialTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    flags.DisposeIfUnlocked();
                    return false;
                }

                SendSequential(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential(in ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (SequentialTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                Flags flags = Flags.Get();
                SendSequential(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            lock (_lock)
            {
                if (SequentialTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    return false;
                }

                Flags flags = Flags.Get();
                SendSequential(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (SequentialTransports.Count == 0)
                {
                    flags.DisposeIfUnlocked();
                    return false;
                }

                Header header = Header.Get();
                SendSequential(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            lock (_lock)
            {
                if (SequentialTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);
                SendSequential(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <summary>
        /// Sequentially sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="ISequentialTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public virtual void SendSequential<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock())
                {
                    GetSequentialTransport<TTransport>().SendSequential(datagram, header, flags);
                }
            }
        }

        /// <inheritdoc cref="SendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, ISequentialTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendSequential<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, ISequentialTransport
        {
            Flags flags = Flags.Get();
            SendSequential<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, ISequentialTransport
        {
            Header header = Header.Get();
            SendSequential<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendSequential<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, ISequentialTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendSequential<TTransport>(datagram, ref header, ref flags);
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
        /// <inheritdoc cref="SendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendSequential<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
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

                    transport.SendSequential(datagram, header, flags);
                    return true;
                }
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                if (!SequentialTransports.TryGet(out TTransport? transport))
                    return false;

                using Header header = Header.GetLocked();
                using Flags flags = Flags.GetLocked();
                transport.SendSequential(datagram, header, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
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
                transport.SendSequential(datagram, header, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
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
                transport.SendSequential(datagram, header, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendSequential<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
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
                    transport.SendSequential(datagram, header, flags);
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
        public virtual void SendResilient(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock())
                {
                    foreach (var transport in ResilientTransports)
                    {
                        transport.SendResilient(datagram, header, flags);
                    }
                }
            }
        }

        /// <inheritdoc cref="SendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient(in ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendResilient(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            Flags flags = Flags.Get();
            SendResilient(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            SendResilient(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendResilient(datagram, ref header, ref flags);
        }

        /// <summary>
        /// Tries to resiliently send <paramref name="datagram"/> to the server.
        /// </summary>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendResilient(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock)
            {
                if (ResilientTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    flags.DisposeIfUnlocked();
                    return false;
                }

                SendResilient(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient(in ReadOnlySpan<byte> datagram)
        {
            lock (_lock)
            {
                if (ResilientTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                Flags flags = Flags.Get();
                SendResilient(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            lock (_lock)
            {
                if (ResilientTransports.Count == 0)
                {
                    header.DisposeIfUnlocked();
                    return false;
                }

                Flags flags = Flags.Get();
                SendResilient(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            lock (_lock)
            {
                if (ResilientTransports.Count == 0)
                {
                    flags.DisposeIfUnlocked();
                    return false;
                }

                Header header = Header.Get();
                SendResilient(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            lock (_lock)
            {
                if (ResilientTransports.Count == 0)
                    return false;

                Header header = Header.Get();
                headerSetup?.Invoke(ref header);
                Flags flags = Flags.Get();
                flagsSetup?.Invoke(ref flags);
                SendResilient(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <summary>
        /// Resiliently sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public virtual void SendResilient<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                using (header.Lock()) using (flags.Lock())
                {
                    GetResilientTransport<TTransport>().SendResilient(datagram, header, flags);
                }
            }
        }

        /// <inheritdoc cref="SendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, IResilientTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            SendResilient<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IResilientTransport
        {
            Flags flags = Flags.Get();
            SendResilient<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IResilientTransport
        {
            Header header = Header.Get();
            SendResilient<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendResilient<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IResilientTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            SendResilient<TTransport>(datagram, ref header, ref flags);
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
        /// <inheritdoc cref="SendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendResilient<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                // Locks even if there is no transport, to release the resources.
                using (header.Lock()) using (flags.Lock())
                {
                    if (!ResilientTransports.TryGet(out TTransport? transport))
                        return false;

                    transport.SendResilient(datagram, header, flags);
                    return true;
                }
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                if (!ResilientTransports.TryGet(out TTransport? transport))
                    return false;

                using Header header = Header.GetLocked();
                using Flags flags = Flags.GetLocked();
                transport.SendResilient(datagram, header, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
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
                transport.SendResilient(datagram, header, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
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
                transport.SendResilient(datagram, header, flags);
                return true;
            }
        }

        /// <remarks>
        /// Does not allocate <see cref="Header"/> and <see cref="Flags"/> unless target transport actually exist.
        /// </remarks>
        /// <inheritdoc cref="TrySendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TrySendResilient<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
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
                    transport.SendResilient(datagram, header, flags);
                }

                return true;
            }
        }
        #endregion
    }
}