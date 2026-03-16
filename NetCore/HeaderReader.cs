using System;
using System.Buffers;
using System.Runtime.InteropServices;

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
        /// Thrown on read in <see cref="HeaderReader"/>, if you provide insufficiently large buffer for the bytes.
        /// </summary>
        public class InsufficientBufferException : Exception {}

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
        public readonly ReadOnlySpan<byte> regions;
        /// <summary>
        /// The rest of the datagram, storing custom header data and message content itself.
        /// Not sliced to avoid counting bits at the initial setup.
        /// </summary>
        /// Note: We can probably encode amount of bits used using zigzag writing/reading.
        public readonly ReadOnlySpan<byte> content;
        /// <summary>
        /// Describes where each header is positioned based on their <see cref="CustomHeader{T}.HeaderOrder"/> value.
        /// </summary>
        public readonly int[] bitPositions;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Initializes a <see cref="HeaderReader"/> using data from a provided <paramref name="datagram"/>.
        /// Reads header data and allows you to use <see cref="Read"/> methods.
        /// </summary>
        public HeaderReader(ReadOnlySpan<byte> datagram)
        {
            if (datagram.IsEmpty)
            {
                bitPositions = [];
                return;
            }

            Header.Unpack(datagram, out type, out flags);
            if ((flags & MessageFlags.HasCustomHeader) == MessageFlags.HasCustomHeader)
            {
                int amount = CustomHeaders.Amount;
                regions = datagram.Slice(1, (amount + 7) >> 3);

                int position = 0;
                var bitMap = CustomHeaders.BitMap;
                // TODO: Rent array from a storage defined in CustomHeaders.
                // Note: Since array aligns with CustomHeader{T}.HeaderOrder - array will contain default values over unset headers.
                bitPositions = new int[amount];
                for (int i = 0; i < amount; i++)
                {
                    byte region = regions[i >> 3]; // TODO: Optimize by accessing the array only on each 8th bit.
                    if ((region & (i & 0b111)) != 0)
                    {
                        bitPositions[i] = position;
                        position += bitMap[i];
                    }
                }
            }
            else
            {
                bitPositions = [];
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
        /// <exception cref="InsufficientBufferException">
        /// <paramref name="buffer"/> is smaller than <see cref="CustomHeader{T}.SizeInBytes"/> and cannot contain all the data.
        /// </exception>
        public void Read<T>(in Span<byte> buffer) where T : CustomHeader<T>, new()
        {
            if (CustomHeader<T>.SizeInBytes == 0 || !Has<T>())
                return;

            if (buffer.Length < CustomHeader<T>.SizeInBytes)
                throw new InsufficientBufferException();

            // Decides which reader to use.
            // Byte-size reading requires the most operations.
            // TODO: with values like 3, 5 and 7, part of the buffer can be read as ushort.
            //  And with 6 - as uint. We can use it to partially copy using larger value types, and fill-in the rest using simple bytes.
            switch (CustomHeader<T>.SizeInBytes & 0b1111)
            {
                case 1 or 3 or 5 or 7: ReadByte(buffer); return; // Read as byte span.
                case 2 or 6: ReadUShort(MemoryMarshal.Cast<byte, ushort>(buffer[..CustomHeader<T>.SizeInBytes])); return; // Read as ushort span.
                case 4: ReadUInt(MemoryMarshal.Cast<byte, uint>(buffer[..CustomHeader<T>.SizeInBytes])); return; // Read as uint span.
                default: ReadULong(MemoryMarshal.Cast<byte, ulong>(buffer[..CustomHeader<T>.SizeInBytes])); return; // Read as ulong span.
            }

            static void ReadByte(in Span<byte> buffer)
            {

            }

            static void ReadUShort(in Span<ushort> buffer)
            {

            }

            static void ReadUInt(in Span<uint> buffer)
            {

            }

            static void ReadULong(in Span<ulong> buffer)
            {

            }

            //int position = bitPositions[CustomHeader<T>.HeaderOrder];
            //int offset = position & 0b111; // Offset within a byte.
            //if (offset == 0)
            //{
            //    // This is glimpse in a world of butterflies! Just plain block copy.
            //    // I wish I could do something similar with buffers.
            //    content.Slice(position >> 3, CustomHeader<T>.SizeInBytes).CopyTo(buffer);
            //    return;
            //}

            //int length = (CustomHeader<T>.SizeInBits + offset + 7) >> 3;
            //content.Slice(position >> 3, length).CopyTo(buffer);

            //int src = position >> 3;
            //byte last = content[src + length - 1];
            //for (int i = CustomHeader<T>.SizeInBytes - 1; i >= 0; i--)
            //{
                
            //}


            //buffer[CustomHeader<T>.SizeInBytes - 1] = (byte)(last >> offset);
            //for (int i = CustomHeader<T>.SizeInBytes - 2; i >= 0; i--)
            //{
            //    buffer[i] = 
            //}


            //int lowMask = (1 << offset)
            //switch (length)
            //{
            //    case 0: return;
            //    case 1: buffer[0] = (byte)(content[position >> 3] & ((1 << CustomHeader<T>.SizeInBits) - 1 + offset));
            //        return;
            //}

            //int src = position >> 3;
            //byte last = content[src];
            //buffer[0] = (byte)(last >> offset);
            //for (int i = 1; i < length; i++)
            //{

            //    buffer[i - 1] = content[src + i];
            //}


            //for (int i = 0; i < length; i++)
            //{
            //    ushort
            //}
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
            //if (packed is not null)
            //    ArrayPool<byte>.Shared.Return(packed);
        }
    }
}
