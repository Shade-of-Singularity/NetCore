using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace NetCore.Transports.Loopback
{
    /// <summary>
    /// Implements native loopback connection, without overhead of any in-between systems, like TCP or UDP.
    /// Useful if you rely on client sending messages to itself at development time.
    /// </summary>
    public class LoopbackTransport : Transport, IReliableTransport, IUnreliableTransport
    {
        private readonly record struct LoopbackEntry(LoopbackTransport Transport, ConnectionID RemoteCID)
        {
            /// <inheritdoc/>
            public override int GetHashCode() => Transport.GetHashCode();
        }


        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private readonly Dictionary<ConnectionID, LoopbackEntry> m_Loopbacks = [];




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Implementations
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc/>
        public override UniTask Connect(IReadOnlyConnectionArgs args, CancellationToken token)
        {
            base.Connect(args, token);
            if (IsServerSide)
            {
                return UniTask.CompletedTask;
            }

            var ep = args.RemoteIPEndPoint;
            if (ep is null) return UniTask.CompletedTask;
            if (!Servers.TryGet(ep, out var server))
            {
                throw new KeyNotFoundException($"ClientData-side {nameof(LoopbackTransport)} cannot find an active server under a port ({ep.Port}).");
            }

            if (!server.TryGetReliableTransport(out LoopbackTransport? remote))
            {
                return UniTask.CompletedTask;
            }

            lock (m_Loopbacks)
            {
                lock (remote.m_Loopbacks)
                {
                    ConnectionID sourceID = Holder!.ConnectionIDProvider.NextCID();
                    ConnectionID remoteID = server.ConnectionIDProvider.NextCID();
                    remote.m_Loopbacks[remoteID] = new(this, sourceID);
                    m_Loopbacks[sourceID] = new(remote, remoteID);
                }
            }

            return UniTask.CompletedTask;
        }

        /// <inheritdoc/>
        public override void Disconnect()
        {
            lock (m_Loopbacks)
            {
                foreach (var entry in m_Loopbacks)
                {
                    var remoteLoopbacks = entry.Value.Transport.m_Loopbacks;
                    lock (remoteLoopbacks)
                    {
                        remoteLoopbacks.Remove(entry.Key);
                    }
                }

                m_Loopbacks.Clear();
            }

            base.Disconnect();
        }

        /// <inheritdoc/>
        public override bool HasConnection(ConnectionID connection)
        {
            lock (m_Loopbacks)
            {
                return m_Loopbacks.ContainsKey(connection);
            }
        }

        /// <inheritdoc/>
        public void SendReliable(in Header header, ReadOnlySpan<byte> datagram)
        {
#if DEBUG
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendReliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
            lock (m_Loopbacks)
            {
                foreach (var entry in m_Loopbacks)
                {
                    entry.Value.Transport.HandleReliable(header, datagram, entry.Value.RemoteCID);
                }
            }
        }

        /// <inheritdoc/>
        public void SendReliableExcluding(in Header header, ReadOnlySpan<byte> datagram, ConnectionID toExclude)
        {
#if DEBUG
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendReliableExcluding)}(exclude: ({toExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
            lock (m_Loopbacks)
            {
                foreach (var entry in m_Loopbacks)
                {
                    if (entry.Key != toExclude)
                    {
                        entry.Value.Transport.HandleReliable(header, datagram, entry.Value.RemoteCID);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void SendReliableTo(in Header header, ReadOnlySpan<byte> datagram, ConnectionID target)
        {
#if DEBUG
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendReliableTo)}(target: ({target}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
            lock (m_Loopbacks)
            {
                if (m_Loopbacks.TryGetValue(target, out LoopbackEntry entry))
                {
                    entry.Transport.HandleReliable(header, datagram, target);
                }
            }
        }

        /// <inheritdoc/>
        public void HandleReliable(in Header header, ReadOnlySpan<byte> datagram, ConnectionID source)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(HandleReliable)}(sourceID: ({source}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <inheritdoc/>
        public void SendUnreliable(in Header header, ReadOnlySpan<byte> datagram)
        {
#if DEBUG
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendUnreliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
            lock (m_Loopbacks)
            {
                foreach (var entry in m_Loopbacks)
                {
                    entry.Value.Transport.HandleUnreliable(header, datagram, entry.Value.RemoteCID);
                }
            }
        }

        /// <inheritdoc/>
        public void SendUnreliableExcluding(in Header header, ReadOnlySpan<byte> datagram, ConnectionID toExclude)
        {
#if DEBUG
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendUnreliableExcluding)}(exclude: ({toExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
            lock (m_Loopbacks)
            {
                foreach (var entry in m_Loopbacks)
                {
                    if (entry.Key != toExclude)
                    {
                        entry.Value.Transport.HandleUnreliable(header, datagram, entry.Value.RemoteCID);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(in Header header, ReadOnlySpan<byte> datagram, ConnectionID target)
        {
#if DEBUG
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendUnreliableTo)}(target: ({target}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
            lock (m_Loopbacks)
            {
                if (m_Loopbacks.TryGetValue(target, out LoopbackEntry entry))
                {
                    entry.Transport.HandleUnreliable(header, datagram, target);
                }
            }
        }

        /// <inheritdoc/>
        public void HandleUnreliable(in Header header, ReadOnlySpan<byte> datagram, ConnectionID source)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(HandleUnreliable)}(sourceID: ({source}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <inheritdoc/>
        public void SendReliableTo(in Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(in Header header, ReadOnlySpan<byte> datagram, ConnectionArgs args)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void HandleUnreliable(in Header header, ReadOnlySpan<byte> datagram, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void HandleReliable(in Header header, ReadOnlySpan<byte> datagram, ConnectionArgs source)
        {
            throw new NotImplementedException();
        }
    }
}
