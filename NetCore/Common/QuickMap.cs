using System;
using System.Diagnostics.CodeAnalysis;

namespace NetCore.Common
{
    /// <summary>
    /// Dictionary, optimized to quickly lookup specific items using CRTP internally.
    /// Around 3.5 times faster than <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/>.
    /// </summary>
    /// <remarks>
    /// (1) Not thread-safe by itself.
    /// (2) Has a technical limit of items. See also: <see cref="QuickMapIndex.Limit"/>
    /// </remarks>
    /// <typeparam name="T">Item type to store.</typeparam>
    /// Note: By avoiding using <see cref="QuickMapIndex"/> usage and by using only <see cref="QuickMapID{TItem, TCategory}.BitFlag"/> instead
    ///  we can increase the amount of storable items to the amount of bits in a <see cref="flags"/> variable.
    ///  If we use ushort - the limit will be 16.
    ///  If we use uint - the limit will be 32.
    ///  If we use ulong - the limit will be 64.
    /// But it will be come a bit slower, because we will need to calculate (once) the <see cref="PopCount(int)"/> on each Get operation.
    /// We can introduce it if benchmarks of it will be successful.
    public struct QuickMap<T> where T : class
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .                                           (Note: do not reorder)
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private int stored; // Amount of items stored in the array. Array capacity might differ from this value.
        private uint flags; // Stores (true/false) states about whether a specific item is present in the array or not.
        private ulong lookup; // Encodes local index of all items in one lookup table.
        private T?[] values; // Stores items themselves. Resized if you add new values. Limited to 15 in size.




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
        /// Constructor allowing you to specify how much space (from 0 to <see cref="QuickMapIndex.Limit"/>) to pre-allocate in the internal array.
        /// </summary>
        /// <param name="capacity">Capacity for the internal array.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> was larger than <see cref="QuickMapIndex.Limit"/></exception>
        public QuickMap(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException($"{nameof(capacity)} in {nameof(QuickMap<T>)} constructor cannot be less than 0. Provided: {capacity}");
            if (capacity > QuickMapIndex.Limit)
            {
                throw new ArgumentOutOfRangeException($"Cannot create QuickMap with a {capacity}, larger than {QuickMapIndex.Limit} (technical limit) (seealso: {nameof(QuickMapIndex)}).{nameof(QuickMapIndex.Limit)}");
            }

            values = new T[capacity];
        }

        /// <returns>
        /// <c>true</c> if item was added.
        /// <c>false</c> if item with the same <see cref="Type"/>/<see cref="QuickMapIndex"/> is already in the map.
        /// </returns>
        public bool Add<TItem>(TItem item) where TItem : class, T
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            // QuickID will throw if item cannot be stored. Thus some checks can be removed.
            uint flag = QuickMapID<TItem, T>.BitFlag;
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
        public void Set<TItem>(TItem item) where TItem : class, T
        {
            if (item is null)
            {
                Remove<TItem>();
                return;
            }

            // QuickID will throw if item cannot be stored. Thus some checks can be removed.
            uint flag = QuickMapID<TItem, T>.BitFlag;
            if ((flags & flag) != 0)
            {
                // Item under a specific order already exist.
                // Replacing it won't rebuild the lookup table and won't require an array resize.
                values[(lookup & (ulong)QuickMapID<TItem, T>.Mask) >> (int)QuickMapID<TItem, T>.Position] = item;
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
        public readonly bool TryGet<TItem>([NotNullWhen(true)] out TItem? item) where TItem : class, T => (item = Get<TItem>()) is not null;

        /// <summary>
        /// Retrieves <typeparamref name="TItem"/> instance from the map.
        /// </summary>
        /// <returns>Instance of <typeparamref name="TItem"/> from the map, or <c>null</c> if not found.</returns>
        public readonly TItem? Get<TItem>() where TItem : class, T => (flags & QuickMapID<TItem, T>.BitFlag) switch
        {
            0 => null,
            _ => (TItem)values[(lookup & (ulong)QuickMapID<TItem, T>.Mask) >> (int)QuickMapID<TItem, T>.Position]!
        };

        /// <summary>
        /// Checks if item of a specific <typeparamref name="TItem"/> type exist exist in the internal map.
        /// </summary>
        /// <returns><c>true</c> if item exist. <c>false</c> otherwise.</returns>
        public readonly bool Has<TItem>() where TItem : class, T => (flags & QuickMapID<TItem, T>.BitFlag) != 0;

        /// <summary>
        /// Removes specific <paramref name="item"/> of a type <typeparamref name="TItem"/> from the map.
        /// </summary>
        /// <returns><c>true</c> if item was removed. <c>false</c> if item wasn't present in the map to begin with.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <c>null</c>.</exception>
        public bool Remove<TItem>(TItem item) where TItem : class, T
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            // QuickID will throw if item cannot be stored. Thus some checks can be removed.
            uint flag = QuickMapID<TItem, T>.BitFlag;
            if ((flags & flag) == 0)
            {
                return false; // Item is was already removed.
            }

            RemoveAtUnchecked(ref values, ref stored, (int)((lookup & (ulong)QuickMapID<TItem, T>.Mask) >> (int)QuickMapID<TItem, T>.Position));
            flags &= ~flag;
            UpdateLookup(flags, out lookup);
            return true;
        }

        /// <summary>
        /// Removes specific any item under a type <typeparamref name="TItem"/> from the map.
        /// </summary>
        /// <returns><c>true</c> if item was removed. <c>false</c> if item wasn't present in the map to begin with.</returns>
        public bool Remove<TItem>() where TItem : class, T
        {
            // QuickID will throw if item cannot be stored. Thus some checks can be removed.
            uint flag = QuickMapID<TItem, T>.BitFlag;
            if ((flags & flag) == 0)
            {
                // Item is was already removed.
                return false;
            }

            RemoveAtUnchecked(ref values, ref stored, (int)((lookup & (ulong)QuickMapID<TItem, T>.Mask) >> (int)QuickMapID<TItem, T>.Position));
            flags &= ~flag;
            UpdateLookup(flags, out lookup);
            return true;
        }

        /// <summary>
        /// Removes specific any item under a type <typeparamref name="TItem"/> from the map.
        /// </summary>
        /// <param name="removed">Item that was removed from the map.</param>
        /// <returns><c>true</c> if item was removed. <c>false</c> if item wasn't present in the map to begin with.</returns>
        public bool Remove<TItem>([NotNullWhen(true)] out TItem? removed) where TItem : class, T
        {
            // QuickID will throw if item cannot be stored. Thus some checks can be removed.
            uint flag = QuickMapID<TItem, T>.BitFlag;
            if ((flags & flag) == 0)
            {
                // Item is was already removed.
                removed = default;
                return false;
            }

            int localIndex = (int)((lookup & (ulong)QuickMapID<TItem, T>.Mask) >> (int)QuickMapID<TItem, T>.Position);
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
        public ref struct QuickMapEnumerator(T[] values, int stored)
        {
            private readonly T[] values = values;
            private readonly int stored = stored;
            private int index = -1;

            /// <inheritdoc/>
            public readonly T Current => index < stored ? values[index] : default!;

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

            for (int order = 0; order < QuickMapIndex.Limit; order++)
            {
                if ((flags & (1u << order)) != 0)
                {
                    lookup |= stored << (int)(order switch
                    {
                        0 => QuickMapIndexPosition.One,
                        1 => QuickMapIndexPosition.Two,
                        2 => QuickMapIndexPosition.Three,
                        3 => QuickMapIndexPosition.Four,
                        4 => QuickMapIndexPosition.Five,
                        5 => QuickMapIndexPosition.Six,
                        6 => QuickMapIndexPosition.Seven,
                        7 => QuickMapIndexPosition.Eight,
                        8 => QuickMapIndexPosition.Nine,
                        9 => QuickMapIndexPosition.Ten,
                        10 => QuickMapIndexPosition.Eleven,
                        11 => QuickMapIndexPosition.Twelve,
                        12 => QuickMapIndexPosition.Thirteen,
                        13 => QuickMapIndexPosition.Fourteen,
                        14 => QuickMapIndexPosition.Fifteen,
                        _ => throw new ArgumentOutOfRangeException(nameof(order)),
                    });

                    stored++;
                }
            }
        }

        private static void InsertAtUnchecked(ref T?[] values, ref int stored, int localIndex, T item)
        {
            int newStored = stored + 1;
            if (newStored >= values.Length)
            {
                var array = new T?[newStored];
                for (int i = 0; i < localIndex; i++)
                {
                    array[i] = values[i];
                }

                values = array;
            }

            // Moves other items to the front.
            for (int i = localIndex + 1; i < newStored; i++)
            {
                values[i] = values[i - 1];
            }

            values[localIndex] = item;
            stored = newStored;
        }

        private static void RemoveAtUnchecked(ref T?[] values, ref int stored, int localIndex)
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
        private static int PopCount(uint input)
        {
            input = ((input & 0xAA) >> 1) + (input & 0x55);
            input = (input & 0x33) + ((input >> 2) & 0x33);
            return (int)((input + (input >> 4)) & 0x0F);
        }
    }
}
