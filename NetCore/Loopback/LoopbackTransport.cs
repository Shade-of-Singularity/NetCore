using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.InteropServices;

namespace NetCore.Loopback
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
        public override void Connect(IPEndPoint remoteEndPoint)
        {
            base.Connect(remoteEndPoint);
            if (IsServerSide)
            {
                return;
            }

            if (!Servers.TryGet(remoteEndPoint, out var server))
            {
                throw new KeyNotFoundException($"ClientData-side {nameof(LoopbackTransport)} cannot find an active server under a port ({remoteEndPoint.Port}).");
            }

            if (!server.TryGetReliableTransport(out LoopbackTransport? remote))
            {
                return;
            }

            lock (m_Loopbacks)
            {
                lock (remote.m_Loopbacks)
                {
                    ConnectionID sourceID = Holder!.CIDProvider.NextCID();
                    ConnectionID remoteID = server.CIDProvider.NextCID();
                    remote.m_Loopbacks[remoteID] = new(this, sourceID);
                    m_Loopbacks[sourceID] = new(remote, remoteID);
                }
            }
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
        public void SendReliable(HeaderReader header, ReadOnlySpan<byte> datagram)
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
        public void SendReliableExcluding(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID toExclude)
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
        public void SendReliableTo(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID target)
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
        public void HandleReliable(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID source)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(HandleReliable)}(sourceID: ({source}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <inheritdoc/>
        public void SendUnreliable(HeaderReader header, ReadOnlySpan<byte> datagram)
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
        public void SendUnreliableExcluding(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID toExclude)
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
        public void SendUnreliableTo(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID target)
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
        public void HandleUnreliable(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID source)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(HandleUnreliable)}(sourceID: ({source}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
