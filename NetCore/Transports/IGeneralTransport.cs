using System.Runtime.CompilerServices;

namespace NetCore.Transports
{
    /// <summary>
    /// Transport interface, implementing all types of transports:
    /// <see cref="IUnreliableTransport"/>, <see cref="IReliableTransport"/>,
    /// <see cref="ISequentialTransport"/>, <see cref="IResilientTransport"/>
    /// </summary>
    /// <remarks>
    /// Implementing this interface will automatically provide additional useful methods via <see cref="GeneralTransportExtensions"/>.
    /// </remarks>
    public interface IGeneralTransport : IUnreliableTransport, IReliableTransport, ISequentialTransport, IResilientTransport
    {
        // Methods provided using extensions, for better performance.
    }

    /// <summary>
    /// Extensions for simpler <see cref="IGeneralTransport"/> usage.
    /// </summary>
    public static class GeneralTransportExtensions
    {
        /// <summary>
        /// Sends <paramref name="datagram"/> to all connections this <see cref="ITransport"/> manages,
        /// using given <paramref name="mode"/>.
        /// </summary>
        /// <param name="transport">Transport to use for sending.</param>
        /// <param name="mode">Sending mode to use.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public static void Send(
            this IGeneralTransport transport, SendingMode mode,
            in ReadOnlySpan<byte> datagram, in Header header, in Flags flags)
        {
            switch (mode)
            {
                case SendingMode.Unreliable: transport.SendUnreliable(in datagram, in header, in flags); return;
                case SendingMode.Reliable: transport.SendReliable(in datagram, in header, in flags); return;
                case SendingMode.Sequential: transport.SendSequential(in datagram, in header, in flags); return;
                case SendingMode.Resilient: transport.SendResilient(in datagram, in header, in flags); return;
                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// Sends <paramref name="datagram"/> to all connections this <see cref="ITransport"/> manages,
        /// excluding <paramref name="toExclude"/>, using given <paramref name="mode"/>.
        /// </summary>
        /// <param name="transport">Transport to use for sending.</param>
        /// <param name="mode">Sending mode to use.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="toExclude">Connection to avoid sending a <paramref name="datagram"/> to.</param>
        public static void SendExcluding(
            this IGeneralTransport transport, SendingMode mode,
            in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID toExclude)
        {
            switch (mode)
            {
                case SendingMode.Unreliable: transport.SendUnreliableExcluding(in datagram, in header, in flags, toExclude); return;
                case SendingMode.Reliable: transport.SendReliableExcluding(in datagram, in header, in flags, toExclude); return;
                case SendingMode.Sequential: transport.SendSequentialExcluding(in datagram, in header, in flags, toExclude); return;
                case SendingMode.Resilient: transport.SendResilientExcluding(in datagram, in header, in flags, toExclude); return;
                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// Sends <paramref name="datagram"/> to a <paramref name="target"/> connection, using given <paramref name="mode"/>.
        /// </summary>
        /// <param name="transport">Transport to use for sending.</param>
        /// <param name="mode">Sending mode to use.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="target">Connection to send a <paramref name="datagram"/> to. Nothing should be sent if transport doesn't manage this connection.</param>
        public static void SendTo(
            this IGeneralTransport transport, SendingMode mode,
            in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionID target)
        {
            switch (mode)
            {
                case SendingMode.Unreliable: transport.SendUnreliableTo(in datagram, in header, in flags, target); return;
                case SendingMode.Reliable: transport.SendReliableTo(in datagram, in header, in flags, target); return;
                case SendingMode.Sequential: transport.SendSequentialTo(in datagram, in header, in flags, target); return;
                case SendingMode.Resilient: transport.SendResilientTo(in datagram, in header, in flags, target); return;
                default: throw new SwitchExpressionException(mode);
            }
        }

        /// <summary>
        /// Sends <paramref name="datagram"/> to a remote host, specified with <paramref name="args"/>, using given <paramref name="mode"/>.
        /// </summary>
        /// <param name="transport">Transport to use for sending.</param>
        /// <param name="mode">Sending mode to use.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header of the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// <param name="args">Temporary connection args used for this connection in particular.</param>
        public static void SendTo(
            this IGeneralTransport transport, SendingMode mode,
            in ReadOnlySpan<byte> datagram, in Header header, in Flags flags, ConnectionArgs args)
        {
            switch (mode)
            {
                case SendingMode.Unreliable: transport.SendUnreliableTo(in datagram, in header, in flags, args); return;
                case SendingMode.Reliable: transport.SendReliableTo(in datagram, in header, in flags, args); return;
                case SendingMode.Sequential: transport.SendSequentialTo(in datagram, in header, in flags, args); return;
                case SendingMode.Resilient: transport.SendResilientTo(in datagram, in header, in flags, args); return;
                default: throw new SwitchExpressionException(mode);
            }
        }
    }
}
