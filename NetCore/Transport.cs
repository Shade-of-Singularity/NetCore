using System.Net;

namespace NetCore
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
        public bool IsServerSide { get; private set; }

        /// <inheritdoc/>
        public bool IsClientSide => !IsServerSide;

        /// <inheritdoc cref="ITransport.IsInitialized"/>
        public bool IsInitialized { get; private set; }
        bool ITransport.IsInitialized
        {
            get => IsInitialized;
            set => IsInitialized = value;
        }

        /// <inheritdoc cref="ITransport.IsActive"/>
        public bool IsActive { get; private set; }
        bool ITransport.IsActive
        {
            get => IsActive;
            set => IsActive = value;
        }

        /// <inheritdoc/>
        public NetworkMember? Holder { get; private set; }

        /// <summary>
        /// End-point on which this transports is running.
        /// </summary>
        /// <remarks>
        /// Might differ from the one provided in <see cref="ITransport.Start(IPEndPoint)"/> if this transport doesn't use default port.
        /// </remarks>
        public IPEndPoint? LocalEndPoint { get; protected set; }

        /// <summary>
        /// Remote end-point to which transport was requested to connect to.
        /// </summary>
        /// <remarks>
        /// When used with <see cref="IsServerSide"/> - represent an <see cref="IPEndPoint"/> of a relay server.
        /// </remarks>
        public IPEndPoint? RemoteEndPoint { get; private set; }

        /// <summary>
        /// <see cref="NetCore.Server"/> instance casted from <see cref="Holder"/> on <see cref="Initialize(NetworkMember)"/>.
        /// </summary>
        /// <remarks>
        /// <c>null</c> when <see cref="IsServerSide"/> is <c>false</c>.
        /// </remarks>
        public Server? Server { get; private set; } = default!;

        /// <summary>
        /// <see cref="NetCore.Client"/> instance casted from <see cref="Holder"/> on <see cref="Initialize(NetworkMember)"/>.
        /// </summary>
        /// <remarks>
        /// <c>null</c> when <see cref="IsServerSide"/> is <c>true</c>.
        /// </remarks>
        public Client? Client { get; private set; } = default!;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc/>
        public abstract bool HasCID(ulong CID);

        /// <inheritdoc cref="ITransport.Initialize"/>
        /// <remarks>
        /// Managed by a <c>try</c> wrapper. Feel free to return <see cref="System.Exception"/>s if you need to.
        /// </remarks>
        public virtual void Initialize(NetworkMember member)
        {
            IsServerSide = TransportHelpers.ResolveInitializer(Holder = member, out Server? server, out Client? client);
            Server = server;
            Client = client;
        }

        /// <inheritdoc cref="ITransport.Terminate"/>
        /// <remarks>
        /// Managed by a <c>try</c> wrapper. Feel free to return <see cref="System.Exception"/>s if you need to.
        /// </remarks>
        public virtual void Terminate(NetworkMember member)
        {
            Holder = null;
            Server = null;
            Client = null;
        }

        /// <inheritdoc cref="ITransport.Start"/>
        /// <remarks>
        /// Managed by a <c>try</c> wrapper. Feel free to return <see cref="System.Exception"/>s if you need to.
        /// </remarks>
        public virtual void Start(IPEndPoint localEndPoint)
        {
            LocalEndPoint = localEndPoint;
        }

        /// <inheritdoc cref="ITransport.Stop"/>
        /// <remarks>
        /// Managed by a <c>try</c> wrapper. Feel free to return <see cref="System.Exception"/>s if you need to.
        /// </remarks>
        public virtual void Stop()
        {
            LocalEndPoint = null;
            RemoteEndPoint = null;
        }

        /// <inheritdoc cref="ITransport.Connect"/>
        /// <remarks>
        /// Managed by a <c>try</c> wrapper. Feel free to return <see cref="System.Exception"/>s if you need to.
        /// </remarks>
        public virtual void Connect(IPEndPoint remoteEndPoint)
        {
            RemoteEndPoint = remoteEndPoint;
        }

        /// <inheritdoc cref="ITransport.Disconnect"/>
        /// <remarks>
        /// Managed by a <c>try</c> wrapper. Feel free to return <see cref="System.Exception"/>s if you need to.
        /// </remarks>
        public virtual void Disconnect()
        {
            RemoteEndPoint = null;
        }
    }
}
