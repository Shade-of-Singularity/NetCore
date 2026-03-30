using NetCore.Transports;
using System;
using System.Runtime.CompilerServices;

namespace NetCore
{
    /// <summary>
    /// Helpful extension methods for working with transport-based messaging methods.
    /// </summary>
    public static partial class TransportBasedSendNetworkMessagingExtensions
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                            SendingMode - Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// <para>Sends <paramref name="datagram"/> to the server, using specified <see cref="SendingMode"/>.</para>
        /// <para>Locks <paramref name="header"/> and <paramref name="flags"/> on usage.</para>
        /// </summary>
        /// <seealso cref="ITransportBasedSendNetworkMessaging.SendUnreliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="ITransportBasedSendNetworkMessaging.SendReliable{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="ITransportBasedSendNetworkMessaging.SendSequential{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="ITransportBasedSendNetworkMessaging.SendResilient{TTransport}(in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="target">Instance of <see cref="ISendNetworkMessaging"/> to work with.</param>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        /// TODO: Remove a constraint for <typeparam name="TTransport"/> of using all transport types.
        ///  It should be now possible after <see cref="Common.CRTPList{TBase}.Lookup{TFilter}"/> rework.
        public static void Send<TTransport>(this ITransportBasedSendNetworkMessaging target, SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            switch (mode)
            {
                case SendingMode.Unreliable: target.SendUnreliable<TTransport>(datagram, ref header, ref flags); return;
                case SendingMode.Reliable: target.SendReliable<TTransport>(datagram, ref header, ref flags); return;
                case SendingMode.Sequential: target.SendSequential<TTransport>(datagram, ref header, ref flags); return;
                case SendingMode.Resilient: target.SendResilient<TTransport>(datagram, ref header, ref flags); return;
                default: throw new SwitchExpressionException(mode);
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                          SendingMode - Variations
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>

        /// <inheritdoc cref="Send{TTransport}(ITransportBasedSendNetworkMessaging, SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public static void Send<TTransport>(ITransportBasedSendNetworkMessaging target, SendingMode mode, in ReadOnlySpan<byte> datagram)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            target.Send<TTransport>(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send{TTransport}(ITransportBasedSendNetworkMessaging, SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public static void Send<TTransport>(ITransportBasedSendNetworkMessaging target, SendingMode mode, in ReadOnlySpan<byte> datagram, ref Header header)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Flags flags = Flags.Get();
            target.Send<TTransport>(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send{TTransport}(ITransportBasedSendNetworkMessaging, SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public static void Send<TTransport>(ITransportBasedSendNetworkMessaging target, SendingMode mode, in ReadOnlySpan<byte> datagram, ref Flags flags)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Header header = Header.Get();
            target.Send<TTransport>(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send{TTransport}(ITransportBasedSendNetworkMessaging, SendingMode, in ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="target"/>
        /// <param name="mode"/>
        /// <param name="datagram"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public static void Send<TTransport>(ITransportBasedSendNetworkMessaging target, SendingMode mode, in ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup, FlagsConstructor? flagsSetup = null)
            where TTransport : class, IReliableTransport, IUnreliableTransport, ISequentialTransport, IResilientTransport
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            target.Send<TTransport>(mode, datagram, ref header, ref flags);
        }
    }
}
