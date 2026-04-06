using System.Runtime.CompilerServices;

namespace NetCore
{
    /// <summary>
    /// Helpful extension methods for working with session-based messaging methods.
    /// </summary>
    public static partial class SendNetworkMessagingExtensions
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
        /// <seealso cref="ISendNetworkMessaging.SendUnreliable(ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="ISendNetworkMessaging.SendReliable(ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="ISendNetworkMessaging.SendSequential(ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <seealso cref="ISendNetworkMessaging.SendResilient(ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="target">Instance of <see cref="ISendNetworkMessaging"/> to work with.</param>
        /// <param name="mode"><see cref="SendingMode"/>, specifying which transport type to use.</param>
        /// <param name="datagram">Datagram to send.</param>
        /// <param name="header">Header to encode with the message.</param>
        /// <param name="flags">Non-encoded in a message. Stores info about how message should be sent.</param>
        public static void Send(this ISendNetworkMessaging target, SendingMode mode, scoped ReadOnlySpan<byte> datagram, ref Header header, ref Flags flags)
        {
            switch (mode)
            {
                case SendingMode.Unreliable: target.SendUnreliable(datagram, ref header, ref flags); return;
                case SendingMode.Reliable: target.SendReliable(datagram, ref header, ref flags); return;
                case SendingMode.Sequential: target.SendSequential(datagram, ref header, ref flags); return;
                case SendingMode.Resilient: target.SendResilient(datagram, ref header, ref flags); return;
                default: throw new SwitchExpressionException(mode);
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                          SendingMode - Variations
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc cref="Send(ISendNetworkMessaging, SendingMode, ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public static void Send(this ISendNetworkMessaging target, SendingMode mode, ReadOnlySpan<byte> datagram)
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            target.Send(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send(ISendNetworkMessaging, SendingMode, ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public static void Send(this ISendNetworkMessaging target, SendingMode mode, ReadOnlySpan<byte> datagram, ref Header header)
        {
            Flags flags = Flags.Get();
            target.Send(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send(ISendNetworkMessaging, SendingMode, ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        public static void Send(this ISendNetworkMessaging target, SendingMode mode, ReadOnlySpan<byte> datagram, ref Flags flags)
        {
            Header header = Header.Get();
            target.Send(mode, datagram, ref header, ref flags);
        }

        /// <inheritdoc cref="Send(ISendNetworkMessaging, SendingMode, ReadOnlySpan{byte}, ref Header, ref Flags)"/>
        /// <param name="target"/>
        /// <param name="datagram"/>
        /// <param name="mode"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public static void Send(this ISendNetworkMessaging target, SendingMode mode, ReadOnlySpan<byte> datagram, HeaderConstructor? headerSetup, FlagsConstructor? flagsSetup = null)
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            target.Send(mode, datagram, ref header, ref flags);
        }
    }
}
