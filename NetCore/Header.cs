using System;

namespace NetCore
{
    /// <summary>
    /// Common class for working with headers, and <see cref="HeaderReader"/> and <see cref="HeaderWriter"/> structs.
    /// </summary>
    /// TODO: Maybe pack a header version as well? So we can modify a reading mode based on a version.
    public static class Header
    {
        /// <summary>
        /// Contains all possible <see cref="MessageFlags"/> at once.
        /// Describes all the bits used by <see cref="MessageFlags"/> as well.
        /// </summary>
        public const MessageFlags AllFlags = MessageFlags.HasCustomHeader;

        /// <summary>
        /// Unpacks a first bit in a message, indicating what type of the message it is.
        /// </summary>
        /// <param name="datagram">Datagram to unpack a first bit of.</param>
        /// <param name="type">Type of the message, unpacked from <paramref name="datagram"/>.</param>
        /// <param name="flags">Flags of the message, unpacked from <paramref name="datagram"/>.</param>
        public static void Unpack(ReadOnlySpan<byte> datagram, out MessageType type, out MessageFlags flags)
        {
            if (datagram.IsEmpty)
            {
                type = MessageType.System;
                flags = MessageFlags.None;
                return;
            }

            Unpack(datagram[0], out type, out flags);
        }

        /// <summary>
        /// Unpacks (usually) a first bit in a message, indicating what type of the message it is.
        /// </summary>
        /// <param name="packed">(Usually) a first bit in a datagram, describing a <see cref="MessageType"/></param>
        /// <param name="type">Type of the message, unpacked from the first <paramref name="packed"/> bit.</param>
        /// <param name="flags">Flags of the message, unpacked from the first <paramref name="packed"/> bit.</param>
        public static void Unpack(byte packed, out MessageType type, out MessageFlags flags)
        {
            type = (MessageType)(packed & (int)~AllFlags);
            flags = (MessageFlags)(packed & (int)AllFlags);
        }

        /// <summary>
        /// Reads header from a <paramref name="datagram"/> and returns it as a <see cref="HeaderReader"/> struct.
        /// </summary>
        /// <param name="datagram">Datagram to read.</param>
        /// <returns><see cref="HeaderReader"/> to use.</returns>
        public static HeaderReader Read(ReadOnlySpan<byte> datagram) => new(datagram);
    }
}
