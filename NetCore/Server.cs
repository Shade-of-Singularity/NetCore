using Cysharp.Threading.Tasks;
using NetCore.Transports;
using System;
using System.Threading;

namespace NetCore
{
    /// <summary>
    /// Base class working with different <see cref="ITransport"/>s.
    /// </summary>
    /// <inheritdoc cref="NetworkMember"/>
    /// TODO: Consider adding a check for 0 transports being present.
    public partial class Server(int transports) : NetworkMember<Server>(transports),
        ISendNetworkMessaging, ISendToNetworkMessaging, ISendExcludingNetworkMessaging,
        ITransportBasedSendNetworkMessaging, ITransportBasedSendToNetworkMessaging, ITransportBasedSendExcludingNetworkMessaging
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
        /// .                                                Send Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc/>
        public virtual void SendUnreliable(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
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
        public void SendReliable(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
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
        public void SendSequential(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
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
        public void SendResilient(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
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
        public void SendUnreliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags) where TTransport : class, IUnreliableTransport
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
        public void SendReliable<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags) where TTransport : class, IReliableTransport
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
        public void SendSequential<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags) where TTransport : class, ISequentialTransport
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
        public void SendResilient<TTransport>(in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags) where TTransport : class, IResilientTransport
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
