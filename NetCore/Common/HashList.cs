using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetCore.Common
{
    /// <summary>
    /// Exception for handling when you use <see cref="HashList{T}.GetLookup{TItem}"/>
    /// with TItem matching a base type of the <see cref="HashList{T}"/>.
    /// </summary>
    /// <param name="type">Target item type.</param>
    public sealed class InvalidLookupTypeException(Type type)
        : Exception($"Cannot use ({type.Name}) as a type for a Lookup, as it matches the type of a HashList. Use HashList directly instead.") { }

    /// <summary>
    /// Struct-based hash list, for storing unique strong-type items.
    /// Optimized to be a lot faster than <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> using CRTP.
    /// </summary>
    /// <remarks>
    /// Internally, encodes invalid indexes as '0' instead of '-1' and such.
    /// Because of that, mapping resizing will not happen when adding 17th item, but a 16th instead.
    /// Another resize will happen when adding 256th item instead of 257th.
    /// </remarks>
    /// <typeparam name="TBase">Base type of an item.</typeparam>
    public struct HashList<TBase>
    {
        static class Indexing
        {
            static int index = 0;
            public static int NextIndex() => index++;
        }

        static class ID<TItem> where TItem : TBase
        {
            /// <summary>
            /// Initialization order of this item.
            /// </summary>
            public static readonly int Index = Indexing.NextIndex();
        }

        enum PackingMode : byte
        {
            Size0to15,
            Size16to255,
            Size256to65535,
        }

        static class Packing0to15
        {
            public const ulong Item1 = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111uL;
            public const ulong Item2 = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_0000uL;
            public const ulong Item3 = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_0000_0000uL;
            public const ulong Item4 = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_0000_0000_0000uL;
            public const ulong Item5 = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_0000_0000_0000_0000uL;
            public const ulong Item6 = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_0000_0000_0000_0000_0000uL;
            public const ulong Item7 = 0b_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_0000_0000_0000_0000_0000_0000uL;
            public const ulong Item8 = 0b_0000_0000_0000_0000_0000_0000_0000_0000_1111_0000_0000_0000_0000_0000_0000_0000uL;
            public const ulong Item9 = 0b_0000_0000_0000_0000_0000_0000_0000_1111_0000_0000_0000_0000_0000_0000_0000_0000uL;
            public const ulong Item10 = 0b0000_0000_0000_0000_0000_0000_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000uL;
            public const ulong Item11 = 0b0000_0000_0000_0000_0000_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000uL;
            public const ulong Item12 = 0b0000_0000_0000_0000_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000uL;
            public const ulong Item13 = 0b0000_0000_0000_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000uL;
            public const ulong Item14 = 0b0000_0000_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000uL;
            public const ulong Item15 = 0b0000_1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000uL;
            public const ulong Item16 = 0b1111_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000uL;
            public const int ShiftItem1 = 0;
            public const int ShiftItem2 = 4;
            public const int ShiftItem3 = 8;
            public const int ShiftItem4 = 12;
            public const int ShiftItem5 = 16;
            public const int ShiftItem6 = 20;
            public const int ShiftItem7 = 24;
            public const int ShiftItem8 = 28;
            public const int ShiftItem9 = 32;
            public const int ShiftItem10 = 36;
            public const int ShiftItem11 = 40;
            public const int ShiftItem12 = 44;
            public const int ShiftItem13 = 48;
            public const int ShiftItem14 = 52;
            public const int ShiftItem15 = 56;
            public const int ShiftItem16 = 60;
        }

        static class Packing16to255
        {
            public const ulong Item1 = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_11111111uL;
            public const ulong Item2 = 0b00000000_00000000_00000000_00000000_00000000_00000000_11111111_00000000uL;
            public const ulong Item3 = 0b00000000_00000000_00000000_00000000_00000000_11111111_00000000_00000000uL;
            public const ulong Item4 = 0b00000000_00000000_00000000_00000000_11111111_00000000_00000000_00000000uL;
            public const ulong Item5 = 0b00000000_00000000_00000000_11111111_00000000_00000000_00000000_00000000uL;
            public const ulong Item6 = 0b00000000_00000000_11111111_00000000_00000000_00000000_00000000_00000000uL;
            public const ulong Item7 = 0b00000000_11111111_00000000_00000000_00000000_00000000_00000000_00000000uL;
            public const ulong Item8 = 0b11111111_00000000_00000000_00000000_00000000_00000000_00000000_00000000uL;
            public const int ShiftItem1 = 0;
            public const int ShiftItem2 = 8;
            public const int ShiftItem3 = 16;
            public const int ShiftItem4 = 24;
            public const int ShiftItem5 = 32;
            public const int ShiftItem6 = 40;
            public const int ShiftItem7 = 48;
            public const int ShiftItem8 = 56;
        }

        static class Packing256to65535
        {
            public const ulong Item1 = 0b0000000000000000_0000000000000000_0000000000000000_1111111111111111uL;
            public const ulong Item2 = 0b0000000000000000_0000000000000000_1111111111111111_0000000000000000uL;
            public const ulong Item3 = 0b0000000000000000_1111111111111111_0000000000000000_0000000000000000uL;
            public const ulong Item4 = 0b1111111111111111_0000000000000000_0000000000000000_0000000000000000uL;
            public const int ShiftItem1 = 0;
            public const int ShiftItem2 = 16;
            public const int ShiftItem3 = 32;
            public const int ShiftItem4 = 48;
        }

        /// <summary>
        /// Lookup struct for managing items of a specific type in a base <see cref="HashList{T}"/>.
        /// </summary>
        /// <typeparam name="TFilter">Type for which to filter.</typeparam>
        /// <param name="list"></param>
        public struct Lookup<TFilter>(ref HashList<TBase> list) where TFilter : TBase
        {
            private readonly ref HashList<TBase> List = list; 
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private int stored;
        private PackingMode mode;
        private ulong[] positions;
        private TBase[] items;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        int GetIndex<TItem>() where TItem : TBase
        {
            int flagsIndex;
            switch (mode)
            {
                case PackingMode.Size0to15:
                    flagsIndex = ID<TItem>.Index >> 4;
                    if (flagsIndex > positions.Length)
                    {
                        return -1; // Handles out-of-bounds.
                    }

                    return (ID<TItem>.Index & 0b1111) switch
                    {
                        0 => (int)((positions[flagsIndex] & Packing0to15.Item1) >> Packing0to15.ShiftItem1),
                        1 => (int)((positions[flagsIndex] & Packing0to15.Item2) >> Packing0to15.ShiftItem2),
                        2 => (int)((positions[flagsIndex] & Packing0to15.Item3) >> Packing0to15.ShiftItem3),
                        3 => (int)((positions[flagsIndex] & Packing0to15.Item4) >> Packing0to15.ShiftItem4),
                        4 => (int)((positions[flagsIndex] & Packing0to15.Item5) >> Packing0to15.ShiftItem5),
                        5 => (int)((positions[flagsIndex] & Packing0to15.Item6) >> Packing0to15.ShiftItem6),
                        6 => (int)((positions[flagsIndex] & Packing0to15.Item7) >> Packing0to15.ShiftItem7),
                        7 => (int)((positions[flagsIndex] & Packing0to15.Item8) >> Packing0to15.ShiftItem8),
                        8 => (int)((positions[flagsIndex] & Packing0to15.Item9) >> Packing0to15.ShiftItem9),
                        9 => (int)((positions[flagsIndex] & Packing0to15.Item10) >> Packing0to15.ShiftItem10),
                        10 => (int)((positions[flagsIndex] & Packing0to15.Item11) >> Packing0to15.ShiftItem11),
                        11 => (int)((positions[flagsIndex] & Packing0to15.Item12) >> Packing0to15.ShiftItem12),
                        12 => (int)((positions[flagsIndex] & Packing0to15.Item13) >> Packing0to15.ShiftItem13),
                        13 => (int)((positions[flagsIndex] & Packing0to15.Item14) >> Packing0to15.ShiftItem14),
                        14 => (int)((positions[flagsIndex] & Packing0to15.Item15) >> Packing0to15.ShiftItem15),
                        _ => throw new SwitchExpressionException(ID<TItem>.Index & 0b1111),
                    };

                case PackingMode.Size16to255:
                    flagsIndex = ID<TItem>.Index >> 3;
                    if (flagsIndex > positions.Length)
                    {
                        return -1; // Handles out-of-bounds.
                    }

                    return (ID<TItem>.Index & 0b111) switch
                    {
                        0 => (int)((positions[flagsIndex] & Packing16to255.Item1) >> Packing16to255.ShiftItem1),
                        1 => (int)((positions[flagsIndex] & Packing16to255.Item2) >> Packing16to255.ShiftItem2),
                        2 => (int)((positions[flagsIndex] & Packing16to255.Item3) >> Packing16to255.ShiftItem3),
                        3 => (int)((positions[flagsIndex] & Packing16to255.Item4) >> Packing16to255.ShiftItem4),
                        4 => (int)((positions[flagsIndex] & Packing16to255.Item5) >> Packing16to255.ShiftItem5),
                        5 => (int)((positions[flagsIndex] & Packing16to255.Item6) >> Packing16to255.ShiftItem6),
                        6 => (int)((positions[flagsIndex] & Packing16to255.Item7) >> Packing16to255.ShiftItem7),
                        _ => throw new SwitchExpressionException(ID<TItem>.Index & 0b1111),
                    };

                case PackingMode.Size256to65535:
                    Span<ushort> size16bits = MemoryMarshal.Cast<uint, ushort>(positions.AsSpan());
                    break;
            }
        }

        public bool Has<TItem>() where TItem : TBase => GetIndex<TItem>() != -1;

        public bool Add()
        {

        }




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
