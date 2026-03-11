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
    /// (2) Has a technical limit of items. See also: <see cref="QuickIndex.Limit"/>
    /// </remarks>
    /// <typeparam name="T">Item type to store.</typeparam>
    /// Note: By avoiding using <see cref="QuickIndex"/> usage and by using only <see cref="QuickID{TItem, TCategory}.Order"/> instead
    ///  we can increase the amount of storable items to the amount of bits in a <see cref="flags"/> variable.
    ///  If we use ushort - the limit will be 16.
    ///  If we use uint - the limit will be 32.
    ///  If we use ulong - the limit will be 64.
    /// But it will be come a bit slower, because we will need to calculate (once) the <see cref="NumberOfSetBits(int)"/> on each Get operation.
    /// We can introduce it if benchmarks of it will be successful.
    public struct QuickMap<T> where T : class
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private ulong lookup; // Encodes local index of all items in one lookup table.
        private ushort flags; // Stores (true/false) states about whether a specific item is present in the array or not.
        private T?[] values; // Stores items themselves. Resized if you add new values. Limited to 13 in size.
        private int stored; // Amount of items stored in the array. Array capacity might differ from this value.




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
        /// Constructor allowing you to specify how much space (from 0 to <see cref="QuickIndex.Limit"/>) to pre-allocate in the internal array.
        /// </summary>
        /// <param name="capacity">Capacity for the internal array.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> was larger than <see cref="QuickIndex.Limit"/></exception>
        public QuickMap(int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException($"{nameof(capacity)} in {nameof(QuickMap<T>)} constructor cannot be less than 0. Provided: {capacity}");
            if (capacity > QuickIndex.Limit)
            {
                throw new ArgumentOutOfRangeException($"Cannot create QuickMap with a {capacity}, larger than {QuickIndex.Limit} (technical limit) (seealso: {nameof(QuickIndex)}).{nameof(QuickIndex.Limit)}");
            }

            values = new T[capacity];
        }

        /// <returns>
        /// <c>true</c> if item was added.
        /// <c>false</c> if item with the same <see cref="Type"/>/<see cref="QuickIndex"/> is already in the map.
        /// </returns>
        public bool Add<TItem>(TItem item) where TItem : class, T
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            // QuickID will throw if item cannot be stored. Thus some checks can be removed.
            int order = QuickID<TItem, T>.Order;
            if ((flags & (1u << order)) != 0u)
            {
                // Item is was already stored.
                return false;
            }

            InsertAtUnchecked(ref values, ref stored, LocalIndexOf(order, flags), item);
            MarkStored(ref flags, order);
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
            int order = QuickID<TItem, T>.Order;
            int localIndex = LocalIndexOf(order, flags);
            uint flag = 1u << order;
            if ((flags & flag) != 0u)
            {
                // Item under a specific order already exist.
                // Replacing it won't rebuild the lookup table and won't require an array resize.
                values[localIndex] = item;
                return;
            }

            InsertAtUnchecked(ref values, ref stored, localIndex, item);
            MarkStored(ref flags, order);
            UpdateLookup(flags, out lookup);
            return;
        }

        /// <summary>
        /// Attempts to retrieve <typeparamref name="TItem"/> instance from the map.
        /// </summary>
        /// <returns><c>true</c> if found. <c>false</c> otherwise.</returns>
        public readonly bool TryGet<TItem>([NotNullWhen(true)] out TItem? item) where TItem : class, T
        {
            item = Get<TItem>();
            return item is not null;
        }

        /// <summary>
        /// Retrieves <typeparamref name="TItem"/> instance from the map.
        /// </summary>
        /// <returns>Instance of <typeparamref name="TItem"/> from the map, or <c>null</c> if not found.</returns>
        public readonly TItem? Get<TItem>() where TItem : class, T
        {
            // QuickID will throw if item cannot be stored. Thus some checks can be removed.
            if ((flags & QuickID<TItem, T>.Order) == 0)
            {
                return default; // There is no such item in a collection.
            }

            QuickIndex index = QuickID<TItem, T>.Index;
            return values[(int)((lookup & (ulong)index.Mask) >> (int)index.position)] as TItem;
        }

        /// <summary>
        /// Checks if item of a specific <typeparamref name="TItem"/> type exist exist in the internal map.
        /// </summary>
        /// <returns><c>true</c> if item exist. <c>false</c> otherwise.</returns>
        public readonly bool Has<TItem>() where TItem : class, T => (flags & QuickID<TItem, T>.Order) != 0;

        /// <summary>
        /// Removes specific <paramref name="item"/> of a type <typeparamref name="TItem"/> from the map.
        /// </summary>
        /// <returns><c>true</c> if item was removed. <c>false</c> if item wasn't present in the map to begin with.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <c>null</c>.</exception>
        public bool Remove<TItem>(TItem item) where TItem : class, T
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            // QuickID will throw if item cannot be stored. Thus some checks can be removed.
            int order = QuickID<TItem, T>.Order;
            if ((flags & (1u << order)) == 0u)
            {
                // Item is was already removed.
                return false;
            }

            RemoveAtUnchecked(ref values, ref stored, LocalIndexOf(order, flags));
            MarkEmpty(ref flags, order);
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
            int order = QuickID<TItem, T>.Order;
            if ((flags & (1u << order)) == 0u)
            {
                // Item is was already removed.
                return false;
            }

            RemoveAtUnchecked(ref values, ref stored, LocalIndexOf(order, flags));
            MarkEmpty(ref flags, order);
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
            int order = QuickID<TItem, T>.Order;
            if ((flags & (1u << order)) == 0u)
            {
                // Item is was already removed.
                removed = default;
                return false;
            }

            int localIndex = LocalIndexOf(order, flags);
            removed = (TItem)values[localIndex]!;
            RemoveAtUnchecked(ref values, ref stored, localIndex);
            MarkEmpty(ref flags, order);
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
        /// Sets bit in <paramref name="flags"/> under given <paramref name="order"/> to '1'.
        /// </summary>
        private static void MarkStored(ref ushort flags, int order) => flags |= (ushort)(1u << order);

        /// <summary>
        /// Sets bit in <paramref name="flags"/> under given <paramref name="order"/> to '0'.
        /// </summary>
        private static void MarkEmpty(ref ushort flags, int order) => flags &= (ushort)~(1u << order);

        /// <summary>
        /// Updates a <paramref name="lookup"/> table based on input <paramref name="flags"/>.
        /// </summary>
        private static void UpdateLookup(ushort flags, out ulong lookup)
        {
            ulong stored = 0;
            lookup = 0uL;

            for (int i = 0; i < QuickIndex.Limit; i++)
            {
                if ((flags & (1u << i)) != 0)
                {
                    lookup |= stored << (int)(i switch
                    {
                        0 => QuickIndexPosition.One,
                        1 => QuickIndexPosition.Two,
                        2 => QuickIndexPosition.Three,
                        3 => QuickIndexPosition.Four,
                        4 => QuickIndexPosition.Five,
                        5 => QuickIndexPosition.Six,
                        6 => QuickIndexPosition.Seven,
                        7 => QuickIndexPosition.Eight,
                        8 => QuickIndexPosition.Nine,
                        9 => QuickIndexPosition.Ten,
                        10 => QuickIndexPosition.Eleven,
                        11 => QuickIndexPosition.Twelve,
                        12 => QuickIndexPosition.Thirteen,
                        13 => QuickIndexPosition.Fourteen,
                        _ => default,
                    });

                    stored++;
                }
            }
        }

        /// <summary>
        /// Retrieves index of an item in a local array, where item's preferred position is described with <paramref name="order"/>.
        /// </summary>
        private static int LocalIndexOf(int order, ushort flags) => NumberOfSetBits(flags & (ushort)((1 << order) - 1));
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

        private static int NumberOfSetBits(int i)
        {
            i -= (i >> 1) & 0x55555555;
            i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
            return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
        }
    }
}
