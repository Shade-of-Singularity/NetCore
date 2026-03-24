using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetCore.Common
{
    public sealed partial class CRTPList<TBase>
    {
        /// <summary>
        /// (Not thread-safe!)
        /// Lookup class for managing items of a specific type in a base <see cref="CRTPList{TBase}"/>.
        /// </summary>
        /// <typeparam name="TFilter">Type for which to filter.</typeparam>
        /// Note: RemoveAt and IndexOf was not implemented, because it is hard to decide whether index
        ///  from <see cref="CRTPList{TBase}"/> should be used, or custom indexing system should be provided for <see cref="Lookup{TFilter}"/>.
        ///  (Probably the latter though - simply map indexes back to flags, and then to <see cref="CRTPList{TBase}"/> positions)
        ///  (Even approach like this should still be faster than <see cref="Dictionary{TKey, TValue}"/>).
        /// Note: Same applies to Insert methods. It's likely that we should use item swapping for insertion operations.
        /// Note: Was turned in a class for ease of use.
        public sealed class Lookup<TFilter> : IDisposable where TFilter : TBase
        {
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
            /// .
            /// .                                              Public Properties
            /// .
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
            /// <summary>
            /// Amount of items for a given <typeparamref name="TFilter"/> type this <see cref="Lookup{TFilter}"/> filters.
            /// </summary>
            public int Count => stored;




            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
            /// .
            /// .                                               Private Fields
            /// .
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
            readonly CRTPList<TBase> list;
            /// <summary>
            /// Stores 32 flags per entry, describing presence of an item in a parent <see cref="CRTPList{TBase}"/>.
            /// </summary>
            uint[] filters;
            /// <summary>
            /// Amount of items "stored" (a.k.a. filtered) using this item list.
            /// </summary>
            int stored;



            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
            /// .
            /// .                                                Constructors
            /// .
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
            /// <summary>
            /// Invalid .ctor for the <see cref="Lookup{TFilter}"/>. use <see cref="Lookup{TFilter}(CRTPList{TBase})"/> instead.
            /// </summary>
            public Lookup() => throw new NotSupportedException("Lookups should only be created by providing a reference to a list.");
            /// <summary>
            /// Valid .ctor for a <see cref="Lookup{TFilter}"/> list.
            /// </summary>
            public Lookup(CRTPList<TBase> list)
            {
                this.list = list;
                filters = [];
                stored = 0;
                list.ItemChanged += OnItemChanged;
            }
            /// <summary>
            /// Destructor which unsubscribed from the internal callbacks on a parent list.
            /// </summary>
            public void Dispose()
            {
                list.ItemChanged -= OnItemChanged;
                filters = [];
            }




            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
            /// .
            /// .                                               Update Handling
            /// .
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
            private void OnItemChanged(TBase? item, int order, bool added)
            {
                if (item is null)
                {
                    Array.Fill(filters, 0u);
                    stored = 0;
                    return;
                }

                int region = order >> 5;
                if (region >= filters.Length)
                {
                    Array.Resize(ref filters, region + 1);
                }

                uint flag = 1u << (order & 0b11111);
                ref uint filter = ref filters[region];
                if (added)
                {
                    if ((filter & flag) == 0)
                    {
                        filter |= flag;
                        stored++;
                    }
                }
                else
                {
                    if ((filter & flag) != 0)
                    {
                        filter &= ~flag;
                        stored--;
                    }
                }
            }





            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
            /// .
            /// .                                               Public Methods
            /// .
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
            /// <inheritdoc cref="CRTPList{TBase}.Has{TItem}()"/>
            public bool Has<TItem>() where TItem : TFilter
            {
                if (ID<TItem>.FlagRegion >= filters.Length)
                {
                    return false; // Handles out-of-bounds.
                }

                return (filters[ID<TItem>.FlagRegion] & ID<TItem>.Flag) != 0;
            }

            /// <inheritdoc cref="CRTPList{TBase}.TryGet{TItem}(out TItem)"/>
            public bool TryGet<TItem>([NotNullWhen(true)] out TItem? item) where TItem : TFilter
            {
                if (ID<TItem>.FlagRegion >= filters.Length)
                {
                    item = default;
                    return false; // Handles out-of-bounds.
                }

                if ((filters[ID<TItem>.FlagRegion] & ID<TItem>.Flag) == 0)
                {
                    item = default;
                    return false; // Item does not exist.
                }

                // Uses unsafe internal methods directly, avoiding double bounds checks.
                // Assumes that local mapping is always valid.
                switch (list.mode)
                {
                    case PackingMode.Size0: item = default; return false;
                    case PackingMode.SizeByte: return list.TryGetInternalUnsafeByte(ID<TItem>.Order, out item, list.flags[ID<TItem>.Order >> 2]);
                    case PackingMode.SizeUShort: return list.TryGetInternalUnsafeUShort(ID<TItem>.Order, out item, list.flags[ID<TItem>.Order >> 1]);
                    case PackingMode.SizeUInt: return list.TryGetInternalUnsafeUInt(out item, list.flags[ID<TItem>.Order]);
                    default: throw new SwitchExpressionException(list.mode);
                }
            }

            /// <inheritdoc cref="CRTPList{TBase}.Get{TItem}()"/>
            public TItem Get<TItem>() where TItem : TFilter
            {
                if (ID<TItem>.FlagRegion >= this.filters.Length)
                {
                    throw new ItemNotFoundException(typeof(TItem));
                }

                if ((this.filters[ID<TItem>.FlagRegion] & ID<TItem>.Flag) == 0)
                {
                    throw new ItemNotFoundException(typeof(TItem));
                }

                // Re-implements internal unsafe methods directly, avoiding double bounds checks.
                // Assumes that local mapping is always valid.
                uint flags;
                switch (list.mode)
                {
                    case PackingMode.Size0: throw new ItemNotFoundException(typeof(TItem));
                    case PackingMode.SizeByte:
                        flags = list.flags[ID<TItem>.Order >> 2];
                        return (TItem)list.items[(ID<TItem>.Order & 0b11) switch
                        {
                            0 => (flags & PackingByte.ItemValue1) >> PackingByte.ItemShift1,
                            1 => (flags & PackingByte.ItemValue2) >> PackingByte.ItemShift2,
                            2 => (flags & PackingByte.ItemValue3) >> PackingByte.ItemShift3,
                            3 => (flags & PackingByte.ItemValue4) >> PackingByte.ItemShift4,
                            _ => throw new SwitchExpressionException(ID<TItem>.Order & 0b11),
                        }]!;

                    case PackingMode.SizeUShort:
                        flags = list.flags[ID<TItem>.Order >> 1];
                        return (TItem)list.items[(ID<TItem>.Order & 0b1) switch
                        {
                            0 => (flags & PackingUShort.ItemValue1) >> PackingUShort.ItemShift1,
                            1 => (flags & PackingUShort.ItemValue2) >> PackingUShort.ItemShift2,
                            _ => throw new SwitchExpressionException(ID<TItem>.Order & 0b1),
                        }]!;

                    case PackingMode.SizeUInt:
                        flags = list.flags[ID<TItem>.Order];
                        return (TItem)list.items[(flags & PackingUInt.ItemValue) >> PackingUInt.ItemShift]!;

                    default: throw new SwitchExpressionException(list.mode);
                }
            }

            /// <inheritdoc cref="CRTPList{TBase}.SafeGet{TItem}()"/>
            public TItem? SafeGet<TItem>() where TItem : class, TFilter
            {
                if (ID<TItem>.FlagRegion >= this.filters.Length)
                {
                    return default;
                }

                if ((this.filters[ID<TItem>.FlagRegion] & ID<TItem>.Flag) == 0)
                {
                    return default;
                }

                // Re-implements internal unsafe methods directly, avoiding double bounds checks.
                // Assumes that local mapping is always valid.
                uint flags;
                switch (list.mode)
                {
                    case PackingMode.Size0: return default;
                    case PackingMode.SizeByte:
                        flags = list.flags[ID<TItem>.Order >> 2];
                        return list.items[(ID<TItem>.Order & 0b11) switch
                        {
                            0 => (flags & PackingByte.ItemValue1) >> PackingByte.ItemShift1,
                            1 => (flags & PackingByte.ItemValue2) >> PackingByte.ItemShift2,
                            2 => (flags & PackingByte.ItemValue3) >> PackingByte.ItemShift3,
                            3 => (flags & PackingByte.ItemValue4) >> PackingByte.ItemShift4,
                            _ => throw new SwitchExpressionException(ID<TItem>.Order & 0b11),
                        }] as TItem;

                    case PackingMode.SizeUShort:
                        flags = list.flags[ID<TItem>.Order >> 1];
                        return list.items[(ID<TItem>.Order & 0b1) switch
                        {
                            0 => (flags & PackingUShort.ItemValue1) >> PackingUShort.ItemShift1,
                            1 => (flags & PackingUShort.ItemValue2) >> PackingUShort.ItemShift2,
                            _ => throw new SwitchExpressionException(ID<TItem>.Order & 0b1),
                        }] as TItem;

                    case PackingMode.SizeUInt:
                        flags = list.flags[ID<TItem>.Order];
                        return list.items[(flags & PackingUInt.ItemValue) >> PackingUInt.ItemShift] as TItem;

                    default: throw new SwitchExpressionException(list.mode);
                }
            }

            /// <inheritdoc cref="CRTPList{TBase}.Add{TItem}(TItem)"/>
            public bool Add<TItem>(TItem item) where TItem : TFilter
            {
                if (item is null)
                    throw new ArgumentNullException(nameof(item));

                if (typeof(TItem) != item.GetType())
                    throw new ItemTypeMismatchException(typeof(TItem), item.GetType());

                if (ID<TItem>.FlagRegion < filters.Length)
                {
                    if ((filters[ID<TItem>.FlagRegion] & ID<TItem>.Flag) != 0)
                        return false; // Item already exist.
                }

                return AddInternalUnchecked(list, ID<TItem>.Order, item);
            }

            /// <inheritdoc cref="CRTPList{TBase}.Add{TItem}(TItem)"/>
            public bool SafeAdd<TItem>(TItem? item) where TItem : TFilter
            {
                if (item is null)
                    return false;

                if (typeof(TItem) != item.GetType())
                    return false;


                if (ID<TItem>.FlagRegion < filters.Length)
                {
                    if ((filters[ID<TItem>.FlagRegion] & ID<TItem>.Flag) != 0)
                        return false; // Item already exist.
                }

                return AddInternalUnchecked(list, ID<TItem>.Order, item);
            }

            /// <seealso cref="CRTPList{TBase}.AddInternal(int, TBase)"/>
            static bool AddInternalUnchecked(CRTPList<TBase> list, int order, TFilter item)
            {
                // Re-implements internal methods without bounds checks.
                uint index;
                switch (list.mode)
                {
                    case PackingMode.Size0:
                        list.mode = PackingMode.SizeByte;
                        list.flags = new uint[(order >> 2) + 1];
                        list.flags[order] = PackingByte.ItemFlag1 | 0;
                        list.items = [item];
                        list.stored = 1;
                        return true;

                    case PackingMode.SizeByte:
                        if ((order >> 2) >= list.flags.Length)
                        {
                            Array.Resize(ref list.flags, )
                        }

                        index = (uint)list.stored++;
                        if (index >= list.items.Length)
                        {
                            Array.Resize(ref list.flags, (int)(index + 1));
                        }

                        ref uint flags1 = ref list.flags[order >> 2];
                        list.items[index] = item;
                        flags1 |= (order & 0b11) switch
                        {
                            0 => PackingByte.ItemFlag1 | (index << PackingByte.ItemShift1),
                            1 => PackingByte.ItemFlag2 | (index << PackingByte.ItemShift2),
                            2 => PackingByte.ItemFlag3 | (index << PackingByte.ItemShift3),
                            3 => PackingByte.ItemFlag4 | (index << PackingByte.ItemShift4),
                            _ => throw new SwitchExpressionException(order & 0b11),
                        };
                        return true;

                    case PackingMode.SizeUShort:
                        ref uint flags2 = ref list.flags[order >> 1];
                        index = (uint)list.stored++; // TODO: Resize array to account for "stored + 1"
                        list.items[index] = item;
                        flags2 |= (order & 0b1) switch
                        {
                            0 => PackingUShort.ItemFlag1 | (index << PackingUShort.ItemShift1),
                            1 => PackingUShort.ItemFlag2 | (index << PackingUShort.ItemShift2),
                            _ => throw new SwitchExpressionException(order & 0b11),
                        };
                        return true;

                    case PackingMode.SizeUInt:
                        ref uint flags3 = ref list.flags[order];
                        index = (uint)list.stored++; // TODO: Resize array to account for "stored + 1"
                        list.items[index] = item;
                        flags3 |= PackingUInt.ItemFlag | (index << PackingUInt.ItemShift);
                        return true;

                    default: throw new SwitchExpressionException(list.mode);
                }
            }

            /// <inheritdoc cref="CRTPList{TBase}.Remove{TItem}()"/>
            public bool Remove<TItem>() where TItem : TFilter
            {
                if (ID<TItem>.FlagRegion >= filters.Length)
                    return false; // Handles out-of-bounds.

                if ((filters[ID<TItem>.FlagRegion] & ID<TItem>.Flag) == 0)
                    return false; // Item does not exist.

                return RemoveInternalUnchecked<TItem>(list, ID<TItem>.Order, default, out var _);
            }

            /// <inheritdoc cref="CRTPList{TBase}.Remove{TItem}(out TItem)"/>
            public bool Remove<TItem>([NotNullWhen(true)] out TItem? item) where TItem : TFilter
            {
                if (ID<TItem>.FlagRegion >= filters.Length)
                {
                    item = default;
                    return false; // Handles out-of-bounds.
                }

                if ((filters[ID<TItem>.FlagRegion] & ID<TItem>.Flag) == 0)
                {
                    item = default;
                    return false; // Item does not exist.
                }

                return RemoveInternalUnchecked(list, ID<TItem>.Order, default, out item);
            }

            /// <inheritdoc cref="CRTPList{TBase}.Remove{TItem}(TItem)"/>
            public bool Remove<TItem>(TItem item) where TItem : TFilter
            {
                if (item is null)
                    throw new ArgumentNullException(nameof(item));

                if (typeof(TItem) != item.GetType())
                    throw new ItemTypeMismatchException(typeof(TItem), item.GetType());

                if (ID<TItem>.FlagRegion >= filters.Length)
                    return false; // Handles out-of-bounds.

                if ((filters[ID<TItem>.FlagRegion] & ID<TItem>.Flag) == 0)
                    return false; // Item does not exist.

                return RemoveInternalUnchecked(list, ID<TItem>.Order, item, out var _);
            }

            /// <inheritdoc cref="CRTPList{TBase}.SafeRemove{TItem}(TItem)"/>
            public bool SafeRemove<TItem>(TItem? item) where TItem : TFilter
            {
                if (item is null || typeof(TItem) != item.GetType())
                    return false;

                if (ID<TItem>.FlagRegion >= filters.Length)
                    return false; // Handles out-of-bounds.

                if ((filters[ID<TItem>.FlagRegion] & ID<TItem>.Flag) == 0)
                    return false; // Item does not exist.

                return RemoveInternalUnchecked(list, ID<TItem>.Order, item, out var _);
            }

            static bool RemoveInternalUnchecked<TItem>(CRTPList<TBase> list, int order, TItem? criteria, [NotNullWhen(true)] out TItem? item)
                where TItem : TFilter
            {
                // Re-implements internal method without bounds checks.
                int index;
                uint flags;
                switch (list.mode)
                {
                    case PackingMode.Size0: item = default; return false;
                    case PackingMode.SizeByte:
                        flags = list.flags[order >> 2];
                        index = (int)((order & 0b11) switch
                        {
                            0 => (flags & PackingByte.ItemValue1) >> PackingByte.ItemShift1,
                            1 => (flags & PackingByte.ItemValue2) >> PackingByte.ItemShift2,
                            2 => (flags & PackingByte.ItemValue3) >> PackingByte.ItemShift3,
                            3 => (flags & PackingByte.ItemValue4) >> PackingByte.ItemShift4,
                            _ => throw new SwitchExpressionException(order & 0b11),
                        });

                        if (list.items[index] is TItem result1 && (criteria is null || EqualityComparer<TItem>.Default.Equals(result1, criteria)))
                        {
                            item = result1;
                            list.RemoveAtInternalUnchecked(index, out var _);
                            return true;
                        }

                        item = default;
                        return false;

                    case PackingMode.SizeUShort:
                        flags = list.flags[order >> 1];
                        index = (int)((order & 0b1) switch
                        {
                            0 => (flags & PackingUShort.ItemValue1) >> PackingUShort.ItemShift1,
                            1 => (flags & PackingUShort.ItemValue2) >> PackingUShort.ItemShift2,
                            _ => throw new SwitchExpressionException(order & 0b1),
                        });

                        if (list.items[index] is TItem result2 && (criteria is null || EqualityComparer<TItem>.Default.Equals(result2, criteria)))
                        {
                            item = result2;
                            list.RemoveAtInternalUnchecked(index, out var _);
                            return true;
                        }

                        item = default;
                        return false;

                    case PackingMode.SizeUInt:
                        flags = list.flags[order];
                        index = (int)((flags & PackingUInt.ItemValue) >> PackingUInt.ItemShift);
                        if (list.items[index] is TItem result3 && (criteria is null || EqualityComparer<TItem>.Default.Equals(result3, criteria)))
                        {
                            item = result3;
                            list.RemoveAtInternalUnchecked(index, out var _);
                            return true;
                        }

                        item = default;
                        return false;

                    default: throw new SwitchExpressionException(list.mode);
                }
            }

            // TODO: Add the rest of the methods, including an enumerator.
            /// <summary>
            /// Retrieves special struct-based enumerator for iterating over items, filtered with <typeparamref name="TFilter"/> type.
            /// </summary>
            /// <returns>Special struct-based enumerator for very fast type-safe iterations.</returns>
            public Enumerator GetEnumerator()
            {
                PackingMode mode = list.mode;
                return new(list.items, mode,
                    bytes: mode == PackingMode.SizeByte ? MemoryMarshal.AsBytes(list.flags.AsSpan()) : default,
                    ushorts: mode == PackingMode.SizeUShort ? MemoryMarshal.Cast<uint, ushort>(list.flags.AsSpan()) : default,
                    uints: mode == PackingMode.SizeUInt ? list.flags.AsSpan() : default,
                    total: list.stored, filters, stored);
            }

            /// <summary>
            /// Struct-based enumerator for fast iterations over filtered items.
            /// </summary>
            public ref struct Enumerator
            {
                // CRTP List data:
                readonly TBase[] items;
                readonly PackingMode mode; // TODO: Replace with 4 enumerators instead, if possible.
                readonly Span<byte> bytes;
                readonly Span<ushort> ushorts;
                readonly Span<uint> uints;
                // Lookup data:
                readonly uint[] flags;
                readonly int stored;
                int iterator = -1;
                int index = -1;
                int passed = 0;
                /// <param name="items">Reference to the internal array of items from <see cref="CRTPList{TBase}"/>.</param>
                /// <param name="mode">Current packing mode from a parent <see cref="CRTPList{TBase}"/>.</param>
                /// <param name="bytes">Bytes, used for encoding position data in a parent list or <see langword="default"/> if cannot be provided.</param>
                /// <param name="ushorts">UShorts, used for encoding position data in a parent list or <see langword="default"/> if cannot be provided.</param>
                /// <param name="uints">UInts, used for encoding position data in a parent list or <see langword="default"/> if cannot be provided.</param>
                /// <param name="total">Total amount of items, stored in the <paramref name="items"/> array.</param>
                /// <param name="flags">Flags, describing which <typeparamref name="TFilter"/> items are stored in <paramref name="items"/> array.</param>
                /// <param name="stored">Amount of filtered items. When all will be found - iteration will stop earlier.</param>
                internal Enumerator(
                    TBase[] items, PackingMode mode, Span<byte> bytes, Span<ushort> ushorts, Span<uint> uints, int total, // CRTP List data.
                    uint[] flags, int stored) // Lookup data.
                {
                    this.items = items;
                    this.mode = mode;
                    this.bytes = bytes;
                    this.ushorts = ushorts;
                    this.uints = uints;
                    this.flags = flags;
                    this.stored = Math.Min(total, stored);
                }
                /// <summary>
                /// Retrieves item under current iterator state.
                /// </summary>
                /// <exception cref="ArgumentOutOfRangeException">
                /// Enumerator is either not started or accessed when all possible items were already iterated over.
                /// </exception>
                public readonly TFilter Current => (TFilter)items[index];
                /// <summary>
                /// Moves enumerator forward.
                /// </summary>
                /// <returns>
                /// <see langword="true"/> if there was more items to iterate over, and <see cref="Current"/> was assigned.
                /// <see langword="false"/> if there was no more items, and <see cref="Current"/> was set to <see langword="default"/> value.
                /// </returns>
                public bool MoveNext()
                {
                    if (passed >= stored)
                    {
                        index = -1;
                        return false;
                    }

                    iterator++;
                    if ((iterator >> 5) >= flags.Length)
                    {
                        index = -1;
                        passed = stored;
                        return false;
                    }

                    // TODO: Optimize lookups by processing own flags in bulk - in a set of 4 bits,
                    //  and mapping them to valid indexes using switch jump-table.
                    //  With such approach, sets of 4 bits can be discarded if jump-table lands on '0' (i.e. no flags)
                    //  Or come up with even better approach using only bit masks.
                    uint region = flags[iterator >> 5];
                    switch (mode)
                    {
                        case PackingMode.Size0: passed = stored; return false;
                        case PackingMode.SizeByte:
                            while ((region & (1u << (iterator & 0b11111))) == 0u)
                            {
                                iterator++;
                            }
                            index = bytes[iterator] & PackingByte.ByteValue;
                            passed++;
                            return true;

                        case PackingMode.SizeUShort:
                            while ((region & (1u << (iterator & 0b11111))) == 0u)
                            {
                                iterator++;
                            }
                            index = ushorts[iterator] & PackingUShort.UShortValue;
                            passed++;
                            return true;

                        case PackingMode.SizeUInt:
                            while ((region & (1u << (iterator & 0b11111))) == 0u)
                            {
                                iterator++;
                            }
                            index = (int)(uints[iterator] & PackingUInt.ItemValue);
                            passed++;
                            return true;

                        default: throw new SwitchExpressionException(mode);
                    }
                }
            }
        }
    }
}
