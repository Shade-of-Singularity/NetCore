using System;
using System.Net;

namespace NetCore
{
    /// <summary>
    /// Base interface for all kinds of transports.
    /// </summary>
    /// <remarks>
    /// Transport manages both re-sending of a message, and a handshake process.
    /// This is needed to allow custom handshaking for different transports types, where one relies on IP, and the other - on a UID.
    /// </remarks>
    /// TODO: Add a way to define custom transports.
    /// TODO: Add a way to define custom sending modes (Riptide's Notify, etc.)
    public interface ITransport
    {
        /// <summary>
        /// Whether this transport is server-side (true) or client-side (false).
        /// </summary>
        /// <remarks>
        /// <see cref="ITransport"/>s that implement only server-side or only client-side logic
        /// might choose to throw if you attach them to an unsupported <see cref="NetworkMember"/>.
        /// </remarks>
        public bool IsServerSide { get; }

        /// <summary>
        /// Whether this transport is client-side (true) or server-side (false).
        /// </summary>
        /// <remarks>
        /// <see cref="ITransport"/>s that implement only server-side or only client-side logic
        /// might choose to throw if you attach them to an unsupported <see cref="NetworkMember"/>.
        /// </remarks>
        public bool IsClientSide => !IsServerSide;

        /// <summary>
        /// <see cref="ITransport"/> holder which manages this <see cref="ITransport"/> instance.
        /// </summary>
        public NetworkMember? Holder { get; }

        /// <summary>
        /// Local end-point on which this <see cref="ITransport"/> was started.
        /// </summary>
        /// <remarks>
        /// UID-based transports might choose to use different local port than the one provided in a <see cref="Start(IPEndPoint)"/> method.
        /// </remarks>
        public IPEndPoint? LocalEndPoint { get; }

        /// <summary>
        /// Remote end-point to which this <see cref="ITransport"/> was requested to connect to.
        /// </summary>
        /// <remarks>
        /// When used with <see cref="IsServerSide"/> - represents a remote <see cref="IPEndPoint"/> of a relay server.
        /// </remarks>
        public IPEndPoint? RemoteEndPoint { get; }

        /// <summary>
        /// Whether this <see cref="ITransport"/> is initialized or not.
        /// </summary>
        public bool IsInitialized { get; protected set; }

        /// <summary>
        /// Whether this <see cref="ITransport"/> was activated with <see cref="Start"/> or not.
        /// </summary>
        public bool IsActive { get; protected set; }

        /// <summary>
        /// Initializes the <see cref="ITransport"/>.
        /// Called when transport is registered in a <see cref="NetworkMember"/>.
        /// </summary>
        /// <param name="member">Network member which initializes this <see cref="ITransport"/>.</param>
        /// <returns>
        /// <c>true</c> if started successfully.
        /// <c>false</c> if any issues appeared at initialization (see console for more info).
        /// </returns>
        public bool InvokeInitialize(NetworkMember member)
        {
            if (!IsInitialized)
            {
                try
                {
                    Initialize(member);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(exception);
                    Console.ForegroundColor = ConsoleColor.White;

                    try
                    {
                        Terminate(member);
                    }
                    catch (Exception inner)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(inner);
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    return false;
                }

                IsInitialized = true;
            }

            return true;
        }

        /// <summary>
        /// Terminates the <see cref="ITransport"/>.
        /// Called when transport is detached from a <see cref="NetworkMember"/>.
        /// </summary>
        /// <param name="member"><see cref="NetworkMember"/> which previously initialized this <see cref="ITransport"/>.</param>
        /// <returns>
        /// <c>true</c> if started successfully.
        /// <c>false</c> if any issues appeared at termination (see console for more info).
        /// </returns>
        public bool InvokeTerminate(NetworkMember member)
        {
            if (IsInitialized && Holder == member)
            {
                try
                {
                    Terminate(member);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(exception);
                    Console.ForegroundColor = ConsoleColor.White;
                    return false;
                }

                IsInitialized = false;
            }

            return true;
        }

        /// <summary>
        /// Starts transport on a provided <paramref name="endPoint"/>.
        /// Transports which rely on UID, like SteamNetworking UDP transport, can choose to use a different port than a provided one.
        /// </summary>
        /// <param name="endPoint"><see cref="IPEndPoint"/> provided when <see cref="NetworkMember"/> was starting.</param>
        /// <returns>
        /// <c>true</c> if started successfully.
        /// <c>false</c> if any issues appeared at starting (see console for more info).
        /// </returns>
        public bool InvokeStart(IPEndPoint endPoint)
        {
            if (!IsActive)
            {
                try
                {
                    Start(endPoint);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(exception);
                    Console.ForegroundColor = ConsoleColor.White;

                    try
                    {
                        Stop();
                    }
                    catch (Exception inner)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(inner);
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    return false;
                }

                IsActive = true;
            }

            return true;
        }

        /// <summary>
        /// Stops the transport, disposes the data and unbinds it.
        /// </summary>
        /// <returns>
        /// <c>true</c> if started successfully.
        /// <c>false</c> if any issues appeared at stopping (see console for more info).
        /// </returns>
        public bool InvokeStop()
        {
            if (IsActive)
            {
                try
                {
                    Stop();
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(exception);
                    Console.ForegroundColor = ConsoleColor.White;
                    return false;
                }

                IsActive = false;
            }

            return true;
        }

        /// <summary>
        /// Attempts to connect to a <paramref name="remoteEndPoint"/>.
        /// </summary>
        /// <remarks>
        /// When used with <see cref="IsServerSide"/> - connects to a remote relay and manages NAT hole if needed.
        /// </remarks>
        /// <param name="remoteEndPoint">Remote <see cref="IPEndPoint"/> to connect to.</param>
        /// <returns>
        /// <c>true</c> if connection started successfully.
        /// <c>false</c> if already connected or any issues appeared during connection attempt (see console for more info).
        /// </returns>
        /// TODO: Maybe return awaitable UniTask or a ValueTask?
        /// TODO: Add a way to specify which transport sets expect to fire (TCP+UDP, SteamUDP-only, TCP-only, UDP-only, Pipe-only, etc).
        public bool InvokeConnect(IPEndPoint remoteEndPoint)
        {
            if (RemoteEndPoint is null)
            {
                try
                {
                    Connect(remoteEndPoint);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(exception);
                    Console.ForegroundColor = ConsoleColor.White;

                    try
                    {
                        Disconnect();
                    }
                    catch (Exception inner)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.WriteLine(inner);
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Disconnects from current remote <see cref="IPEndPoint"/>.
        /// </summary>
        /// <remarks>
        /// When used with <see cref="IsServerSide"/> - disconnects from a remote relay.
        /// </remarks>
        /// <returns>
        /// <c>true</c> if disconnected successfully.
        /// <c>false</c> if any issues appeared during disconnection (see console for more info).
        /// </returns>
        public bool InvokeDisconnect()
        {
            if (RemoteEndPoint is not null)
            {
                try
                {
                    Disconnect();
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(exception);
                    Console.ForegroundColor = ConsoleColor.White;
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc cref="InvokeInitialize"/>
        protected void Initialize(NetworkMember member);

        /// <inheritdoc cref="InvokeTerminate"/>
        protected void Terminate(NetworkMember member);

        /// <inheritdoc cref="InvokeStart"/>
        protected void Start(IPEndPoint localEndPoint);

        /// <inheritdoc cref="InvokeStop"/>
        protected void Stop();

        /// <inheritdoc cref="InvokeConnect"/>
        protected void Connect(IPEndPoint remoteEndPoint);

        /// <inheritdoc cref="InvokeDisconnect"/>
        protected void Disconnect();

        /// <summary>
        /// Checks if <see cref="ITransport"/> manages a specific <paramref name="connection"/> at the moment.
        /// </summary>
        /// <param name="connection">Connection ID to check for.</param>
        /// <returns><c>true</c> if this <see cref="ITransport"/> manages given connection. <c>false</c> otherwise.</returns>
        /// Note: There is no guarantee that connection #0 client-side is a server/host connection.
        public bool HasConnection(ConnectionID connection);
    }
}
