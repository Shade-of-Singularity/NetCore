using NetCore.Transports;
using NetCore.Transports.TCP;
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
        /// <para>Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <seealso cref="SendUnreliable(in ReadOnlySpan{byte}, HeaderConstructor?, FlagsConstructor?)"/>
        /// <seealso cref="SendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        void SendCore(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            switch (mode)
            {
                case SendingMode.Unreliable: SendUnreliableCore(datagram, ref header, ref flags); return;
                case SendingMode.Reliable: SendReliableCore(datagram, ref header, ref flags); return;
                case SendingMode.Sequential: SendSequentialCore(datagram, ref header, ref flags); return;
                case SendingMode.Resilient: SendResilientCore(datagram, ref header, ref flags); return;
                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <inheritdoc cref="Send(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void Send(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock) SendCore(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void Send(SendingMode mode, in ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendCore(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void Send(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header)
        {
            Flags flags = Flags.Get();
            lock (_lock) SendCore(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void Send(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            lock (_lock) SendCore(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="mode"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public void Send(SendingMode mode, in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendCore(mode, datagram, ref header, ref flags);
        }




        /// <summary>
        /// If there are transports that can send the data:
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// As a bonus: allocate less if target transport doesn't exist.
        /// </remarks>
        /// <seealso cref="SendUnreliable(in ReadOnlySpan{byte}, HeaderConstructor?, FlagsConstructor?)"/>
        /// <seealso cref="SendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
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
        public bool TrySend(SendingMode mode, in ReadOnlySpan<byte> datagram)
        {
            if (!HasAnyTransport(mode))
                return false;

            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendCore(mode, datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySend(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySend(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header)
        {
            if (!HasAnyTransport(mode))
            {
                header.DisposeIfUnlocked();
                return false;
            }

            Flags flags = Flags.Get();
            lock (_lock) SendCore(mode, datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySend(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySend(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            if (!HasAnyTransport(mode))
            {
                flags.DisposeIfUnlocked();
                return false;
            }

            Header header = Header.Get();
            lock (_lock) SendCore(mode, datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySend(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="mode"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public bool TrySend(SendingMode mode, in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            if (!HasAnyTransport(mode))
                return false;

            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendCore(mode, datagram, ref header, ref flags);
            return true;
        }




        /// <summary>
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// Throws if no suitable transports were found.
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// This sending method requires target transport to define all transportation methods.
        /// To use transports with only specific transportation methods, please use specialized methods instead.
        /// </remarks>
        /// <seealso cref="SendUnreliable{TTransport}(in ReadOnlySpan{byte}, HeaderConstructor?, FlagsConstructor?)"/>
        /// <seealso cref="SendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// TODO: Remove a constraint for <typeparam name="TTransport"/> of using all transport types.
        ///  It should be now possible after <see cref="Common.CRTPList{TBase}.Lookup{TFilter}"/> rework.
        void SendCore<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
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

        /// <inheritdoc cref="SendCore{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void Send<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            lock (_lock) SendCore<TTransport>(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendCore{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void Send<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendCore<TTransport>(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendCore{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void Send<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Flags flags = Flags.Get();
            lock (_lock) SendCore<TTransport>(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendCore{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void Send<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Header header = Header.Get();
            lock (_lock) SendCore<TTransport>(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendCore{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="mode"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public void Send<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendCore<TTransport>(mode, datagram, ref header, ref flags);
        }




        /// <summary>
        /// If there are transports that can send the data:
        /// <para>Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// As a bonus: allocate less if target transport doesn't exist.
        /// </remarks>
        /// <seealso cref="SendUnreliable{TTransport}(in ReadOnlySpan{byte}, HeaderConstructor?, FlagsConstructor?)"/>
        /// <seealso cref="SendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <typeparam name="TTransport"><see cref="ITransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> if there was no <typeparamref name="TTransport"/> registered and nothing was sent.
        /// </returns>
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

        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySend<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            if (!HasTransport<TTransport>(mode))
                return false;

            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendCore<TTransport>(mode, datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySend<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            if (!HasTransport<TTransport>(mode))
                return false;

            Flags flags = Flags.Get();
            lock (_lock) SendCore<TTransport>(mode, datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySend<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            if (!HasTransport<TTransport>(mode))
                return false;

            Header header = Header.Get();
            lock (_lock) SendCore<TTransport>(mode, datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="mode"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public bool TrySend<TTransport>(SendingMode mode, in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            if (!HasTransport<TTransport>(mode))
                return false;

            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendCore<TTransport>(mode, datagram, ref header, ref flags);
            return true;
        }
        #endregion

        #region Narrow sending methods - Unreliable
        /// <summary>
        /// <para>Unreliably sends <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        protected virtual void SendUnreliableCore(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            using (header.Lock()) using (flags.Lock())
            {
                foreach (var transport in UnreliableTransports)
                {
                    transport.SendUnreliable(datagram, header, flags);
                }
            }
        }

        /// <inheritdoc cref="SendUnreliableCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendUnreliable(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock) SendUnreliableCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendUnreliableCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendUnreliable(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            Flags flags = Flags.Get();
            lock (_lock) SendUnreliableCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendUnreliableCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendUnreliable(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            lock (_lock) SendUnreliableCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendUnreliableCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendUnreliable(in ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendUnreliableCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendUnreliableCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public void SendUnreliable(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendUnreliableCore(datagram, ref header, ref flags);
        }




        /// <summary>
        /// <para>Tries to unreliably send <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// As a bonus: allocate less if target transport doesn't exist.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendUnreliableCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
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

                SendUnreliableCore(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <inheritdoc cref="TrySendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendUnreliable(in ReadOnlySpan<byte> datagram)
        {
            if (!HasAnyUnreliableTransport())
                return false;

            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendUnreliableCore(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendUnreliable(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            if (!HasAnyUnreliableTransport())
            {
                header.DisposeIfUnlocked();
                return false;
            }

            Flags flags = Flags.Get();
            lock (_lock) SendUnreliableCore(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendUnreliable(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            if (!HasAnyUnreliableTransport())
            {
                flags.DisposeIfUnlocked();
                return false;
            }

            Header header = Header.Get();
            lock (_lock) SendUnreliableCore(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public bool TrySendUnreliable(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            if (!HasAnyUnreliableTransport())
                return false;

            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendUnreliableCore(datagram, ref header, ref flags);
            return true;
        }




        /// <summary>
        /// <para>Unreliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendUnreliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IUnreliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        protected virtual void SendUnreliableCore<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IUnreliableTransport
        {
            using (header.Lock()) using (flags.Lock())
            {
                GetUnreliableTransport<TTransport>().SendUnreliable(datagram, header, flags);
            }
        }

        /// <inheritdoc cref="SendUnreliableCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock) SendUnreliableCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendUnreliableCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendUnreliableCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendUnreliableCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IUnreliableTransport
        {
            Flags flags = Flags.Get();
            lock (_lock) SendUnreliableCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendUnreliableCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            lock (_lock) SendUnreliableCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendUnreliableCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public void SendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IUnreliableTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendUnreliableCore<TTransport>(datagram, ref header, ref flags);
        }




        /// <summary>
        /// <para>Tries to unreliably send <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// As a bonus: allocate less if target transport doesn't exist.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> if there was no <typeparamref name="TTransport"/> registered and nothing was sent.
        /// </returns>
        /// <inheritdoc cref="SendUnreliableCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                if (!UnreliableTransports.Has<TTransport>())
                {
                    header.DisposeIfUnlocked();
                    flags.DisposeIfUnlocked();
                    return false;
                }

                SendUnreliableCore<TTransport>(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <inheritdoc cref="TrySendUnreliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, IUnreliableTransport
        {
            if (!HasUnreliableTransport<TTransport>())
                return false;

            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendUnreliableCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendUnreliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IUnreliableTransport
        {
            if (!HasUnreliableTransport<TTransport>())
                return false;

            Flags flags = Flags.Get();
            lock (_lock) SendUnreliableCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendUnreliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IUnreliableTransport
        {
            if (!HasUnreliableTransport<TTransport>())
                return false;

            Header header = Header.Get();
            lock (_lock) SendUnreliableCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendUnreliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public bool TrySendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IUnreliableTransport
        {
            if (!HasUnreliableTransport<TTransport>())
                return false;

            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendUnreliableCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }
        #endregion

        #region Narrow sending methods - Reliable
        /// <summary>
        /// <para>Reliably sends <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        protected virtual void SendReliableCore(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            using (header.Lock()) using (flags.Lock())
            {
                foreach (var transport in ReliableTransports)
                {
                    transport.SendReliable(datagram, header, flags);
                }
            }
        }

        /// <inheritdoc cref="SendReliableCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendReliable(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock) SendReliableCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendReliableCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendReliable(in ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendReliableCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendReliableCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendReliable(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            Flags flags = Flags.Get();
            lock (_lock) SendReliableCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendReliableCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendReliable(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            lock (_lock) SendReliableCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendReliableCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public void SendReliable(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendReliableCore(datagram, ref header, ref flags);
        }




        /// <summary>
        /// <para>Tries to reliably send <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// As a bonus: allocate less if target transport doesn't exist.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendReliableCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
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

                SendReliableCore(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <inheritdoc cref="TrySendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendReliable(in ReadOnlySpan<byte> datagram)
        {
            if (!HasAnyReliableTransport())
                return false;

            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendUnreliableCore(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendReliable(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            if (!HasAnyReliableTransport())
                return false;

            Flags flags = Flags.Get();
            lock (_lock) SendUnreliableCore(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendReliable(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            if (!HasAnyReliableTransport())
                return false;

            Header header = Header.Get();
            lock (_lock) SendUnreliableCore(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendReliable(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public bool TrySendReliable(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            if (!HasAnyReliableTransport())
                return false;

            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendUnreliableCore(datagram, ref header, ref flags);
            return true;
        }




        /// <summary>
        /// <para>Reliably sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        protected virtual void SendReliableCore<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IReliableTransport
        {
            using (header.Lock()) using (flags.Lock())
            {
                GetReliableTransport<TTransport>().SendReliable(datagram, header, flags);
            }
        }

        /// <inheritdoc cref="SendReliableCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public virtual void SendReliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IReliableTransport
        {
            lock (_lock) SendReliableCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendReliableCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendReliable<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendReliableCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendReliableCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendReliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IReliableTransport
        {
            Flags flags = Flags.Get();
            lock (_lock) SendReliableCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendReliableCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendReliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            lock (_lock) SendReliableCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendReliableCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public void SendReliable<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IReliableTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendReliableCore<TTransport>(datagram, ref header, ref flags);
        }




        /// <summary>
        /// <para>Tries to reliably send <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// As a bonus: allocate less if target transport doesn't exist.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> if there was no <typeparamref name="TTransport"/> registered and nothing was sent.
        /// </returns>
        /// <inheritdoc cref="SendReliableCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendReliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                if (!ReliableTransports.Has<TTransport>())
                {
                    header.DisposeIfUnlocked();
                    flags.DisposeIfUnlocked();
                    return false;
                }

                SendReliableCore<TTransport>(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <inheritdoc cref="TrySendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendReliable<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport
        {
            if (!HasReliableTransport<TTransport>())
                return false;

            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendReliableCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendReliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IReliableTransport
        {
            if (!HasReliableTransport<TTransport>())
                return false;

            Flags flags = Flags.Get();
            lock (_lock) SendReliableCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendReliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport
        {
            if (!HasReliableTransport<TTransport>())
                return false;

            Header header = Header.Get();
            lock (_lock) SendReliableCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public bool TrySendReliable<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IReliableTransport
        {
            if (!HasReliableTransport<TTransport>())
                return false;

            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendReliableCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }
        #endregion

        #region Narrow sending methods - Sequential
        /// <summary>
        /// <para>Sequentially sends <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        protected virtual void SendSequentialCore(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            using (header.Lock()) using (flags.Lock())
            {
                foreach (var transport in SequentialTransports)
                {
                    transport.SendSequential(datagram, header, flags);
                }
            }
        }

        /// <inheritdoc cref="SendSequentialCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public virtual void SendSequential(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock) SendSequentialCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendSequentialCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendSequential(in ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendSequentialCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendSequentialCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendSequential(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            Flags flags = Flags.Get();
            lock (_lock) SendSequentialCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendSequentialCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendSequential(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            lock (_lock) SendSequentialCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendSequentialCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public void SendSequential(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendSequentialCore(datagram, ref header, ref flags);
        }




        /// <summary>
        /// <para>Tries to sequentially send <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// As a bonus: allocate less if target transport doesn't exist.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendSequentialCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
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

                SendSequentialCore(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <inheritdoc cref="TrySendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendSequential(in ReadOnlySpan<byte> datagram)
        {
            if (!HasAnySequentialTransport())
                return false;

            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendSequentialCore(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendSequential(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            if (!HasAnySequentialTransport())
                return false;

            Flags flags = Flags.Get();
            lock (_lock) SendSequentialCore(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendSequential(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            if (!HasAnySequentialTransport())
                return false;

            Header header = Header.Get();
            lock (_lock) SendSequentialCore(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendSequential(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public bool TrySendSequential(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            if (!HasAnySequentialTransport())
                return false;

            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendSequentialCore(datagram, ref header, ref flags);
            return true;
        }




        /// <summary>
        /// <para>Sequentially sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="ISequentialTransport"/> to use for sending of a message.</typeparam>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public virtual void SendSequentialCore<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, ISequentialTransport
        {
            using (header.Lock()) using (flags.Lock())
            {
                GetSequentialTransport<TTransport>().SendSequential(datagram, header, flags);
            }
        }

        /// <inheritdoc cref="SendSequentialCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendSequential<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock) SendSequentialCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendSequentialCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendSequential<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, ISequentialTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendSequentialCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendSequentialCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendSequential<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, ISequentialTransport
        {
            Flags flags = Flags.Get();
            lock (_lock) SendSequentialCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendSequentialCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendSequential<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, ISequentialTransport
        {
            Header header = Header.Get();
            lock (_lock) SendSequentialCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendSequentialCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public void SendSequential<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, ISequentialTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendSequentialCore<TTransport>(datagram, ref header, ref flags);
        }




        /// <summary>
        /// Tries to sequentially send <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.
        /// </summary>
        /// <remarks>
        /// As a bonus: allocate less if target transport doesn't exist.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> if there was no <typeparamref name="TTransport"/> registered and nothing was sent.
        /// </returns>
        /// <inheritdoc cref="SendSequentialCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendSequential<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                if (!SequentialTransports.Has<TTransport>())
                {
                    header.DisposeIfUnlocked();
                    flags.DisposeIfUnlocked();
                    return false;
                }

                SendSequentialCore<TTransport>(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <inheritdoc cref="TrySendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendSequential<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, ISequentialTransport
        {
            if (!HasSequentialTransport<TTransport>())
                return false;

            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendSequentialCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendSequential<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, ISequentialTransport
        {
            if (!HasSequentialTransport<TTransport>())
                return false;

            Flags flags = Flags.Get();
            lock (_lock) SendSequentialCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendSequential<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, ISequentialTransport
        {
            if (!HasSequentialTransport<TTransport>())
                return false;

            Header header = Header.Get();
            lock (_lock) SendSequentialCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public bool TrySendSequential<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, ISequentialTransport
        {
            if (!HasSequentialTransport<TTransport>())
                return false;

            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendSequentialCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }
        #endregion

        #region Narrow sending methods - Resilient
        /// <summary>
        /// <para>Resiliently sends <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        protected virtual void SendResilientCore(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            using (header.Lock()) using (flags.Lock())
            {
                foreach (var transport in ResilientTransports)
                {
                    transport.SendResilient(datagram, header, flags);
                }
            }
        }

        /// <inheritdoc cref="SendResilientCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendResilient(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock) SendResilientCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendResilientCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendResilient(in ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendResilientCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendResilientCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendResilient(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            Flags flags = Flags.Get();
            lock (_lock) SendResilientCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendResilientCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendResilient(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            lock (_lock) SendResilientCore(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendResilientCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public void SendResilient(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendResilientCore(datagram, ref header, ref flags);
        }




        /// <summary>
        /// <para>Tries to resiliently send <paramref name="datagram"/> to the server.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// As a bonus: allocate less if target transport doesn't exist.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        /// <inheritdoc cref="SendResilientCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
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

                SendResilientCore(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <inheritdoc cref="TrySendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendResilient(in ReadOnlySpan<byte> datagram)
        {
            if (!HasAnyResilientTransport())
                return false;

            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendResilientCore(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendResilient(in ReadOnlySpan<byte> datagram, ref Header header)
        {
            if (!HasAnyResilientTransport())
                return false;

            Flags flags = Flags.Get();
            lock (_lock) SendResilientCore(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendResilient(in ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            if (!HasAnyResilientTransport())
                return false;

            Header header = Header.Get();
            lock (_lock) SendResilientCore(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendResilient(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public bool TrySendResilient(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
        {
            if (!HasAnyResilientTransport())
                return false;

            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendResilientCore(datagram, ref header, ref flags);
            return true;
        }




        /// <summary>
        /// <para>Resiliently sends <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// Throws is specified transport is not registered.
        /// Use <see cref="TrySendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/> to send message only if <typeparamref name="TTransport"/> is present.
        /// </remarks>
        /// <typeparam name="TTransport"><see cref="IReliableTransport"/> to use for sending of a message.</typeparam>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        protected virtual void SendResilientCore<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IResilientTransport
        {
            using (header.Lock()) using (flags.Lock())
            {
                GetResilientTransport<TTransport>().SendResilient(datagram, header, flags);
            }
        }

        /// <inheritdoc cref="SendResilientCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendResilient<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IResilientTransport
        {
            lock (_lock) SendResilientCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendResilientCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendResilient<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, IResilientTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendResilientCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendResilientCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendResilient<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IResilientTransport
        {
            Flags flags = Flags.Get();
            lock (_lock) SendResilientCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendResilientCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public void SendResilient<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IResilientTransport
        {
            Header header = Header.Get();
            lock (_lock) SendResilientCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="SendResilientCore{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public void SendResilient<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IResilientTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendResilientCore<TTransport>(datagram, ref header, ref flags);
        }

        /// <summary>
        /// <para>Tries to resiliently send <paramref name="datagram"/> to the server using specified <typeparamref name="TTransport"/>.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// As a bonus: allocate less if target transport doesn't exist.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> if there was no <typeparamref name="TTransport"/> registered and nothing was sent.
        /// </returns>
        /// <inheritdoc cref="SendResilientCore(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendResilient<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                if (!ResilientTransports.Has<TTransport>())
                {
                    header.DisposeIfUnlocked();
                    flags.DisposeIfUnlocked();
                    return false;
                }

                SendResilientCore<TTransport>(datagram, ref header, ref flags);
                return true;
            }
        }

        /// <inheritdoc cref="TrySendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendResilient<TTransport>(in ReadOnlySpan<byte> datagram)
            where TTransport : class, IResilientTransport
        {
            if (!HasResilientTransport<TTransport>())
                return false;

            Header header = Header.Get();
            Flags flags = Flags.Get();
            lock (_lock) SendResilientCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendResilient<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IResilientTransport
        {
            if (!HasResilientTransport<TTransport>())
                return false;

            Flags flags = Flags.Get();
            lock (_lock) SendResilientCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySendResilient<TTransport>(in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IResilientTransport
        {
            if (!HasResilientTransport<TTransport>())
                return false;

            Header header = Header.Get();
            lock (_lock) SendResilientCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public bool TrySendResilient<TTransport>(in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup = null, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IResilientTransport
        {
            if (!HasResilientTransport<TTransport>())
                return false;

            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            lock (_lock) SendResilientCore<TTransport>(datagram, ref header, ref flags);
            return true;
        }
        #endregion
    }
}