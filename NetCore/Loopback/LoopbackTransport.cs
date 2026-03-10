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
        private readonly record struct LoopbackEntry(LoopbackTransport Transport, ulong RemoteCID)
        {
            /// <inheritdoc/>
            public override int GetHashCode() => Transport.GetHashCode();
        }


        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private readonly Dictionary<ulong, LoopbackEntry> m_Loopbacks = [];




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
                throw new KeyNotFoundException($"Client-side {nameof(LoopbackTransport)} cannot find an active server under a port ({remoteEndPoint.Port}).");
            }

            lock (m_Loopbacks)
            {
                var remoteLoopbacks = server.LoopbackTransport;
                lock (remoteLoopbacks.m_Loopbacks)
                {
                    ulong sourceCID = Holder!.CIDProvider.NextCID();
                    ulong remoteCID = server.CIDProvider.NextCID();
                    remoteLoopbacks.m_Loopbacks[remoteCID] = new(this, sourceCID);
                    m_Loopbacks[sourceCID] = new(remoteLoopbacks, remoteCID);
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
        public override bool HasCID(ulong CID)
        {
            lock (m_Loopbacks)
            {
                return m_Loopbacks.ContainsKey(CID);
            }
        }

        /// <inheritdoc/>
        public void SendReliable(ReadOnlySpan<byte> datagram)
        {
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendReliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            lock (m_Loopbacks)
            {
                foreach (var entry in m_Loopbacks)
                {
                    entry.Value.Transport.HandleReliable(datagram, entry.Value.RemoteCID);
                }
            }
        }

        /// <inheritdoc/>
        public void SendReliableExclusive(ReadOnlySpan<byte> datagram, ulong CIDToExclude)
        {
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendReliableExclusive)}(exclude: ({CIDToExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            lock (m_Loopbacks)
            {
                foreach (var entry in m_Loopbacks)
                {
                    if (entry.Key != CIDToExclude)
                    {
                        entry.Value.Transport.HandleReliable(datagram, entry.Value.RemoteCID);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void SendReliableTo(ReadOnlySpan<byte> datagram, ulong targetCID)
        {
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendReliableTo)}(target: ({targetCID}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            lock (m_Loopbacks)
            {
                if (m_Loopbacks.TryGetValue(targetCID, out LoopbackEntry entry))
                {
                    entry.Transport.HandleReliable(datagram, targetCID);
                }
            }
        }

        /// <inheritdoc/>
        public void HandleReliable(ReadOnlySpan<byte> datagram, ulong sourceCID)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(HandleReliable)}(source: ({sourceCID}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <inheritdoc/>
        public void SendUnreliable(ReadOnlySpan<byte> datagram)
        {
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendUnreliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            lock (m_Loopbacks)
            {
                foreach (var entry in m_Loopbacks)
                {
                    entry.Value.Transport.HandleUnreliable(datagram, entry.Value.RemoteCID);
                }
            }
        }

        /// <inheritdoc/>
        public void SendUnreliableExclusive(ReadOnlySpan<byte> datagram, ulong CIDToExclude)
        {
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendUnreliableExclusive)}(exclude: ({CIDToExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            lock (m_Loopbacks)
            {
                foreach (var entry in m_Loopbacks)
                {
                    if (entry.Key != CIDToExclude)
                    {
                        entry.Value.Transport.HandleUnreliable(datagram, entry.Value.RemoteCID);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(ReadOnlySpan<byte> datagram, ulong targetCID)
        {
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(SendUnreliableTo)}(target: ({targetCID}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            lock (m_Loopbacks)
            {
                if (m_Loopbacks.TryGetValue(targetCID, out LoopbackEntry entry))
                {
                    entry.Transport.HandleUnreliable(datagram, targetCID);
                }
            }
        }

        /// <inheritdoc/>
        public void HandleUnreliable(ReadOnlySpan<byte> datagram, ulong sourceCID)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(LoopbackTransport)}.{nameof(HandleUnreliable)}(source: ({sourceCID}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
