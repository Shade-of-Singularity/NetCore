using Cysharp.Threading.Tasks;
using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace NetCore.Transports
{
    /// <summary>
    /// Base interface for all kinds of transports.
    /// </summary>
    /// <remarks>
    /// Transport manages both re-sending of a message, and a handshake process.
    /// This is needed to allow custom handshaking for different transports types, where one relies on IP, and the other - on a UID.
    /// <para>
    /// <see cref="ITransport"/> supports manual usage outside of <see cref="NetworkMember"/>,
    /// BUT if you do - do NOT mix the uses! Do NOT use <see cref="ITransport"/> manually after registering it!
    /// (For debugging:) You can check if it is registered anywhere by checking if <see cref="Holder"/> is assigned.
    /// But good architecture should not create a situation where you need to check for it.
    /// Just completely separate the usages - manually managed transports should be stored completely outside of an <see cref="NetworkMember"/>.
    /// </para>
    /// </remarks>
    /// TODO: Add a way to define custom transports.
    /// TODO: Add a way to define custom sending modes (Riptide's Notify, etc.)
    public partial interface ITransport
    {
        /// <summary>
        /// Which <see cref="AsyncMode"/>s does <see cref="InvokeStart"/> supports.
        /// </summary>
        /// <remarks>
        /// <see cref="AsyncMode.Synced"/> assumed to be always supported.
        /// </remarks>
        public AsyncMode SupportedStartAsyncModes { get; }
        /// <summary>
        /// Which <see cref="AsyncMode"/>s does <see cref="InvokeConnect"/> supports.
        /// </summary>
        /// <remarks>
        /// <see cref="AsyncMode.Synced"/> assumed to be always supported.
        /// </remarks>
        public AsyncMode SupportedConnectionAsyncModes { get; }
        /// <summary>
        /// Whether this transport right now used as server-side (true) or client-side (false) transport.
        /// </summary>
        /// <remarks>
        /// <see cref="ITransport"/>s that implement only server-side or only client-side logic
        /// might choose to throw if you attach them to an unsupported <see cref="NetworkMember"/>.
        /// </remarks>
        public bool IsServerSide { get; }
        /// <summary>
        /// Whether this transport right now used as client-side (true) or server-side (false) transport.
        /// </summary>
        /// <remarks>
        /// <see cref="ITransport"/>s that implement only server-side or only client-side logic
        /// might choose to throw if you attach them to an unsupported <see cref="NetworkMember"/>.
        /// </remarks>
        public bool IsClientSide => !IsServerSide;
        /// <summary>
        /// Current <see cref="NetworkMember"/> which manages this <see cref="ITransport"/> instance.
        /// </summary>
        public NetworkMember? Holder { get; protected set; }
        /// <summary>
        /// Whether this <see cref="ITransport"/> is initialized or not.
        /// </summary>
        public bool IsAttached { get; protected set; }
        /// <summary>
        /// Whether this transport is actively communicating with a remote host.
        /// </summary>
        public bool IsConnected { get; }
        /// <summary>
        /// Current state of the <see cref="NetworkMember"/>.
        /// </summary>
        public StartupState StartupState
        {
            get
            {
                MemberState state;
                lock (Lock) state = State;
                return state switch
                {
                    MemberState.Stopped => StartupState.Stopped,
                    MemberState.Starting => StartupState.Starting,
                    MemberState.Stopping => StartupState.Stopping,

                    MemberState.Started_Idle => StartupState.Started,
                    MemberState.Started_Connecting => StartupState.Started,
                    MemberState.Started_Disconnecting => StartupState.Started,
                    MemberState.Started_Connected => StartupState.Started,

                    _ => throw new SwitchExpressionException(state),
                };
            }
        }
        /// <summary>
        /// Current state of the connection.
        /// </summary>
        /// <remarks>
        /// If there are no transports registered - <see cref="NetworkMember"/> almost immediately
        /// marked as <see cref="ConnectionState.Connected"/> on <see cref="Connect"/> method call.
        /// Otherwise - marked as <see cref="ConnectionState.Connected"/> when at least one transport is connected.
        /// </remarks>
        public ConnectionState ConnectionState
        {
            get
            {
                MemberState state;
                lock (Lock) state = State;
                return state switch
                {
                    MemberState.Stopped => ConnectionState.Idle,
                    MemberState.Starting => ConnectionState.Idle,
                    MemberState.Stopping => ConnectionState.Idle,
                    MemberState.Started_Idle => ConnectionState.Idle,

                    MemberState.Started_Connecting => ConnectionState.Connecting,
                    MemberState.Started_Disconnecting => ConnectionState.Disconnecting,
                    MemberState.Started_Connected => IsConnected ? ConnectionState.Connected : ConnectionState.Connecting,

                    _ => throw new SwitchExpressionException(state),
                };
            }
        }
        /// <summary>
        /// Attaches this <see cref="ITransport"/> to a <see cref="NetworkMember"/>.
        /// </summary>
        /// <remarks>
        /// Operation cannot be performed if <see cref="ITransport"/> is already started.
        /// </remarks>
        /// <param name="member"><see cref="NetworkMember"/> to which to attach this <see cref="ITransport"/> instance.</param>
        /// <returns>
        /// <c>true</c> if attached successfully.
        /// <c>false</c> if any issues appeared at initialization (see console for more info).
        /// </returns>
        /// <exception cref="InvalidOperationException">Attempted to attach this <see cref="ITransport"/> after starting it.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="member"/> is <see langword="null"/>.</exception>
        public bool InvokeAttach(NetworkMember member)
        {
            if (member is null)
                throw new ArgumentNullException(nameof(member));

            lock (Lock)
            {
                if (State != MemberState.Stopped)
                {
                    throw new InvalidOperationException($"Cannot attach an {GetType().Name} while it is active!");
                }

                if (!IsAttached)
                {
                    try
                    {
                        Attach(member);
                        Holder = member;
                    }
                    catch (Exception exception)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        if (exception is SocketException se)
                            Console.WriteLine($"SocketException Code: {se.ErrorCode}");
                        Console.WriteLine(exception);
                        Console.ForegroundColor = ConsoleColor.White;

                        try
                        {
                            Detach(member);
                            Holder = null;
                        }
                        catch (Exception inner)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkRed;
                            if (exception is SocketException se2)
                                Console.WriteLine($"SocketException Code: {se2.ErrorCode}");
                            Console.WriteLine(inner);
                            Console.ForegroundColor = ConsoleColor.White;
                        }

                        return false;
                    }

                    IsAttached = true;
                }

                return true;
            }
        }

        /// <summary>
        /// Detaches this <see cref="ITransport"/> from a current <see cref="NetworkMember"/>.
        /// </summary>
        /// <remarks>
        /// Operation cannot be performed if <see cref="ITransport"/> is already started.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if detached successfully.
        /// <c>false</c> if any issues appeared at detaching (see console for more info).
        /// </returns>
        public bool InvokeDetach()
        {
            lock (Lock)
            {
                if (State != MemberState.Stopped)
                {
                    throw new InvalidOperationException($"Cannot detach an {GetType().Name} while it is active!");
                }

                if (IsAttached)
                {
                    if (Holder is null)
                    {
                        throw new NullReferenceException($"Reference to a parent {nameof(NetworkMember)} in an attached {nameof(ITransport)} is missing!");
                    }

                    try
                    {
                        Detach(Holder);
                        Holder = null;
                    }
                    catch (Exception exception)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        if (exception is SocketException se)
                            Console.WriteLine($"SocketException Code: {se.ErrorCode}");
                        Console.WriteLine(exception);
                        Console.ForegroundColor = ConsoleColor.White;
                        return false;
                    }

                    IsAttached = false;
                }

                return true;
            }
        }

        /// <inheritdoc cref="InvokeAttach"/>
        protected void Attach(NetworkMember member);

        /// <inheritdoc cref="InvokeDetach"/>
        protected void Detach(NetworkMember member);

        /// <inheritdoc cref="InvokeStart"/>
        protected UniTask Start(IReadOnlyStartupArgs args, CancellationToken token);

        /// <inheritdoc cref="InvokeStop"/>
        protected UniTask Stop();

        /// <inheritdoc cref="InvokeConnect"/>
        protected UniTask Connect(IReadOnlyConnectionArgs args, CancellationToken token);

        /// <inheritdoc cref="InvokeDisconnect"/>
        protected UniTask Disconnect();

        /// <summary>
        /// Checks if <see cref="ITransport"/> manages a specific <paramref name="connection"/> at the moment.
        /// </summary>
        /// <param name="connection">Connection ID to check for.</param>
        /// <returns><c>true</c> if this <see cref="ITransport"/> manages given connection. <c>false</c> otherwise.</returns>
        /// Note: There is no guarantee that connection #0 client-side is a server/host connection.
        public bool HasConnection(ConnectionID connection);
    }
}
