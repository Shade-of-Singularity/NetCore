using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace NetCore.Common
{
    /// <summary>
    /// Exception for handling when you use <see cref="CRTPList{T}.GetLookup{TItem}"/>
    /// with TItem matching a base type of the <see cref="CRTPList{T}"/>.
    /// </summary>
    /// <param name="type">Target item type.</param>
    public sealed class InvalidLookupTypeException(Type type)
        : Exception($"Cannot use ({type.Name}) as a type for a Lookup, as it matches the type of a HashList. Use HashList directly instead.");

    /// <summary>
    /// Thrown when you try to register a type, which does not inherit a base type, used for <see cref="CRTPList{TBase}"/>.
    /// </summary>
    /// <param name="itemType">Type of an item.</param>
    /// <param name="baseType">Base type from <see cref="CRTPList{TBase}"/>.</param>
    public sealed class InvalidIndexingTypeException(Type itemType, Type baseType)
        : Exception($"Provided type ({itemType.Name}) does not inherit a base type ({baseType.Name}), and cannot be stored in a HashList.");

    /// <summary>
    /// Thrown when a specific item you were looking for was not found in the <see cref="CRTPList{TBase}"/>.
    /// </summary>
    public sealed class ItemNotFoundException : Exception
    {
        /// <summary>.ctor which will insert given <paramref name="itemType"/> in the error message.</summary>
        /// <param name="itemType">Type of an item.</param>
        public ItemNotFoundException(Type itemType) : base($"Item of a type ({itemType.Name}) is not found in a HashList.") { }
        /// <summary>Default .ctor with default message.</summary>
        public ItemNotFoundException() : base($"Item not found.") { }
    }

    /// <summary>
    /// Thrown when you try to use an insert item method in a list, using item of a type, that was already defined.
    /// </summary>
    /// <param name="itemType">Type of an item that was attempted to be added.</param>
    public sealed class ItemAlreadyExistException(Type itemType)
        : Exception($"Item of a type ({itemType.Name}) is already defined in a HashList. Duplicates are not allowed.");

    /// <summary>
    /// Thrown when you attempt to add/insert/remove an item, providing type which doesn't match an actual type of the item.
    /// </summary>
    /// <param name="providedType">Provided type.</param>
    /// <param name="realType">Type of the real item instance.</param>
    public sealed class ItemTypeMismatchException(Type providedType, Type realType)
        : Exception($"Specified item type ({providedType.Name}) and type of a real item ({realType.Name}) does not match.");

    /// <summary>
    /// Thrown when you (somehow) exceed a maximum capacity of a <see cref="CRTPList{TBase}"/> - <see cref="int.MaxValue"/>.
    /// </summary>
    /// <remarks>
    /// Simply how.
    /// </remarks>
    public sealed class CRTPListTooLargeException()
        : Exception($"Attempted to resize a HashList beyond a current limit of ({CRTPLists.MaxCapacity}).");

    /// <summary>
    /// Stores some trivia about <see cref="CRTPList{TBase}"/>.
    /// </summary>
    public static class CRTPLists
    {
        /// <summary>
        /// Max possible capacity an <see cref="CRTPList{TBase}"/> can have.
        /// </summary>
        public const int MaxCapacity = int.MaxValue;
    }

    /// <summary>
    /// (Not thread-safe!)
    /// CRTP list, for storing unique strong-typed items.
    /// Optimized to be a lot faster than <see cref="Dictionary{TKey, TValue}"/> using CRTP.
    /// </summary>
    /// <typeparam name="TBase">Base type of an item.</typeparam>
    /// Note: Implementation is x86 compatible.
    /// Note: Was turned into a class to support <see cref="Lookup{TFilter}"/> classes.
    /// Note (for maintainers):
    ///  (Not applicable anymore - after rework all switch cases were removed. Later this note will be moved to a knowledge database)
    ///  All <see langword="default"/> <see langword="case"/>s in <see langword="switch"/> statements return an exception.
    ///  
    ///  This was made not for convention, or stuff like that - but for branch prediction.
    ///  If you use <![CDATA["3 or _ => ..."]]> or similar syntax - <see langword="switch"/> gets optimized to a jump-table, 
    ///  but the state "3" is merged with a <see langword="default"/> case, both in Debug and Release modes (as of 2Q, 2026).
    ///  Jump-tables are optimized by checking if value falls within a specific range:
    ///  - If range check succeeds - it will jump to a case (e.g. 0, 1, 2, ...).
    ///  - But if range check fails - it will jump to a default case.
    ///  This might create branch unpredictability with highest switch case.
    ///  Thus - we manually define highest case, and provide *literally anything* as a fallback option.
    ///  This pattern enforces branch prediction better.
    ///  - - -
    ///  TL;DR: If you use default case with *any other value* in a switch statement - it creates unpredictable jump-table (if at all).
    ///   That's why we define default cases separately, and will them with garbage (<see cref="SwitchExpressionException"/>s, in this case). 
    ///  
    /// TODO: Consider providing a thread-safe alternative around v1 release.
    /// TODO: Add IndexOf methods.
    /// TODO: Rework growth strategy by introducing a load factor.
    public partial class CRTPList<TBase> where TBase : class
    {
        internal static class Indexing
        {
            // Note: Consider implementing a custom data structure for weakly-typed indexing as well, instead of a dictionary.
            static readonly Dictionary<RuntimeTypeHandle, int> Indexes = [];
            static int Iterator = 0;
            /// <remarks>
            /// Doesn't check if <paramref name="type"/> inherits <typeparamref name="TBase"/>.
            /// Use <see cref="TryGetOrder(Type, out int)"/> to check if it inherits <typeparamref name="TBase"/>.
            /// </remarks>
            public static int GetOrderUnchecked(Type type)
            {
                lock (Indexes)
                {
                    var handle = type.TypeHandle;
                    if (!Indexes.TryGetValue(type.TypeHandle, out int order))
                    {
                        Indexes[handle] = order = checked(Iterator++);
                    }

                    return order;
                }
            }
            /// <returns>
            /// <c>false</c> if <paramref name="type"/> does not inherit <typeparamref name="TBase"/>.
            /// <c>true</c> if it does, and an <paramref name="order"/> was provided.
            /// </returns>
            public static bool TryGetOrder(Type type, out int order)
            {
                lock (Indexes)
                {
                    var handle = type.TypeHandle;
                    if (!Indexes.TryGetValue(type.TypeHandle, out order))
                    {
                        if (!typeof(TBase).IsAssignableFrom(type))
                        {
                            return false;
                        }

                        Indexes[handle] = order = checked(Iterator++);
                    }

                    return true;
                }
            }
        }

        internal static class ID<TItem> where TItem : TBase
        {
            /// <summary>
            /// Initialization order of this item.
            /// </summary>
            /// <remarks>
            /// Initialization order might be different depending on the app, and as such - order should not be sent over the network.
            /// Assume that this value is never guaranteed to be either equal or different from the one on a remote host.
            /// </remarks>
            public static readonly int Order = Indexing.GetOrderUnchecked(typeof(TItem));
        }

        internal static class ID
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int RegionFrom(int order) => order >> 4;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint FlagFrom(int order) => 1u << (order & 0b1111);
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int GetIndex(uint region, uint flag) => (int)(region >> 16) + PopCountUShortUnbound(region & (flag - 1));

            /// <summary>
            /// Pop counter for ushort values.
            /// </summary>
            internal static int PopCountUShortUnbound(uint v)
            {
                v -= (v >> 1) & 0x5555u;
                v = (v & 0x3333u) + ((v >> 2) & 0x3333u);
                v = (v + (v >> 4)) & 0x0F0Fu;
                v += v >> 8;

                return (int)(v & 0x1F); // max is 16
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                 Constants
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        const uint AddBit = 0x10000;
        const int RemoveAll = -1;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                 Delegates
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private delegate void ItemRemovedHandler(int order);




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                   Events
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private event ItemRemovedHandler? ItemRemoved;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Public Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Amount of items stored in this <see cref="CRTPList{TBase}"/>.
        /// </summary>
        public int Count => stored;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private int stored;
        private uint[] flags;
        private ushort[] indexes;
        private TBase[] items;
        [Obsolete("Implement fully. Alternatively we can use unique resizing, and access it via CRTP.")]
        private float loadFactor;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Constructors
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Default .ctor, specifying 0 initial capacity.
        /// </summary>
        public CRTPList()
        {
            flags = [];
            indexes = [];
            items = [];
        }
        /// <summary>
        /// .ctor for <see cref="CRTPList{TBase}"/>, allowing you to specify an initial capacity.
        /// </summary>
        public CRTPList(int capacity)
        {
            if (capacity < 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            if (capacity == 0)
            {
                flags = [];
                indexes = [];
                items = [];
                return;
            }

            flags = new uint[capacity >> 4];
            indexes = new ushort[capacity];
            items = new TBase[capacity];
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Checks if <see cref="CRTPList{TBase}"/> has <typeparamref name="TItem"/> defined.
        /// <para>Complexity: O(1)</para>
        /// </summary>
        public bool Has<TItem>() where TItem : TBase => HasInternal(ID<TItem>.Order);
        /// <summary>
        /// Checks if a given item type is defined in a list.
        /// </summary>
        /// <remarks>
        /// Less performant than <see cref="TryGet{TItem}(out TItem)"/> (as it doesn't use pre-computed masks),
        /// but instead - it supports weak types.
        /// <para>Complexity: O(1)</para>
        /// </remarks>
        /// <param name="itemType">Type of an target item to check for.</param>
        public bool Has(Type itemType)
        {
            if (Indexing.TryGetOrder(itemType, out int order))
            {
                return HasInternal(order);
            }

            return false;
        }

        bool HasInternal(int order)
        {
            int region = ID.RegionFrom(order);
            if (region >= flags.Length)
            {
                return false;
            }

            return (flags[region] & ID.FlagFrom(order)) != 0u;
        }




        /// <summary>
        /// Attempts to retrieve an <typeparamref name="TItem"/> from this <see cref="CRTPList{TBase}"/>
        /// <para>Complexity: O(1)</para>
        /// </summary>
        /// <typeparam name="TItem">Item, inheriting <typeparamref name="TBase"/> to look for.</typeparam>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was found and was provided.
        /// <c>false</c> otherwise.
        /// </returns>
        public bool TryGet<TItem>([NotNullWhen(true)] out TItem? item) where TItem : TBase => TryGetInternal(ID<TItem>.Order, out item);
        /// <summary>
        /// Attempts to retrieve an <paramref name="item"/> of a given <paramref name="itemType"/> from this <see cref="CRTPList{TBase}"/>
        /// </summary>
        /// <remarks>
        /// Less performant than <see cref="TryGet{TItem}(out TItem)"/> (as it doesn't use pre-computed masks),
        /// but instead - it supports weak types.
        /// <para>Complexity: O(1)</para>
        /// </remarks>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was found and was provided.
        /// <c>false</c> otherwise.
        /// </returns>
        public bool TryGet(Type itemType, [NotNullWhen(true)] out TBase? item)
        {
            if (Indexing.TryGetOrder(itemType, out int order))
            {
                return TryGetInternal(order, out item);
            }

            item = default;
            return false;
        }

        bool TryGetInternal<TItem>(int order, [NotNullWhen(true)] out TItem? item) where TItem : TBase
        {
            int region = ID.RegionFrom(order);
            if (region >= flags.Length)
            {
                item = default;
                return false;
            }

            uint packed = flags[region];
            uint flag = ID.FlagFrom(order);
            if ((packed & flag) == 0u)
            {
                item = default;
                return false;
            }

            item = (TItem)items[indexes[ID.GetIndex(packed, flag)]];
            return true;
        }




        /// <summary>
        /// Retrieves an item of a type <typeparamref name="TItem"/> from this <see cref="CRTPList{TBase}"/>.
        /// </summary>
        /// <remarks>
        /// <para>Throws a <see cref="KeyNotFoundException"/> if <typeparamref name="TItem"/> is not defined in this list.</para>
        /// <para>Complexity: O(1)</para>
        /// </remarks>
        /// <typeparam name="TItem">Item type to look for.</typeparam>
        /// <exception cref="KeyNotFoundException"><typeparamref name="TItem"/> is not defined in this list.</exception>
        public TItem Get<TItem>() where TItem : TBase => (TItem)GetInternal(ID<TItem>.Order);
        /// <summary>
        /// Retrieves an item of a given <paramref name="itemType"/> from this <see cref="CRTPList{TBase}"/>.
        /// </summary>
        /// <remarks>
        /// Less performant than <see cref="TryGet{TItem}(out TItem)"/> (as it doesn't use pre-computed masks),
        /// but instead - it supports weak types.
        /// <para>Throws a <see cref="KeyNotFoundException"/> if item under given <paramref name="itemType"/> is not defined in this list.</para>
        /// <para>Complexity: O(1)</para>
        /// </remarks>
        /// <param name="itemType">Item type to look for.</param>
        /// <exception cref="KeyNotFoundException">Item under a given <paramref name="itemType"/> is not defined in this list.</exception>
        public TBase Get(Type itemType)
        {
            if (Indexing.TryGetOrder(itemType, out int order))
            {
                return GetInternal(order);
            }

            throw new ItemNotFoundException(itemType);
        }

        TBase GetInternal(int order)
        {
            int region = ID.RegionFrom(order);
            if (region >= flags.Length)
            {
                throw new ItemNotFoundException();
            }

            uint packed = flags[region];
            uint flag = ID.FlagFrom(order);
            if ((packed & flag) == 0u)
            {
                throw new ItemNotFoundException();
            }

            return items[indexes[ID.GetIndex(packed, flag)]];
        }

        TBase GetInternalUnchecked(int order)
        {
            return items[indexes[ID.GetIndex(region: flags[ID.RegionFrom(order)], flag: ID.FlagFrom(order))]];
        }




        /// <summary>
        /// Retrieves an item of a type <typeparamref name="TItem"/> from this <see cref="CRTPList{TBase}"/>.
        /// Throws a <see cref="KeyNotFoundException"/> if not defined.
        /// <para>Complexity: O(1)</para>
        /// </summary>
        /// <typeparam name="TItem">Item type to look for.</typeparam>
        /// <exception cref="KeyNotFoundException"><typeparamref name="TItem"/> is not defined in this list.</exception>
        public TItem? SafeGet<TItem>() where TItem : class, TBase => SafeGetInternal(ID<TItem>.Order) as TItem;
        /// <summary>
        /// Retrieves an item of a given <paramref name="itemType"/> from this <see cref="CRTPList{TBase}"/>.
        /// Throws a <see cref="KeyNotFoundException"/> if not defined.
        /// </summary>
        /// <remarks>
        /// Less performant than <see cref="TryGet{TItem}(out TItem)"/> (as it doesn't use pre-computed masks),
        /// but instead - it supports weak types.
        /// <para>Complexity: O(1)</para>
        /// </remarks>
        /// <param name="itemType">Item type to look for.</param>
        /// <exception cref="KeyNotFoundException">Item of a given <paramref name="itemType"/> is not defined in this list.</exception>
        public TBase? SafeGet(Type itemType)
        {
            if (Indexing.TryGetOrder(itemType, out int order))
            {
                return SafeGetInternal(order);
            }

            return default;
        }

        TBase? SafeGetInternal(int order)
        {
            int region = ID.RegionFrom(order);
            if (region >= flags.Length)
            {
                return default;
            }

            uint packed = flags[region];
            uint flag = ID.FlagFrom(order);
            if ((packed & flag) == 0u)
            {
                return default;
            }

            return items[indexes[ID.GetIndex(packed, flag)]];
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                Modification
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Attempts to add given <paramref name="item"/> to the list.
        /// <para>Complexity: O(n)</para>
        /// </summary>
        /// <typeparam name="TItem">Item type to register.</typeparam>
        /// <param name="item">Item to add in the end of the list.</param>
        /// <returns>
        /// <c>true</c> if added successfully.
        /// <c>false</c> if item already exist.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/></exception>
        /// <exception cref="ItemTypeMismatchException"><typeparamref name="TItem"/> does not match a real type of the <paramref name="item"/>.</exception>
        public bool Add<TItem>(TItem item) where TItem : TBase
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (typeof(TItem) != item.GetType())
                throw new ItemTypeMismatchException(typeof(TItem), item.GetType());

            return AddInternal(ID<TItem>.Order, item);
        }

        /// <summary>
        /// Attempts to add given <paramref name="item"/> to the list.
        /// <para>Complexity: O(n)</para>
        /// </summary>
        /// <param name="item">Item to add in the end of the list.</param>
        /// <returns>
        /// <c>true</c> if added successfully.
        /// <c>false</c> if item already exist.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/></exception>
        /// <exception cref="ItemTypeMismatchException"><typeparamref name="TBase"/> does not match a real type of the <paramref name="item"/>.</exception>
        /// <seealso cref="CRTPList{TBase}.Add{TItem}(TItem)"/>
        public bool Add(TBase item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (typeof(TBase) != item.GetType())
                throw new ItemTypeMismatchException(typeof(TBase), item.GetType());

            if (CRTPList<TBase>.Indexing.TryGetOrder(item.GetType(), out int order))
            {
                return AddInternal(order, item);
            }

            return false;
        }

        bool AddInternal(int order, TBase item)
        {
            EnsureMappingCapacity(order);
            EnsureItemsCapacity(stored);

            // Inserts item at the end.
            int region = ID.RegionFrom(order);
            ref uint packed = ref flags[region];
            uint flag = ID.FlagFrom(order);
            if ((packed & flag) != 0)
            {
                return false;
            }

            int index = ID.GetIndex(packed, flag);
            packed |= flag;

            Buffer.BlockCopy(indexes, index, indexes, index + 1, stored - index);

            // Inserts item at the end.
            indexes[index] = (ushort)stored;
            items[stored] = item;
            stored++;
            return true;
        }

        // TODO: Add internal InsertAt(index) + IndexOf.




        /// <summary>
        /// Removes item under a type <typeparamref name="TItem"/> from the list.
        /// <para>Complexity: O(n)</para>
        /// </summary>
        /// <typeparam name="TItem">Item type to remove from the list.</typeparam>
        /// <returns>
        /// <see langword="true"/> if item was successfully removed.
        /// <see langword="false"/> if item was not defined in this list in a first place.
        /// </returns>
        public bool Remove<TItem>() where TItem : TBase => RemoveInternal<TItem>(ID<TItem>.Order, default, out var _);
        /// <summary>
        /// Removes item under a type <typeparamref name="TItem"/> from the list, and returns it as <paramref name="item"/>.
        /// <para>Complexity: O(n)</para>
        /// </summary>
        /// <typeparam name="TItem">Item type to remove from the list.</typeparam>
        /// <returns>
        /// <see langword="true"/> if item was successfully removed.
        /// <see langword="false"/> if item was not defined in this list in a first place.
        /// </returns>
        public bool Remove<TItem>([NotNullWhen(true)] out TItem? item) where TItem : TBase => RemoveInternal(ID<TItem>.Order, default, out item);
        /// <summary>
        /// Removes specified <paramref name="item"/> from the list.
        /// <para>Complexity: O(n)</para>
        /// </summary>
        /// <typeparam name="TItem">Item type to remove from the list.</typeparam>
        /// <returns>
        /// <see langword="true"/> if item was successfully removed.
        /// <see langword="false"/> if item was not defined in this list in a first place.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/></exception>
        /// <exception cref="ItemTypeMismatchException"><typeparamref name="TItem"/> does not match a real type of the <paramref name="item"/>.</exception>
        public bool Remove<TItem>(TItem item) where TItem : TBase
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (typeof(TItem) != item.GetType())
                throw new ItemTypeMismatchException(typeof(TItem), item.GetType());

            return RemoveInternal(ID<TItem>.Order, item, out var _);
        }

        /// <summary>
        /// Removes item under a type <typeparamref name="TBase"/> from the list, without throwing exceptions.
        /// <para>Complexity: O(n)</para>
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="item"/> is <see langword="null"/></exception>
        /// <exception cref="InvalidIndexingTypeException"><paramref name="item"/> does not inherit <typeparamref name="TBase"/>.</exception>
        /// <exception cref="ItemAlreadyExistException">An item with the same type is already listed.</exception>
        /// <exception cref="ItemTypeMismatchException"><typeparamref name="TBase"/> does not match a real type of the <paramref name="item"/>.</exception>
        /// <returns>
        /// <see langword="true"/> if item was successfully removed.
        /// <see langword="false"/> if item was not defined in this list in a first place.
        /// </returns>
        public bool Remove(TBase item)
        {
            if (item is null)
                throw new ArgumentNullException(nameof(item));

            if (typeof(TBase) != item.GetType())
                throw new ItemTypeMismatchException(typeof(TBase), item.GetType());

            if (!CRTPList<TBase>.Indexing.TryGetOrder(item.GetType(), out int order))
                throw new InvalidIndexingTypeException(item.GetType(), typeof(TBase));

            return RemoveInternal(order, item, out var _);
        }

        bool RemoveInternal<TItem>(int order, TItem? criteria, [NotNullWhen(true)] out TItem? item) where TItem : TBase
        {
            int region = ID.RegionFrom(order);
            if (region >= flags.Length)
            {
                item = default;
                return false;
            }

            ref uint packed = ref flags[region];
            uint flag = ID.FlagFrom(order);
            if ((region & flag) == 0)
            {
                item = default;
                return false;
            }

            packed &= ~flag;
            for (int i = region + 1; i < stored; i++)
            {
                //Reduces all further indexes by 1.
                flags[i] -= AddBit;
            }

            int index = ID.GetIndex(packed, flag);
            int itemIndex = indexes[index];

            var temp = items[itemIndex];
            if (criteria is not null && !ReferenceEquals(criteria, temp))
            {
                item = default;
                return false;
            }

            Buffer.BlockCopy(items, itemIndex + 1, items, itemIndex, stored - itemIndex);
            items[stored] = default!;
            Buffer.BlockCopy(indexes, index + 1, indexes, index, stored - index);

            ItemRemoved?.Invoke(order);
            item = (TItem)temp;
            return true;
        }




        /// <summary>
        /// Clears the entire list.
        /// </summary>
        public void Clear()
        {
            Array.Fill(flags, default);
            Array.Fill(indexes, default);
            Array.Fill(items, default);
            stored = 0;
            ItemRemoved?.Invoke(RemoveAll);
        }




        /// <summary>
        /// Copies items from the internal array into the output array.
        /// </summary>
        /// <param name="array">Buffer for storing items.</param>
        public void CopyTo(TBase[] array) => Array.Copy(items, array, stored);
        /// <summary>
        /// Copies items from the internal array into the output array.
        /// </summary>
        /// <param name="array">Buffer for storing items.</param>
        /// <param name="length">Amount of items copied to the buffer array.</param>
        public void CopyTo(TBase[] array, out int length)
        {
            length = stored;
            Array.Copy(items, array, length);
        }
        /// <summary>
        /// Retrieves ref struct-based <see cref="Enumerator"/> for iterating over all items this <see cref="CRTPList{TBase}"/> holds.
        /// </summary>
        /// <remarks>
        /// Does not allocate a buffer for iteration, thus should only be used within a lock or a single thread.
        /// </remarks>
        /// <returns><see cref="Enumerator"/> to iterate over.</returns>
        public Enumerator GetEnumerator() => new(items, stored);
        /// <summary>
        /// Enumerates over <see cref="CRTPList{TBase}"/>.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="stored"></param>
        public ref struct Enumerator(TBase[] items, int stored)
        {
            private readonly TBase[] items = items;
            private readonly int stored = stored;
            int iterator = -1;
            /// <summary>
            /// Item under current iterator. Throws if used before or after using <see cref="MoveNext"/>.
            /// </summary>
            /// <exception cref="ArgumentOutOfRangeException">When used on uninitialized or already used-up <see cref="Enumerator"/>.</exception>
            public readonly TBase Current => items[iterator];
            /// <summary>
            /// Moves enumerator forward by an item.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> if there are still more items to iterate over.
            /// <see langword="false"/> when there are no more items. Using <see cref="Current"/> at this point will throw.
            /// </returns>
            public bool MoveNext() => ++iterator < stored;
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private void EnsureMappingCapacity(int order)
        {
            int flagsSize = (order + 16) >> 4;
            if (flagsSize > flags.Length)
            {
                Array.Resize(ref flags, flagsSize);
            }
        }

        private void EnsureItemsCapacity(int amount)
        {
            if (amount > indexes.Length)
            {
                Array.Resize(ref indexes, amount);
                Array.Resize(ref items, amount);
            }
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Static Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Creates a new lookup data structure, attached to this <see cref="CRTPList{TBase}"/> parent.
        /// </summary>
        /// <exception cref="InvalidLookupTypeException">
        /// <typeparamref name="TFilter"/> equals to <typeparamref name="TBase"/>.
        /// This is not allowed, because <see cref="Lookup{TFilter}"/> will describe all values of <see cref="CRTPList{TBase}"/>
        /// which doesn't make sense - it's better to expose the original <see cref="CRTPList{TBase}"/> instead.
        /// </exception>
        public Lookup<TFilter> GetLookup<TFilter>() where TFilter : TBase
        {
            if (typeof(TFilter) == typeof(TBase))
            {
                throw new InvalidLookupTypeException(typeof(TFilter));
            }

            return new Lookup<TFilter>(this);
        }
    }
}
