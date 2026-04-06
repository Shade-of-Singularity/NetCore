using ComputerysBitStream;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetCore.Packets
{
    /// <summary>
    /// Extension methods for working with <see cref="Packet"/>s.
    /// </summary>
    public static class PacketExtensions
    {
        /// <summary>
        /// Encodes given <paramref name="packet"/> as datagram and sends it, using provided <see cref="SendingMode"/>.
        /// </summary>
        /// <typeparam name="T">Type of a packet.</typeparam>
        /// <param name="target">Member, sending the datagram.</param>
        /// <param name="mode"><see cref="SendingMode"/> to use.</param>
        /// <param name="packet">Packet to encode.</param>
        /// <param name="header">Header to encode.</param>
        /// <param name="flags">Flags, specifying how the message should be sent.</param>
        public static void Send<T>(this ISendNetworkMessaging target, SendingMode mode, Packet<T> packet, ref Header header, ref Flags flags) where T : Packet<T>
        {
            scoped WriteContext context = new(stackalloc ulong[1280]);
            packet.Write(context);
            target.Send(mode, context.ToByte(), ref header, ref flags);
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                          SendingMode - Variations
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc cref="Send{T}(ISendNetworkMessaging, SendingMode, Packet{T}, ref Header, ref Flags)"/>
        public static void Send<T>(this ISendNetworkMessaging target, SendingMode mode, Packet<T> packet) where T : Packet<T>
        {
            Header header = Header.Get();
            Flags flags = Flags.Get();
            target.Send(mode, packet, ref header, ref flags);
        }

        /// <inheritdoc cref="Send{T}(ISendNetworkMessaging, SendingMode, Packet{T}, ref Header, ref Flags)"/>
        public static void Send<T>(this ISendNetworkMessaging target, SendingMode mode, Packet<T> packet, ref Header header) where T : Packet<T>
        {
            Flags flags = Flags.Get();
            target.Send(mode, packet, ref header, ref flags);
        }

        /// <inheritdoc cref="Send{T}(ISendNetworkMessaging, SendingMode, Packet{T}, ref Header, ref Flags)"/>
        public static void Send<T>(this ISendNetworkMessaging target, SendingMode mode, Packet<T> packet, ref Flags flags) where T : Packet<T>
        {
            Header header = Header.Get();
            target.Send(mode, packet, ref header, ref flags);
        }

        /// <inheritdoc cref="Send{T}(ISendNetworkMessaging, SendingMode, Packet{T}, ref Header, ref Flags)"/>
        /// <param name="target"/>
        /// <param name="packet"/>
        /// <param name="mode"/>
        /// <param name="headerSetup">Constructor for setting up provided <see cref="Header"/> reference.</param>
        /// <param name="flagsSetup">Constructor for setting up provided <see cref="Flags"/> reference.</param>
        public static void Send<T>(this ISendNetworkMessaging target, SendingMode mode, Packet<T> packet, HeaderConstructor? headerSetup, FlagsConstructor? flagsSetup = null) where T : Packet<T>
        {
            Header header = Header.Get();
            headerSetup?.Invoke(ref header);
            Flags flags = Flags.Get();
            flagsSetup?.Invoke(ref flags);
            target.Send(mode, packet, ref header, ref flags);
        }
    }
}
