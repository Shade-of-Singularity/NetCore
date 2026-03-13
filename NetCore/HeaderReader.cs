using System;
using System.Buffers;

namespace NetCore
{
    /// <summary>
    /// Message header with custom data.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Header.Read"/> to get an instance of it.
    /// </remarks>
    /// TODO: Add a way to parse a beginning of the message into a valid header without using <see cref="NetworkMember"/> as a base.
    ///  This will be useful if people would want to create custom relays which does not rely on <see cref="ITransport"/>s.
    public readonly ref struct HeaderReader
    {
        /// <summary>
        /// Type of the message this header describes.
        /// </summary>
        public readonly MessageType type;
        /// <summary>
        /// Flags of the message this header describes.
        /// </summary>
        public readonly MessageFlags flags;
        /// <summary>
        /// Regions, encoding presence of the headers.
        /// </summary>
        private readonly ReadOnlySpan<byte> regions;
        /// <summary>
        /// The rest of the datagram, storing custom header data and message content itself.
        /// Not sliced to avoid counting bits at the initial setup.
        /// </summary>
        /// Note: We can probably encode amount of bits used using zigzag writing/reading.
        private readonly ReadOnlySpan<byte> content;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        public HeaderReader(ReadOnlySpan<byte> datagram)
        {
            if (datagram.IsEmpty)
            {
                return;
            }

            Header.Unpack(datagram, out type, out flags);
            if ((flags & MessageFlags.HasCustomHeader) == MessageFlags.HasCustomHeader)
            {
                int amount = CustomHeaders.Amount;
                regions = datagram.Slice(1, (amount + 7) >> 3);

                int offset = 0;
                var bitMap = CustomHeaders.BitMap;
                // TODO: Rent array from a storage defined in CustomHeaders.
                // Note: Since array aligns with CustomHeader{T}.HeaderOrder - array will contain default values over unset headers.
                int[] offsets = new int[amount];
                for (int i = 0; i < amount; i++)
                {
                    byte region = regions[i >> 3]; // TODO: Optimize by accessing the array only on each 8th bit.
                    if ((region & (i & 0b111)) != 0)
                    {
                        offsets[i] = offset;
                        offset += bitMap[i];
                    }
                }
            }
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
        public readonly bool Has<T>() where T : CustomHeader<T>, new()
        {
            return !regions.IsEmpty && (regions[CustomHeader<T>.TargetRegion] & CustomHeader<T>.RegionFlag) != 0;
        }

        /// <summary>
        /// Reads all data associated with this custom header into provided <paramref name="buffer"/>.
        /// </summary>
        public void Read<T>(ref Span<byte> buffer, out int bits) where T : CustomHeader<T>, new()
        {
            if (!Has<T>())
            {
                bits = 0;
                return;
            }

            // TODO: Simplify lookup if possible. Maybe use advanced bit-packing like with QuickMap after all?
            int position = 0;
            var sizes = CustomHeaders.SizeMap;
            int order = CustomHeader<T>.HeaderOrder;
            int limit = order >> 3;

            int index = 0;
            for (int i = 0; i < limit; i++)
            {
                int region = regions[i];
                for (int f = 1; 0b1_0000_0000 > f; f <<= 1)
                {
                    if ((region & f) != 0)
                        position += sizes[index];
                    index++;
                }
            }


            bits = CustomHeader<T>.SizeInBits;
            CustomHeader<T>.
        }

        /// <summary>
        /// Sets bit data in a region, allocated to the custom header.
        /// </summary>
        /// <remarks>
        /// Anything beyond declared in <see cref="CustomHeader{T}.Size"/> will be cut off.
        /// </remarks>
        /// <typeparam name="T">Type of the header.</typeparam>
        /// <param name="bytes">Bytes to set a header to.</param>
        public void Set<T>(ReadOnlySpan<byte> bytes) where T : CustomHeader<T>, new() => Set<T>(bytes, CustomHeader<T>.SizeInBits);

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
            if (packed is not null)
                ArrayPool<byte>.Shared.Return(packed);
        }
    }
}
