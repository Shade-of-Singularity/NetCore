using System;
using System.Diagnostics.CodeAnalysis;

namespace NetCore.Common
{
    /// <summary>
    /// Stores trivia about all <see cref="QuickList{TBase}"/> instances.
    /// </summary>
    public readonly struct QuickMaps
    {
        /// <summary>
        /// Maximum amount of items an <see cref="QuickList{TBase}"/> can hold.
        /// </summary>
        public const int ItemLimit = 19; // Technical limitation.
    }

    /// <summary>
    /// Dictionary, optimized to quickly lookup specific items using CRTP internally.
    /// Around 3.5 times faster than <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/>.
    /// </summary>
    /// <remarks>
    /// (1) Not thread-safe by itself.
    /// (2) Has a technical limit of items. See also: <see cref="BaseSet.ItemLimit"/>
    /// </remarks>
    /// <typeparam name="TBase">Item type to store.</typeparam>
    /// Note: By avoiding using (already removed) <see cref="QuickMapIndex"/> usage and by using only <see cref="ID{TItem}.BitFlag"/> instead
    ///  we can increase the amount of storable items to the amount of bits in a <see cref="flags"/> variable.
    ///  If we use ushort - the limit will be 16.
    ///  If we use uint - the limit will be 32.
    ///  If we use ulong - the limit will be 64.
    /// But it will be come a bit slower, because we will need to calculate (once) the <see cref="PopCount(uint)"/> on each Get operation.
    /// We can introduce it if benchmarks of it will be successful.
    public struct QuickMap<TBase> where TBase : class
    {
        enum IndexMask : ulong
        {
            One = 0b_______00000_00000_00000_0000_0000_0000_0000_0000_0000_0000_0000_000_000_000_000_00_00_0uL, // of: 0
            Two = 0b_______00000_00000_00000_0000_0000_0000_0000_0000_0000_0000_0000_000_000_000_000_00_00_1uL, // of: 0
            Three = 0b_____00000_00000_00000_0000_0000_0000_0000_0000_0000_0000_0000_000_000_000_000_00_11_0uL, // of: 1
            Four = 0b______00000_00000_00000_0000_0000_0000_0000_0000_0000_0000_0000_000_000_000_000_11_00_0uL, // of: 3
            Five = 0b______00000_00000_00000_0000_0000_0000_0000_0000_0000_0000_0000_000_000_000_111_00_00_0uL, // of: 5
            Six = 0b_______00000_00000_00000_0000_0000_0000_0000_0000_0000_0000_0000_000_000_111_000_00_00_0uL, // of: 8
            Seven = 0b_____00000_00000_00000_0000_0000_0000_0000_0000_0000_0000_0000_000_111_000_000_00_00_0uL, // of: 11
            Eight = 0b_____00000_00000_00000_0000_0000_0000_0000_0000_0000_0000_0000_111_000_000_000_00_00_0uL, // of: 14
            Nine = 0b______00000_00000_00000_0000_0000_0000_0000_0000_0000_0000_1111_000_000_000_000_00_00_0uL, // of: 17
            Ten = 0b_______00000_00000_00000_0000_0000_0000_0000_0000_0000_1111_0000_000_000_000_000_00_00_0uL, // of: 21
            Eleven = 0b____00000_00000_00000_0000_0000_0000_0000_0000_1111_0000_0000_000_000_000_000_00_00_0uL, // of: 25
            Twelve = 0b____00000_00000_00000_0000_0000_0000_0000_1111_0000_0000_0000_000_000_000_000_00_00_0uL, // of: 29
            Thirteen = 0b__00000_00000_00000_0000_0000_0000_1111_0000_0000_0000_0000_000_000_000_000_00_00_0uL, // of: 33
            Fourteen = 0b__00000_00000_00000_0000_0000_1111_0000_0000_0000_0000_0000_000_000_000_000_00_00_0uL, // of: 37
            Fifteen = 0b___00000_00000_00000_0000_1111_0000_0000_0000_0000_0000_0000_000_000_000_000_00_00_0uL, // of: 41
            Sixteen = 0b___00000_00000_00000_1111_0000_0000_0000_0000_0000_0000_0000_000_000_000_000_00_00_0uL, // of: 45
            Seventeen = 0b_00000_00000_11111_0000_0000_0000_0000_0000_0000_0000_0000_000_000_000_000_00_00_0uL, // of: 49
            Eighteen = 0b__00000_11111_00000_0000_0000_0000_0000_0000_0000_0000_0000_000_000_000_000_00_00_0uL, // of: 54
            Nineteen = 0b__11111_00000_00000_0000_0000_0000_0000_0000_0000_0000_0000_000_000_000_000_00_00_0uL, // of: 59
        }

        enum IndexPosition : byte
        {
            One = 0,
            Two = 0,
            Three = 1,
            Four = 3,
            Five = 5,
            Six = 8,
            Seven = 11,
            Eight = 14,
            Nine = 17,
            Ten = 21,
            Eleven = 25,
            Twelve = 29,
            Thirteen = 33,
            Fourteen = 37,
            Fifteen = 41,
            Sixteen = 45,
            Seventeen = 49,
            Eighteen = 54,
            Nineteen = 59,
        }

        static class BaseSet
        {
            static byte inUse;
            public static void NextSet(out ulong mask, out int shift, out uint bitFlag)
            {
                if (inUse >= QuickMaps.ItemLimit)
                {
                    throw new Exception($"{nameof(QuickList<TBase>)} - exhausted all item IDs ({inUse}/{QuickMaps.ItemLimit})");
                }

                int index = inUse;
                shift = (int)(index switch
                {
                    0 => IndexPosition.One,
                    1 => IndexPosition.Two,
                    2 => IndexPosition.Three,
                    3 => IndexPosition.Four,
                    4 => IndexPosition.Five,
                    5 => IndexPosition.Six,
                    6 => IndexPosition.Seven,
                    7 => IndexPosition.Eight,
                    8 => IndexPosition.Nine,
                    9 => IndexPosition.Ten,
                    10 => IndexPosition.Eleven,
                    11 => IndexPosition.Twelve,
                    12 => IndexPosition.Thirteen,
                    13 => IndexPosition.Fourteen,
                    14 => IndexPosition.Fifteen,
                    15 => IndexPosition.Sixteen,
                    16 => IndexPosition.Seventeen,
                    17 => IndexPosition.Eighteen,
                    18 => IndexPosition.Nineteen,
                    _ => throw new ArgumentOutOfRangeException(nameof(index)),
                });
                mask = (ulong)(index switch
                {
                    0 => IndexMask.One,
                    1 => IndexMask.Two,
                    2 => IndexMask.Three,
                    3 => IndexMask.Four,
                    4 => IndexMask.Five,
                    5 => IndexMask.Six,
                    6 => IndexMask.Seven,
                    7 => IndexMask.Eight,
                    8 => IndexMask.Nine,
                    9 => IndexMask.Ten,
                    10 => IndexMask.Eleven,
                    11 => IndexMask.Twelve,
                    12 => IndexMask.Thirteen,
                    13 => IndexMask.Fourteen,
                    14 => IndexMask.Fifteen,
                    15 => IndexMask.Sixteen,
                    16 => IndexMask.Seventeen,
                    17 => IndexMask.Eighteen,
                    18 => IndexMask.Nineteen,
                    _ => throw new ArgumentOutOfRangeException(nameof(index)),
                });
                bitFlag = 1u << index;
                inUse++;
            }
        }

        static class ID<TItem>
        {
            public static readonly int Shift;
            public static readonly uint BitFlag;
            public static readonly ulong Mask;
            static ID() => BaseSet.NextSet(out Mask, out Shift, out BitFlag);
        }


        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .                                           (Note: do not reorder)
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private int stored; // Amount of items stored in the array. Array capacity might differ from this value.
        private uint flags; // Stores (true/false) states about whether a specific item is present in the array or not.
        private ulong lookup; // Encodes local index of all items in one lookup table.
        private TBase?[] values; // Stores items themselves. Resized if you add new values. Limited to 15 in size.




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Default constructor. Initialized <see cref="QuickMap{T}"/> with an empty array.
        /// </summary>
        public QuickMap() => values = [];

        /// <summary>
        /// Constructor allowing you to specify how much space (from 0 to <see cref="QuickMaps.ItemLimit"/>) to pre-allocate in the internal array.
        /// </summary>
        /// <param name="capacity">Capacity for the internal array.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> was larger than <see cref="QuickMaps.ItemLimit"/></exception>
        public QuickMap(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException($"{nameof(capacity)} in {nameof(QuickMap<TBase>)} constructor cannot be less than 0. Provided: {capacity}");
            if (capacity > QuickMaps.ItemLimit)
            {
                throw new ArgumentOutOfRangeException($"Cannot create QuickMap with a {capacity}, larger than {QuickMaps.ItemLimit} (technical limit).");
            }

            values = new TBase[capacity];
        }

        /// <returns>
        /// <c>true</c> if item was added.
        /// <c>false</c> if item of the same <typeparamref name="TItem"/> is already in the map.
        /// </returns>
        public bool Add<TItem>(TItem item) where TItem : class, TBase
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            // QuickID will throw if item cannot be stored. Thus some checks can be removed.
            uint flag = ID<TItem>.BitFlag;
            if ((flags & flag) != 0)
            {
                // Item is was already stored.
                return false;
            }

            InsertAtUnchecked(ref values, ref stored, PopCount(flag - 1), item);
            flags |= flag;
            UpdateLookup(flags, out lookup);
            return true;
        }

        /// <summary>
        /// Sets (adds or replaces) an item under its type in the internal array.
        /// </summary>
        /// <remarks>
        /// Providing <c>null</c> will make it call <see cref="Remove{TItem}()"/> instead.
        /// </remarks>
        public void Set<TItem>(TItem item) where TItem : class, TBase
        {
            if (item is null)
            {
                Remove<TItem>();
                return;
            }

            // QuickID will throw if item cannot be stored. Thus some checks can be removed.
            uint flag = ID<TItem>.BitFlag;
            if ((flags & flag) != 0)
            {
                // Item under a specific order already exist.
                // Replacing it won't rebuild the lookup table and won't require an array resize.
                values[(lookup & ID<TItem>.Mask) >> ID<TItem>.Shift] = item;
                return;
            }

            InsertAtUnchecked(ref values, ref stored, PopCount(flag - 1), item);
            flags |= flag;
            UpdateLookup(flags, out lookup);
        }

        /// <summary>
        /// Attempts to retrieve <typeparamref name="TItem"/> instance from the map.
        /// </summary>
        /// <returns><c>true</c> if found. <c>false</c> otherwise.</returns>
        public readonly bool TryGet<TItem>([NotNullWhen(true)] out TItem? item) where TItem : class, TBase
        {
            if ((flags & ID<TItem>.BitFlag) == 0)
            {
                item = default;
                return false;
            }

            item = (TItem)values[(lookup & ID<TItem>.Mask) >> ID<TItem>.Shift]!;
            return true;
        }

        /// <summary>
        /// Retrieves <typeparamref name="TItem"/> instance from the map.
        /// </summary>
        /// <returns>Instance of <typeparamref name="TItem"/> from the map, or <c>null</c> if not found.</returns>
        public readonly TItem? Get<TItem>() where TItem : class, TBase => (flags & ID<TItem>.BitFlag) switch
        {
            0 => null,
            _ => (TItem?)values[(lookup & ID<TItem>.Mask) >> ID<TItem>.Shift]
        };

        /// <summary>
        /// Checks if item of a specific <typeparamref name="TItem"/> type exist exist in the internal map.
        /// </summary>
        /// <returns><c>true</c> if item exist. <c>false</c> otherwise.</returns>
        public readonly bool Has<TItem>() where TItem : class, TBase => (flags & ID<TItem>.BitFlag) != 0;

        /// <summary>
        /// Removes specific <paramref name="item"/> of a type <typeparamref name="TItem"/> from the map.
        /// </summary>
        /// <returns><c>true</c> if item was removed. <c>false</c> if item wasn't present in the map to begin with.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <c>null</c>.</exception>
        public bool Remove<TItem>(TItem item) where TItem : class, TBase
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            // QuickID will throw if item cannot be stored. Thus some checks can be removed.
            uint flag = ID<TItem>.BitFlag;
            if ((flags & flag) == 0)
            {
                return false; // Item is was already removed.
            }

            RemoveAtUnchecked(ref values, ref stored, (int)((lookup & ID<TItem>.Mask) >> ID<TItem>.Shift));
            flags &= ~flag;
            UpdateLookup(flags, out lookup);
            return true;
        }

        /// <summary>
        /// Removes specific any item under a type <typeparamref name="TItem"/> from the map.
        /// </summary>
        /// <returns><c>true</c> if item was removed. <c>false</c> if item wasn't present in the map to begin with.</returns>
        public bool Remove<TItem>() where TItem : class, TBase
        {
            // QuickID will throw if item cannot be stored. Thus some checks can be removed.
            uint flag = ID<TItem>.BitFlag;
            if ((flags & flag) == 0)
            {
                // Item is was already removed.
                return false;
            }

            RemoveAtUnchecked(ref values, ref stored, (int)((lookup & ID<TItem>.Mask) >> ID<TItem>.Shift));
            flags &= ~flag;
            UpdateLookup(flags, out lookup);
            return true;
        }

        /// <summary>
        /// Removes specific any item under a type <typeparamref name="TItem"/> from the map.
        /// </summary>
        /// <param name="removed">Item that was removed from the map.</param>
        /// <returns><c>true</c> if item was removed. <c>false</c> if item wasn't present in the map to begin with.</returns>
        public bool Remove<TItem>([NotNullWhen(true)] out TItem? removed) where TItem : class, TBase
        {
            // QuickID will throw if item cannot be stored. Thus some checks can be removed.
            uint flag = ID<TItem>.BitFlag;
            if ((flags & flag) == 0)
            {
                // Item is was already removed.
                removed = default;
                return false;
            }

            int localIndex = (int)((lookup & ID<TItem>.Mask) >> ID<TItem>.Shift);
            removed = (TItem)values[localIndex]!;
            RemoveAtUnchecked(ref values, ref stored, localIndex);
            flags &= ~flag;
            UpdateLookup(flags, out lookup);
            return true;
        }

        /// <summary>
        /// Retrieves struct-based enumerator for enumerating over all value of this <see cref="QuickMap{T}"/>.
        /// </summary>
        /// <returns>Struct-based enumerator.</returns>
        public readonly QuickMapEnumerator GetEnumerator() => new(values!, stored);

        /// <summary>
        /// Struct-based enumerator used for zero allocation enumeration over the internal array of a <see cref="QuickMap{T}"/>.
        /// </summary>
        /// <param name="values">Array of all values from a <see cref="QuickMap{T}"/>.</param>
        /// <param name="stored">Amount of values stored in the beginning of <paramref name="values"/> array.</param>
        public ref struct QuickMapEnumerator(TBase[] values, int stored)
        {
            private readonly TBase[] values = values;
            private readonly int stored = stored;
            private int index = -1;

            /// <inheritdoc/>
            public readonly TBase Current => index < stored ? values[index] : default!;

            /// <summary>
            /// Moves pointer to the next index forward.
            /// </summary>
            /// <returns>Whether there are more items in a sequence.</returns>
            public bool MoveNext() => ++index < stored;
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Recalculates a lookup table based on input <paramref name="flags"/>.
        /// </summary>
        /// Note: There is no way invented to update a lookup table partially, so we update it in full instead.
        ///  The problem is to understand how many items were already added...
        /// <param name="flags"></param>
        /// <param name="lookup"></param>
        private static void UpdateLookup(uint flags, out ulong lookup)
        {
            ulong stored = 0uL;
            lookup = 0uL;

            for (int order = 0; order < QuickMaps.ItemLimit; order++)
            {
                if ((flags & (1u << order)) != 0)
                {
                    lookup |= stored << (int)(order switch
                    {
                        0 => IndexPosition.One,
                        1 => IndexPosition.Two,
                        2 => IndexPosition.Three,
                        3 => IndexPosition.Four,
                        4 => IndexPosition.Five,
                        5 => IndexPosition.Six,
                        6 => IndexPosition.Seven,
                        7 => IndexPosition.Eight,
                        8 => IndexPosition.Nine,
                        9 => IndexPosition.Ten,
                        10 => IndexPosition.Eleven,
                        11 => IndexPosition.Twelve,
                        12 => IndexPosition.Thirteen,
                        13 => IndexPosition.Fourteen,
                        14 => IndexPosition.Fifteen,
                        15 => IndexPosition.Sixteen,
                        16 => IndexPosition.Seventeen,
                        17 => IndexPosition.Eighteen,
                        18 => IndexPosition.Nineteen,
                        _ => throw new ArgumentOutOfRangeException(nameof(order)),
                    });

                    stored++;
                }
            }
        }

        private static void InsertAtUnchecked(ref TBase?[] values, ref int stored, int localIndex, TBase item)
        {
            int newStored = stored + 1;
            if (newStored >= values.Length)
            {
                var array = new TBase?[newStored];
                Array.Copy(values, 0, array, 0, localIndex);
                Array.Copy(values, localIndex, array, localIndex + 1, stored - localIndex);
                values = array;
            }

            // Moves other items to the front.
            for (int i = newStored - 1; i > localIndex; i--)
            {
                values[i] = values[i - 1];
            }

            values[localIndex] = item;
            stored = newStored;
        }

        private static void RemoveAtUnchecked(ref TBase?[] values, ref int stored, int localIndex)
        {
            for (int i = localIndex + 1; i < stored; i++)
            {
                values[i - 1] = values[i];
            }

            values[--stored] = default;
        }

        /// <summary>
        /// Pop counter for ushort values.
        /// </summary>
        private static int PopCount(uint x)
        {
            x -= (x >> 1) & 0x55555555u;
            x = (x & 0x33333333u) + ((x >> 2) & 0x33333333u);
            return (int)((((x + (x >> 4)) & 0x0F0F0F0Fu) * 0x01010101u) >> 24);
        }
    }
}
