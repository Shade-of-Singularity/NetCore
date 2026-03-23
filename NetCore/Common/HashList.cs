using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NetCore.Common
{
    /// <summary>
    /// Exception for handling when you use <see cref="HashList{T}.GetLookup{TItem}"/>
    /// with TItem matching a base type of the <see cref="HashList{T}"/>.
    /// </summary>
    /// <param name="type">Target item type.</param>
    public sealed class InvalidLookupTypeException(Type type)
        : Exception($"Cannot use ({type.Name}) as a type for a Lookup, as it matches the type of a HashList. Use HashList directly instead.")
    { }

    /// <summary>
    /// (Not thread-safe! Used must ensure safety)
    /// Struct-based hash list, for storing unique strong-typed items.
    /// Optimized to be a lot faster than <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> using CRTP.
    /// </summary>
    /// <remarks>
    /// Internally, encodes invalid indexes as '0' instead of '-1' and such.
    /// Because of that, mapping resizing will not happen when adding 17th item, but a 16th instead.
    /// Another resize will happen when adding 256th item instead of 257th.
    /// </remarks>
    /// <typeparam name="TBase">Base type of an item.</typeparam>
    /// TODO: Support trimming/resizing down.
    /// TODO: Use Dictionary as a fallback for weak-type indexing.
    /// Note: There are two technologies that can be used - flag-based and index-based.
    ///  > In the first case - we split one entry in two parts:
    ///  - left describing how many items are stored in entries before that one.
    ///  - right storing flags, describing which items are stored in the list.
    ///  With uint being used as a base, you can store up to <see cref="ushort.MaxValue"/> of items.
    ///  And with ulong as a base - <see cref="uint.MaxValue"/> is a realistic limit.
    ///  And with that, each entry will be able to store up to 16 items (with uint) or up to 32 (with ulong).
    ///  = Downsides:
    ///    - A bit slower lookups:
    ///      - Exist: (IL: 1 array get + 1 bit-masking + 1 branch)
    ///      - TryGet: (IL: 1 array get + 1 bit-masking + 1 branch)(+IL: 1 bit-masking + 1 bit-shift + 1 jump-table + 1 addition)
    ///      - Get: (IL: 1 array get + 1 bit-masking + 1 bit-shift + 1 jump-table + 1 addition)
    ///    - when you add an item, you need to update all left sides entries.
    ///  = Upsides:
    ///    - you can iterate over them by marshaling the collection to a lower type (uint -> ushort, etc) and iterating over each 2nd value.
    ///    - Flags processing can be simplified with a very large jump-table, if needed.16
    ///    - Constant overhead of 2 bits per item.
    ///    - Minimal lag-spikes (only for array resize).
    ///  > In the second case we can store the index (where item is stored) and its flag (if it is stored at all) in one entry.
    ///  Entries themselves can be of a different size. Most optimal are:
    ///  - 4 bits (allows up to 8 items),
    ///  - 8 bits (allows up to 128 items),
    ///  - 16 bits (allows up to 32768 items),
    ///  - 32 bits (allows up to 2147483647 items)
    ///  Entry is allocated *per-item*.
    ///  > Downsides:
    ///    - When crossing a "size threshold" (8 -> 9, 128 -> 129, etc.) you need to upscale the map.
    ///      - Similarly, on trimming, you might need to downscale it, depending on the implementation.
    ///      - This requires rebuilding the entire map, creating a lag spike once.
    ///    - Need to update the indexes everywhere on each item addition.
    ///    - When working with up to 128 items (most likely scenario), memory usage - 1 byte per item (aligned to a size of uint).
    ///  > Upsides:
    ///    - Fast index lookups:
    ///      - Exist: (IL: 1 array get + 1 bit-masking + 1 branch)
    ///      - TryGet: (IL: 1 array get + 1 bit-masking + 1 branch)(+IL: 1 bit-masking + 1 bit-shift)
    ///      - Get: (IL: 1 array get + 1 bit-masking + 1 bit-shift)
    ///    - Fast and intuitive lookups (since entries are aligned)
    ///      - Generally - data is stored directly as-is, ignoring a leading bit-flag.
    public struct HashList<TBase>
    {
        static class Indexing
        {
            static uint index = 0;
            public static uint NextIndex() => index++;
        }

        static class ID<TItem> where TItem : TBase
        {
            /// <summary>
            /// Initialization order of this item.
            /// </summary>
            public static readonly uint Index = Indexing.NextIndex();
        }

        enum PackingMode : byte
        {
            /// <summary>
            /// Completely empty list. Does not store or encode any values.
            /// </summary>
            Size0,
            /// <summary>
            /// Uses 4bits per entry. <see cref="uint"/> encodes up to 8 such entries.
            /// </summary>
            Size1to8,
            /// <summary>
            /// Uses 8bits per entry. <see cref="uint"/> encodes up to 4 such entries.
            /// </summary>
            Size9to128,
            /// <summary>
            /// Uses 16bits per entry. <see cref="uint"/> encodes up to 2 such entries.
            /// </summary>
            Size129to32768,
            /// <summary>
            /// Uses 32bits per entry. <see cref="uint"/> encodes only one such entry.
            /// </summary>
            /// <remarks>
            /// If you ever reach this point - let us know. You will be the first, LoL :D
            /// </remarks>
            Size32769to2147483647,
        }

        static class Packing1to8
        {
            public const int Capacity = 8;

            public const uint ItemMask1 = 0b0000_0000_0000_0000_0000_0000_0000_1111u;
            public const uint ItemMask2 = 0b0000_0000_0000_0000_0000_0000_1111_0000u;
            public const uint ItemMask3 = 0b0000_0000_0000_0000_0000_1111_0000_0000u;
            public const uint ItemMask4 = 0b0000_0000_0000_0000_1111_0000_0000_0000u;
            public const uint ItemMask5 = 0b0000_0000_0000_1111_0000_0000_0000_0000u;
            public const uint ItemMask6 = 0b0000_0000_1111_0000_0000_0000_0000_0000u;
            public const uint ItemMask7 = 0b0000_1111_0000_0000_0000_0000_0000_0000u;
            public const uint ItemMask8 = 0b1111_0000_0000_0000_0000_0000_0000_0000u;

            public const uint ItemFlag1 = 0b0000_0000_0000_0000_0000_0000_0000_1000u;
            public const uint ItemFlag2 = 0b0000_0000_0000_0000_0000_0000_1000_0000u;
            public const uint ItemFlag3 = 0b0000_0000_0000_0000_0000_1000_0000_0000u;
            public const uint ItemFlag4 = 0b0000_0000_0000_0000_1000_0000_0000_0000u;
            public const uint ItemFlag5 = 0b0000_0000_0000_1000_0000_0000_0000_0000u;
            public const uint ItemFlag6 = 0b0000_0000_1000_0000_0000_0000_0000_0000u;
            public const uint ItemFlag7 = 0b0000_1000_0000_0000_0000_0000_0000_0000u;
            public const uint ItemFlag8 = 0b1000_0000_0000_0000_0000_0000_0000_0000u;

            public const uint ItemValue1 = 0b0000_0000_0000_0000_0000_0000_0000_0111u;
            public const uint ItemValue2 = 0b0000_0000_0000_0000_0000_0000_0111_0000u;
            public const uint ItemValue3 = 0b0000_0000_0000_0000_0000_0111_0000_0000u;
            public const uint ItemValue4 = 0b0000_0000_0000_0000_0111_0000_0000_0000u;
            public const uint ItemValue5 = 0b0000_0000_0000_0111_0000_0000_0000_0000u;
            public const uint ItemValue6 = 0b0000_0000_0111_0000_0000_0000_0000_0000u;
            public const uint ItemValue7 = 0b0000_0111_0000_0000_0000_0000_0000_0000u;
            public const uint ItemValue8 = 0b0111_0000_0000_0000_0000_0000_0000_0000u;

            public const int ItemShift1 = 0;
            public const int ItemShift2 = 4;
            public const int ItemShift3 = 8;
            public const int ItemShift4 = 12;
            public const int ItemShift5 = 16;
            public const int ItemShift6 = 20;
            public const int ItemShift7 = 24;
            public const int ItemShift8 = 28;
        }

        static class Packing9to128
        {
            public const int Capacity = sbyte.MaxValue + 1;

            public const uint ItemMask1 = 0b00000000_00000000_00000000_11111111u;
            public const uint ItemMask2 = 0b00000000_00000000_11111111_00000000u;
            public const uint ItemMask3 = 0b00000000_11111111_00000000_00000000u;
            public const uint ItemMask4 = 0b11111111_00000000_00000000_00000000u;

            public const uint ItemFlag1 = 0b00000000_00000000_00000000_10000000u;
            public const uint ItemFlag2 = 0b00000000_00000000_10000000_00000000u;
            public const uint ItemFlag3 = 0b00000000_10000000_00000000_00000000u;
            public const uint ItemFlag4 = 0b10000000_00000000_00000000_00000000u;

            public const uint ItemValue1 = 0b00000000_00000000_00000000_01111111u;
            public const uint ItemValue2 = 0b00000000_00000000_01111111_00000000u;
            public const uint ItemValue3 = 0b00000000_01111111_00000000_00000000u;
            public const uint ItemValue4 = 0b01111111_00000000_00000000_00000000u;

            public const int ItemShift1 = 0;
            public const int ItemShift2 = 8;
            public const int ItemShift3 = 16;
            public const int ItemShift4 = 24;
        }

        static class Packing129to32768
        {
            public const int Capacity = short.MaxValue + 1;
            public const uint ItemMask1 = 0b0000000000000000_1111111111111111u;
            public const uint ItemMask2 = 0b1111111111111111_0000000000000000u;

            public const uint ItemFlag1 = 0b0000000000000000_1000000000000000u;
            public const uint ItemFlag2 = 0b1000000000000000_0000000000000000u;

            public const uint ItemValue1 = 0b0000000000000000_0111111111111111u;
            public const uint ItemValue2 = 0b0111111111111111_0000000000000000u;

            public const int ItemShift1 = 0;
            public const int ItemShift2 = 16;
        }

        static class Packing32769to2147483647
        {
            public const int Capacity = int.MaxValue;
            public const uint ItemMask1 = 0b11111111111111111111111111111111u;
            public const uint ItemFlag1 = 0b10000000000000000000000000000000u;
            public const uint ItemValue1 = 0b01111111111111111111111111111111u;
            public const int ItemShift1 = 0;
        }

        /// <summary>
        /// Lookup struct for managing items of a specific type in a base <see cref="HashList{T}"/>.
        /// </summary>
        /// <typeparam name="TFilter">Type for which to filter.</typeparam>
        /// <param name="list"></param>
        public struct Lookup<TFilter>(ref HashList<TBase> list) where TFilter : TBase
        {
            private byte[] lookups = [];
            private readonly ref HashList<TBase> List = list;
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private uint stored;
        private PackingMode mode;
        private uint[] flags;
        private TBase[] items;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Checks if <see cref="HashList{TBase}"/> has <typeparamref name="TItem"/> defined.
        /// </summary>
        public readonly bool Has<TItem>() where TItem : TBase
        {
            uint flagsIndex;
            switch (mode)
            {
                case PackingMode.Size0: return false;
                case PackingMode.Size1to8:
                    flagsIndex = ID<TItem>.Index >> 3;
                    if (flagsIndex >= flags.Length)
                    {
                        return false; // Handles out-of-bounds.
                    }

                    return (ID<TItem>.Index & 0b111) switch
                    {
                        0 => (flags[flagsIndex] & Packing1to8.ItemFlag1) != 0,
                        1 => (flags[flagsIndex] & Packing1to8.ItemFlag2) != 0,
                        2 => (flags[flagsIndex] & Packing1to8.ItemFlag3) != 0,
                        3 => (flags[flagsIndex] & Packing1to8.ItemFlag4) != 0,
                        4 => (flags[flagsIndex] & Packing1to8.ItemFlag5) != 0,
                        5 => (flags[flagsIndex] & Packing1to8.ItemFlag6) != 0,
                        6 => (flags[flagsIndex] & Packing1to8.ItemFlag7) != 0,
                        _ => (flags[flagsIndex] & Packing1to8.ItemFlag8) != 0,
                    };

                case PackingMode.Size9to128:
                    flagsIndex = ID<TItem>.Index >> 2;
                    if (flagsIndex >= flags.Length)
                    {
                        return false; // Handles out-of-bounds.
                    }

                    return (ID<TItem>.Index & 0b11) switch
                    {
                        0 => (flags[flagsIndex] & Packing9to128.ItemFlag1) != 0,
                        1 => (flags[flagsIndex] & Packing9to128.ItemFlag2) != 0,
                        2 => (flags[flagsIndex] & Packing9to128.ItemFlag3) != 0,
                        _ => (flags[flagsIndex] & Packing9to128.ItemFlag4) != 0,
                    };

                case PackingMode.Size129to32768:
                    flagsIndex = ID<TItem>.Index >> 1;
                    if (flagsIndex >= flags.Length)
                    {
                        return false; // Handles out-of-bounds.
                    }

                    return (ID<TItem>.Index & 0b1) switch
                    {
                        0 => (flags[flagsIndex] & Packing129to32768.ItemFlag1) != 0,
                        _ => (flags[flagsIndex] & Packing129to32768.ItemFlag2) != 0,
                    };

                case PackingMode.Size32769to2147483647:
                default:
                    return ID<TItem>.Index < flags.Length // Handles out-of-bounds.
                        && (flags[ID<TItem>.Index] & Packing32769to2147483647.ItemFlag1) != 0;
            }
        }

        public bool Add<TItem>(TItem item) where TItem : TBase
        {
            uint flagsIndex;
            int capacity = GetModeCapacity(mode);
            if (stored + 1 > capacity)
            {
                flagsIndex = (mode += 1) switch
                {
                    PackingMode.Size0 => 0,
                    PackingMode.Size1to8 => ID<TItem>.Index >> 3,
                    PackingMode.Size9to128 => ID<TItem>.Index >> 2,
                    PackingMode.Size129to32768 => ID<TItem>.Index >> 1,
                    _ => ID<TItem>.Index,
                };

                // Remap & Resize to a requires size.
                throw new NotImplementedException();
            }
            else
            {
                flagsIndex = mode switch
                {
                    PackingMode.Size0 => 0,
                    PackingMode.Size1to8 => ID<TItem>.Index >> 3,
                    PackingMode.Size9to128 => ID<TItem>.Index >> 2,
                    PackingMode.Size129to32768 => ID<TItem>.Index >> 1,
                    _ => ID<TItem>.Index,
                };

                // Resize if needed.
            }

            switch (mode)
            {
                case PackingMode.Size0: return false;
                case PackingMode.Size1to8:
                    ref uint flagsRef1 = ref flags[flagsIndex];
                    if ((flagsRef1 & (ID<TItem>.Index & 0b111) switch
                    {
                        0 => Packing1to8.ItemFlag1,
                        1 => Packing1to8.ItemFlag2,
                        2 => Packing1to8.ItemFlag3,
                        3 => Packing1to8.ItemFlag4,
                        4 => Packing1to8.ItemFlag5,
                        5 => Packing1to8.ItemFlag6,
                        6 => Packing1to8.ItemFlag7,
                        _ => Packing1to8.ItemFlag8,
                    }) != 0)
                    {
                        return false; // Item already stored.
                    }

                    items[stored++] = item;
                    flagsRef1 |= (ID<TItem>.Index & 0b111) switch
                    {
                        0 => Packing1to8.ItemFlag1 | (ID<TItem>.Index << Packing1to8.ItemShift1),
                        1 => Packing1to8.ItemFlag2 | (ID<TItem>.Index << Packing1to8.ItemShift2),
                        2 => Packing1to8.ItemFlag3 | (ID<TItem>.Index << Packing1to8.ItemShift3),
                        3 => Packing1to8.ItemFlag4 | (ID<TItem>.Index << Packing1to8.ItemShift4),
                        4 => Packing1to8.ItemFlag5 | (ID<TItem>.Index << Packing1to8.ItemShift5),
                        5 => Packing1to8.ItemFlag6 | (ID<TItem>.Index << Packing1to8.ItemShift6),
                        6 => Packing1to8.ItemFlag7 | (ID<TItem>.Index << Packing1to8.ItemShift7),
                        _ => Packing1to8.ItemFlag8 | (ID<TItem>.Index << Packing1to8.ItemShift8),
                    };
                    return true;

                case PackingMode.Size9to128:
                    ref uint flagsRef2 = ref flags[flagsIndex];
                    if ((flagsRef2 & (ID<TItem>.Index & 0b11) switch
                    {
                        0 => Packing9to128.ItemFlag1,
                        1 => Packing9to128.ItemFlag2,
                        2 => Packing9to128.ItemFlag3,
                        _ => Packing9to128.ItemFlag4,
                    }) != 0)
                    {
                        return false; // Item already stored.
                    }

                    items[stored++] = item;
                    flagsRef2 |= (ID<TItem>.Index & 0b11) switch
                    {
                        0 => Packing1to8.ItemFlag1 | (ID<TItem>.Index << Packing1to8.ItemShift1),
                        1 => Packing1to8.ItemFlag2 | (ID<TItem>.Index << Packing1to8.ItemShift2),
                        2 => Packing1to8.ItemFlag3 | (ID<TItem>.Index << Packing1to8.ItemShift3),
                        _ => Packing1to8.ItemFlag4 | (ID<TItem>.Index << Packing1to8.ItemShift4),
                    };
                    return true;

                case PackingMode.Size129to32768:
                    ref uint flagsRef3 = ref flags[flagsIndex];
                    if ((flagsRef3 & (ID<TItem>.Index & 0b1) switch
                    {
                        0 => Packing129to32768.ItemFlag1,
                        _ => Packing129to32768.ItemFlag2,
                    }) != 0)
                    {
                        return false; // Item already stored.
                    }

                    items[stored++] = item;
                    flagsRef3 |= (ID<TItem>.Index & 0b1) switch
                    {
                        0 => Packing129to32768.ItemFlag1 | (ID<TItem>.Index << Packing129to32768.ItemShift1),
                        _ => Packing129to32768.ItemFlag2 | (ID<TItem>.Index << Packing129to32768.ItemShift2),
                    };
                    return true;

                case PackingMode.Size32769to2147483647:
                default:
                    ref uint flagsRef4 = ref flags[flagsIndex];
                    if ((flagsRef4 & Packing32769to2147483647.ItemFlag1) != 0)
                    {
                        return false; // Item already stored.
                    }

                    items[stored++] = item;
                    flagsRef4 |= Packing32769to2147483647.ItemFlag1 | (ID<TItem>.Index << Packing32769to2147483647.ItemShift1);
                    return true;
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                 Experiments
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        sealed class Experiments
        {
            const uint ItemMask1 = 0b0000_0000_0000_0111u;
            const uint ItemMask2 = 0b0000_0000_0111_0000u;
            const uint ItemMask3 = 0b0000_0111_0000_0000u;
            const uint ItemMask4 = 0b0111_0000_0000_0000u;
            const uint ItemFlag1 = 0b0000_0000_0000_1000u;
            const uint ItemFlag2 = 0b0000_0000_1000_0000u;
            const uint ItemFlag3 = 0b0000_1000_0000_0000u;
            const uint ItemFlag4 = 0b1000_0000_0000_0000u;
            const int ShiftToOrigin = 16;
            const int ItemShift1 = 0;
            const int ItemShift2 = 4;
            const int ItemShift3 = 8;
            const int ItemShift4 = 12;

            readonly uint[] flags = [];
            readonly object[] items = [];
            public bool Has(int id)
            {
                int region = id >> 2;
                if (region >= flags.Length)
                {
                    return false; // Handles out-of-range.
                }

                return (flags[region] & (id & 0b11u) switch
                {
                    0 => ItemFlag1,
                    1 => ItemFlag2,
                    2 => ItemFlag3,
                    3 => ItemFlag4,
                    _ => throw new SwitchExpressionException(id & 0b11u),
                }) != 0;
            }

            public bool TryGet(int id, [NotNullWhen(true)] out object? item)
            {
                int region = id >> 2;
                if (region >= this.flags.Length)
                {
                    item = default!;
                    return false; // Handles out-of-range.
                }

                uint flags = this.flags[region];
                if ((this.flags[region] & (id & 0b11u) switch
                {
                    0 => ItemFlag1,
                    1 => ItemFlag2,
                    2 => ItemFlag3,
                    3 => ItemFlag4,
                    _ => throw new SwitchExpressionException(id & 0b11u),
                }) == 0)
                {
                    item = default!;
                    return false; // Item does not exist.
                }

                item = items[(flags >> ShiftToOrigin) + (id & 0b11) switch
                {
                    0 => (flags & ItemMask1) >> ItemShift1,
                    1 => (flags & ItemMask2) >> ItemShift2,
                    2 => (flags & ItemMask3) >> ItemShift3,
                    3 => (flags & ItemMask4) >> ItemShift4,
                    _ => throw new SwitchExpressionException(id & 0b11),
                }];
                return true;
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>





        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Static Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        public static Lookup<TItem> GetLookup<TItem>(ref HashList<TBase> list) where TItem : TBase
        {
            if (typeof(TItem) == typeof(TBase))
            {
                throw new InvalidLookupTypeException(typeof(TItem));
            }

            return new Lookup<TItem>(ref list);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int GetModeCapacity(PackingMode mode) => mode switch
        {
            PackingMode.Size0 => 0,
            PackingMode.Size1to8 => Packing1to8.Capacity,
            PackingMode.Size9to128 => Packing9to128.Capacity,
            PackingMode.Size129to32768 => Packing129to32768.Capacity,
            _ => Packing32769to2147483647.Capacity,
        };
    }

    /// <summary>
    /// Useful extension methods when working with <see cref="HashList{T}"/>.
    /// </summary>
    public static class HashListExtensions
    {
        /// <inheritdoc cref="HashList{T}.GetLookup{TItem}(ref HashList{T})"/>
        public static HashList<T>.Lookup<TItem> GetLookup<T, TItem>(this ref HashList<T> list) where TItem : T
        {
            return HashList<T>.GetLookup<TItem>(ref list);
        }
    }
}
