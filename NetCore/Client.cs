using NetCore.Transports;
using System.Runtime.CompilerServices;

namespace NetCore
{
    /// <summary>
    /// Base class working with different <see cref="ITransport"/>s.
    /// </summary>
    /// <inheritdoc cref="NetworkMember"/>
    /// TODO: Improve parallelism by caching transports of a target type when broadcasting a message.
    /// Note: Consider using <see langword="in"/> on all <see cref="ReadOnlySpan{T}"/> datagrams here.
    public partial class Client(int transports) : NetworkMember<Client>(transports), ISendNetworkMessaging, ITransportBasedSendNetworkMessaging
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
        /// .                                           Additional Send Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// If there are transports that can send the data:
        /// Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// As a bonus: allocate less if target transport doesn't exist.
        /// </remarks>
        /// <seealso cref="SendUnreliable(ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendReliable(ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendSequential(ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendResilient(ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> otherwise.
        /// </returns>
        public bool TrySend(SendingMode mode, ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            switch (mode)
            {
                case SendingMode.Unreliable:
                    if (HasAnyUnreliableTransport())
                    {
                        SendUnreliable(datagram, ref header, ref flags);
                        return true;
                    }
                    return false;

                case SendingMode.Reliable:
                    if (HasAnyReliableTransport())
                    {
                        SendReliable(datagram, ref header, ref flags);
                        return true;
                    }
                    return false;

                case SendingMode.Sequential:
                    if (HasAnySequentialTransport())
                    {
                        SendSequential(datagram, ref header, ref flags);
                        return true;
                    }
                    return false;

                case SendingMode.Resilient:
                    if (HasAnyResilientTransport())
                    {
                        SendReliable(datagram, ref header, ref flags);
                        return true;
                    }
                    return false;

                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <inheritdoc cref="TrySend(SendingMode, ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySend(SendingMode mode, ReadOnlySpan<byte> datagram)
        {
            if (!HasAnyTransport(mode))
                return false;

            Header header = Header.Get();
            Flags flags = Flags.Get();
            this.Send(mode, datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySend(SendingMode, ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySend(SendingMode mode, ReadOnlySpan<byte> datagram, ref Header header)
        {
            if (!HasAnyTransport(mode))
            {
                header.DisposeIfUnlocked();
                return false;
            }

            Flags flags = Flags.Get();
            this.Send(mode, datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySend(SendingMode, ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySend(SendingMode mode, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            if (!HasAnyTransport(mode))
            {
                flags.DisposeIfUnlocked();
                return false;
            }

            Header header = Header.Get();
            this.Send(mode, datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySend(SendingMode, ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="mode"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public bool TrySend(SendingMode mode, ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup, FlagsConstructor? flagsSetup = null)
        {
            if (!HasAnyTransport(mode))
                return false;

            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            this.Send(mode, datagram, ref header, ref flags);
            return true;
        }

        #region General sending methods
        /// <summary>
        /// If there are transports that can send the data:
        /// <para>Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <remarks>
        /// As a bonus: allocate less if target transport doesn't exist.
        /// </remarks>
        /// <seealso cref="SendUnreliable{TTransport}(ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendReliable{TTransport}(ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendSequential{TTransport}(ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="SendResilient{TTransport}(ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <typeparam name="TTransport"><see cref="ITransport"/> to use for sending of a message.</typeparam>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <returns>
        /// <c>true</c> if transport was found and <paramref name="datagram"/> was sent.
        /// <c>false</c> if there was no <typeparamref name="TTransport"/> registered and nothing was sent.
        /// </returns>
        public bool TrySend<TTransport>(SendingMode mode, ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            return mode switch
            {
                SendingMode.Unreliable => this.TrySendUnreliable<TTransport>(datagram, ref header, ref flags),
                SendingMode.Reliable => this.TrySendReliable<TTransport>(datagram, ref header, ref flags),
                SendingMode.Sequential => this.TrySendSequential<TTransport>(datagram, ref header, ref flags),
                SendingMode.Resilient => this.TrySendResilient<TTransport>(datagram, ref header, ref flags),
                _ => throw new SwitchExpressionException(mode),
            };
        }

        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySend<TTransport>(SendingMode mode, ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            if (!HasTransport<TTransport>(mode))
                return false;

            Header header = Header.Get();
            Flags flags = Flags.Get();
            this.Send<TTransport>(mode, datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySend<TTransport>(SendingMode mode, ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            if (!HasTransport<TTransport>(mode))
                return false;

            Flags flags = Flags.Get();
            this.Send<TTransport>(mode, datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public bool TrySend<TTransport>(SendingMode mode, ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            if (!HasTransport<TTransport>(mode))
                return false;

            Header header = Header.Get();
            this.Send<TTransport>(mode, datagram, ref header, ref flags);
            return true;
        }

        /// <inheritdoc cref="TrySend{TTransport}(SendingMode, ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="mode"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public bool TrySend<TTransport>(SendingMode mode, ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            if (!HasTransport<TTransport>(mode))
                return false;

            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            this.Send<TTransport>(mode, datagram, ref header, ref flags);
            return true;
        }
        #endregion

        /// <inheritdoc/>
        public virtual void SendUnreliable(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock)
            {
                header.Lock();
                flags.Lock();
                try
                {
                    foreach (var transport in UnreliableTransports)
                    {
                        transport.SendUnreliable(datagram, in header, in flags);
                    }
                }
                finally
                {
                    header.Dispose();
                    flags.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public virtual void SendReliable(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock)
            {
                header.Lock();
                flags.Lock();
                try
                {
                    foreach (var transport in ReliableTransports)
                    {
                        transport.SendReliable(datagram, in header, in flags);
                    }
                }
                finally
                {
                    header.Dispose();
                    flags.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public virtual void SendSequential(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock)
            {
                header.Lock();
                flags.Lock();
                try
                {
                    foreach (var transport in SequentialTransports)
                    {
                        transport.SendSequential(datagram, in header, in flags);
                    }
                }
                finally
                {
                    header.Dispose();
                    flags.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public virtual void SendResilient(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            lock (_lock)
            {
                header.Lock();
                flags.Lock();
                try
                {
                    foreach (var transport in ResilientTransports)
                    {
                        transport.SendResilient(datagram, in header, in flags);
                    }
                }
                finally
                {
                    header.Dispose();
                    flags.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public virtual void SendUnreliable<TTransport>(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags) where TTransport : class, IUnreliableTransport
        {
            lock (_lock)
            {
                header.Lock();
                flags.Lock();
                try
                {
                    GetUnreliableTransport<TTransport>().SendUnreliable(datagram, in header, in flags);
                }
                finally
                {
                    header.Dispose();
                    flags.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public virtual void SendReliable<TTransport>(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags) where TTransport : class, IReliableTransport
        {
            lock (_lock)
            {
                header.Lock();
                flags.Lock();
                try
                {
                    GetReliableTransport<TTransport>().SendReliable(datagram, in header, in flags);
                }
                finally
                {
                    header.Dispose();
                    flags.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public virtual void SendSequential<TTransport>(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags) where TTransport : class, ISequentialTransport
        {
            lock (_lock)
            {
                header.Lock();
                flags.Lock();
                try
                {
                    GetSequentialTransport<TTransport>().SendSequential(datagram, in header, in flags);
                }
                finally
                {
                    header.Dispose();
                    flags.Dispose();
                }
            }
        }

        /// <inheritdoc/>
        public virtual void SendResilient<TTransport>(scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags) where TTransport : class, IResilientTransport
        {
            lock (_lock)
            {
                header.Lock();
                flags.Lock();
                try
                {
                    GetResilientTransport<TTransport>().SendResilient(datagram, in header, in flags);
                }
                finally
                {
                    header.Dispose();
                    flags.Dispose();
                }
            }
        }
    }
}