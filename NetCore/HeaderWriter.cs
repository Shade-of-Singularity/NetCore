using System;
using System.Buffers;

namespace NetCore
{
    /// <summary>
    /// Message header with custom data.
    /// </summary>
    /// <param name="type">Type of the message this header describes.</param>
    /// TODO: Add a way to parse a beginning of the message into a valid header without using <see cref="NetworkMember"/> as a base.
    ///  This will be useful if people would want to create custom relays which does not rely on <see cref="ITransport"/>s.
    public ref struct HeaderWriter(MessageType type)
    {
        /// <summary>
        /// Provided to remove size checks in a non-initialized header.
        /// </summary>
        private static readonly byte[] ZeroByte = [0];
        /// <summary>
        /// Amount of bits stored in a custom header data.
        /// </summary>
        public readonly int Bits => bits;
        /// <summary>
        /// Amount of bytes stored in a custom header data.
        /// </summary>
        public readonly int Bytes => (bits + 7) >> 3; // Division by 8 with rounding up.
        /// <summary>
        /// Type of the message this header describes.
        /// </summary>
        public readonly MessageType type = type;
        /// <summary>
        /// Bytes storing header flags.
        /// </summary>
        private byte[] flags = ZeroByte;
        /// <summary>
        /// Data storage for custom headers.
        /// Headers can have size smaller that a byte (2-4 bits, etc)
        /// Thus storage is not expected to be aligned to a byte size.
        /// </summary>
        private byte[] storage = ZeroByte; // TODO: Deal with ordering on writes.
        /// <summary>
        /// Packed bits. Needed because we cannot support writing after packing otherwise.
        /// </summary>
        private byte[]? packed;
        /// <summary>
        /// Amount of bits written to the header.
        /// </summary>
        private int bits;
        /// <summary>
        /// Whether this header was initialized or not.
        /// </summary>
        private bool initialized;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Static Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Reads header from a datagram.
        /// </summary>
        /// <returns>
        /// <c>true</c> - if reading was successful and <paramref name="header"/> was provided.
        /// <c>false</c> - if <paramref name="datagram"/> is empty, and <paramref name="header"/> is set to default value.
        /// </returns>
        public static bool TryRead(ReadOnlySpan<byte> datagram, out HeaderReader header)
        {
            if (datagram.IsEmpty)
            {
                header = default;
                return false;
            }

            MessageType type = (MessageType)datagram[0];
            int[] sizes = CustomHeaders.GetSizeMap(type);
            if (sizes.Length == 0)
            {
                header = new(type);
                return true;
            }

            // Calculates amount of regions used for a header flag declarations.
            int regions = (sizes.Length + 6) / 7;

        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Checks if this <see cref="HeaderReader"/> has a <see cref="CustomHeader{T}"/> defined.
        /// </summary>
        /// <typeparam name="T">Type of the header.</typeparam>
        /// <returns>Whether header is defined or not.</returns>
        public readonly bool Has<T>() where T : CustomHeader<T>, new() => (flags[CustomHeader<T>.TargetRegion] & CustomHeader<T>.RegionFlag) != 0;

        /// <summary>
        /// Sets bit data in a region, allocated to the custom header.
        /// </summary>
        /// <remarks>
        /// Anything beyond declared in <see cref="CustomHeader{T}.Size"/> will be cut off.
        /// </remarks>
        /// <typeparam name="T">Type of the header.</typeparam>
        /// <param name="bytes">Bytes to set a header to.</param>
        public void Set<T>(ReadOnlySpan<ulong> bytes) where T : CustomHeader<T>, new()
        {





            //Set<T>(bytes, CustomHeader<T>.SizeInBits);
            //Set<T>(MemoryMarshal.Cast<ulong, byte>(bytes));
        }

        /// <summary>
        /// Sets bit data in a region, allocated to the custom header.
        /// </summary>
        /// <typeparam name="T">Type of the header.</typeparam>
        /// <param name="bytes">Bytes to set a header to.</param>
        /// <param name="bits">Total amount of *bits* used to declare a header.</param>
        public void Set<T>(ReadOnlySpan<byte> bytes, int bits) where T : CustomHeader<T>, new()
        {
            int region = bits >> 3;
            bits -= region << 3; // Remaining bits.

        }

        public void Set<T>(byte value) where T : CustomHeader<T>, new()
        {

        }

        public void Set<T>(byte value, int bits) where T : CustomHeader<T>, new()
        {

        }

        public void Set<T>(short value) where T : CustomHeader<T>, new()
        {

        }

        public void Set<T>(short value, int bits) where T : CustomHeader<T>, new()
        {

        }

        public void Set<T>(ushort value) where T : CustomHeader<T>, new()
        {

        }

        public void Set<T>(ushort value, int bits) where T : CustomHeader<T>, new()
        {

        }


        /// <summary>
        /// Disposes internal arrays.
        /// </summary>
        public readonly void Dispose()
        {
            if (storage is not null)
                ArrayPool<byte>.Shared.Return(storage);
        }
    }
}
