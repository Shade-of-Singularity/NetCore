namespace NetCore.Transports
{
    /// <summary>
    /// Base class which implements required functionality from <see cref="ITransport"/>s.
    /// </summary>
    /// <remarks>
    /// In arrays and dictionaries only <see cref="ITransport"/> is used.
    /// <see cref="Transport"/> only defined required fields, methods and event himself, so you don't need to.
    /// </remarks>
    public abstract class Transport : ITransport
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Public Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc/>
        public virtual AsyncMode SupportedStartAsyncModes => AsyncMode.Synced;

        /// <inheritdoc/>
        public virtual AsyncMode SupportedConnectionAsyncModes => AsyncMode.Synced;

        /// <inheritdoc/>
        public bool IsServerSide { get; private set; }

        /// <inheritdoc/>
        public bool IsClientSide => !IsServerSide;

        /// <inheritdoc/>
        public virtual bool IsConnected { get; protected set; }

        /// <inheritdoc cref="ITransport.IsAttached"/>
        public bool IsAttached { get; private set; }
        bool ITransport.IsAttached
        {
            get => IsAttached;
            set => IsAttached = value;
        }

        /// <inheritdoc cref="ITransport.Holder"/>
        public NetworkMember? Holder { get; private set; }
        NetworkMember? ITransport.Holder
        {
            get => Holder;
            set => Holder = value;
        }

        /// <summary>
        /// <see cref="NetCore.Server"/> instance casted from <see cref="Holder"/> on <see cref="Attach(NetworkMember)"/>.
        /// </summary>
        /// <remarks>
        /// <c>null</c> when <see cref="IsServerSide"/> is <c>false</c>.
        /// </remarks>
        public Server? Server { get; private set; } = default!;
        /// <summary>
        /// <see cref="NetCore.Client"/> instance casted from <see cref="Holder"/> on <see cref="Attach(NetworkMember)"/>.
        /// </summary>
        /// <remarks>
        /// <c>null</c> when <see cref="IsServerSide"/> is <c>true</c>.
        /// </remarks>
        public Client? Client { get; private set; } = default!;

        /// <inheritdoc cref="ITransport.Holder"/>
        public MemberState State
        {
            get
            {
                lock (_lock) return m_State;
            }
        }
        MemberState ITransport.State
        {
            get => m_State;
            set => m_State = value;
        }

        object ITransport.Lock => _lock;
        CancellationTokenSource? ITransport.StartTokenSource
        {
            get => m_StartTokenSource;
            set => m_StartTokenSource = value;
        }
        OperationResultTask ITransport.StartOperation
        {
            get => m_StartOperation;
            set => m_StartOperation = value;
        }
        CancellationTokenSource? ITransport.StopTokenSource
        {
            get => m_StopTokenSource;
            set => m_StopTokenSource = value;
        }
        CancellationTokenSource? ITransport.ConnectionTokenSource
        {
            get => m_ConnectionTokenSource;
            set => m_ConnectionTokenSource = value;
        }
        OperationResultTask ITransport.ConnectionOperation
        {
            get => m_ConnectionTask;
            set => m_ConnectionTask = value;
        }
        CancellationTokenSource? ITransport.DisconnectionTokenSource
        {
            get => m_DisconnectionTokenSource;
            set => m_DisconnectionTokenSource = value;
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Protected Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Lock, used when interacting with internal items and a state machine.
        /// </summary>
        protected readonly object _lock = new();




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private CancellationTokenSource? m_StartTokenSource;
        private OperationResultTask m_StartOperation = StateMachineHelpers.CompletedTask;
        private CancellationTokenSource? m_StopTokenSource;
        private CancellationTokenSource? m_ConnectionTokenSource;
        private OperationResultTask m_ConnectionTask = StateMachineHelpers.CompletedTask;
        private CancellationTokenSource? m_DisconnectionTokenSource;
        private volatile MemberState m_State;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc/>
        public abstract bool HasConnection(ConnectionID connection);

        /// <inheritdoc cref="ITransport.Attach"/>
        /// <remarks>
        /// Managed by a <c>try</c> wrapper. Feel free to return <see cref="System.Exception"/>s if you need to.
        /// </remarks>
        public virtual void Attach(NetworkMember member)
        {
            IsServerSide = member.ResolveInitializer(out Server? server, out Client? client);
            Holder = member;
            Server = server;
            Client = client;
        }

        /// <inheritdoc cref="ITransport.Detach"/>
        /// <remarks>
        /// Managed by a <c>try</c> wrapper. Feel free to return <see cref="System.Exception"/>s if you need to.
        /// </remarks>
        public virtual void Detach(NetworkMember member)
        {
            IsServerSide = default;
            Holder = null;
            Server = null;
            Client = null;
        }

        /// <inheritdoc cref="ITransport.Start"/>
        /// <remarks>
        /// Managed by a <c>try</c> wrapper. Feel free to return <see cref="System.Exception"/>s if you need to.
        /// </remarks>
        public virtual AsyncTask Start(IReadOnlyStartupArgs args, CancellationToken token)
        {
            // Nothing here right now, but might be something in the future.
            return AsyncTask.CompletedTask;
        }

        /// <inheritdoc cref="ITransport.Stop"/>
        /// <remarks>
        /// Managed by a <c>try</c> wrapper. Feel free to return <see cref="System.Exception"/>s if you need to.
        /// </remarks>
        public virtual AsyncTask Stop()
        {
            // Nothing here right now, but might be something in the future.
            return AsyncTask.CompletedTask;
        }

        /// <inheritdoc cref="ITransport.Connect"/>
        /// <remarks>
        /// Managed by a <c>try</c> wrapper. Feel free to return <see cref="System.Exception"/>s if you need to.
        /// </remarks>
        public virtual AsyncTask Connect(IReadOnlyConnectionArgs args, CancellationToken token)
        {
            // Nothing here right now, but might be something in the future.
            IsConnected = true;
            return AsyncTask.CompletedTask;
        }

        /// <inheritdoc cref="ITransport.Disconnect"/>
        /// <remarks>
        /// Managed by a <c>try</c> wrapper. Feel free to return <see cref="System.Exception"/>s if you need to.
        /// </remarks>
        public virtual AsyncTask Disconnect()
        {
            // Nothing here right now, but might be something in the future.
            IsConnected = false;
            return AsyncTask.CompletedTask;
        }
    }
}
