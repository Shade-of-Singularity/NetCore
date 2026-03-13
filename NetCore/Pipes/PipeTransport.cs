using System;
using System.Runtime.InteropServices;

namespace NetCore.Pipes
{
    /// <summary>
    /// Transport for C# Pipes.
    /// </summary>
    /// <remarks>
    /// Does not implement networking functionality at the moment.
    /// </remarks>
    public class PipeTransport : Transport, IUnreliableTransport, IReliableTransport
    {
        /// <inheritdoc/>
        public override bool HasConnection(ConnectionID connection)
        {
            return true;
        }

        /// <inheritdoc/>
        public void SendReliable(HeaderReader header, ReadOnlySpan<byte> datagram)
        {
#if DEBUG
            Console.WriteLine($"{nameof(PipeTransport)}.{nameof(SendReliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
        }

        /// <inheritdoc/>
        public void SendReliableExcluding(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID toExclude)
        {
#if DEBUG
            Console.WriteLine($"{nameof(PipeTransport)}.{nameof(SendReliableExcluding)}(exclude: ({toExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
        }

        /// <inheritdoc/>
        public void SendReliableTo(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID target)
        {
#if DEBUG
            Console.WriteLine($"{nameof(PipeTransport)}.{nameof(SendReliableTo)}(target: ({target}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
        }

        /// <inheritdoc/>
        public void HandleReliable(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID source)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(PipeTransport)}.{nameof(HandleReliable)}(source: ({source}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <inheritdoc/>
        public void SendUnreliable(HeaderReader header, ReadOnlySpan<byte> datagram)
        {
#if DEBUG
            Console.WriteLine($"{nameof(PipeTransport)}.{nameof(SendUnreliable)}(datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
        }

        /// <inheritdoc/>
        public void SendUnreliableExcluding(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID toExclude)
        {
#if DEBUG
            Console.WriteLine($"{nameof(PipeTransport)}.{nameof(SendUnreliableExcluding)}(exclude: ({toExclude}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
        }

        /// <inheritdoc/>
        public void SendUnreliableTo(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID target)
        {
#if DEBUG
            Console.WriteLine($"{nameof(PipeTransport)}.{nameof(SendUnreliableTo)}(target: ({target}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
#endif
        }

        /// <inheritdoc/>
        public void HandleUnreliable(HeaderReader header, ReadOnlySpan<byte> datagram, ConnectionID source)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{nameof(PipeTransport)}.{nameof(HandleUnreliable)}(source: ({source}) datagram: {MemoryMarshal.Cast<byte, char>(datagram).ToString()})");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
