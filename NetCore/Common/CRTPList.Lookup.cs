using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NetCore.Common
{
    public partial class CRTPList<TBase>
    {
        internal static class Lookup
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static int RegionFrom(int order) => order >> 5;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint FlagFrom(int order) => (uint)(order & 0b11111);
        }

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
        /// Note: Consider turning it into a standardized data structure, allowing for pretty fast iteration over the filtered items + counting of them.
        ///  Or if it will not be efficient - provide helper methods instead, and consider moving then to SoG.Common library.
        /// TODO: Add a base class/interface implementing <see cref="Count"/>, for basic size checks.
        ///  This can simplify some switch operations in <see cref="NetworkMember"/>, e.g. <see cref="NetworkMember.HasAnyTransport(SendingMode)"/>.
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
                list.ItemRemoved += ItemRemoved;
            }
            /// <summary>
            /// Destructor which unsubscribed from the internal callbacks on a parent list.
            /// </summary>
            public void Dispose()
            {
                list.ItemRemoved -= ItemRemoved;
                filters = null!;
            }




            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
            /// .
            /// .                                               Update Handling
            /// .
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
            private void ItemRemoved(int order)
            {
                if (order == RemoveAll)
                {
                    Array.Fill(filters, default);
                    stored = 0;
                    return;
                }

                int region = order >> 5;
                if (region > filters.Length)
                {
                    return;
                }

                uint flag = 1u << (order & 0b11111);
                ref uint filter = ref filters[region];
                if ((filter & flag) != 0)
                {
                    stored--;
                    filter &= ~flag;
                }
            }




            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
            /// .
            /// .                                               Public Methods
            /// .
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
            /// <inheritdoc cref="CRTPList{TBase}.Has{TItem}()"/>
            public bool Has<TItem>() where TItem : TBase
            {
                int region = Lookup.RegionFrom(ID<TItem>.Order);
                if (region >= filters.Length)
                {
                    return false; // Handles out-of-bounds.
                }

                return (filters[region] & Lookup.FlagFrom(ID<TItem>.Order)) != 0;
            }

            /// <inheritdoc cref="CRTPList{TBase}.TryGet{TItem}(out TItem)"/>
            public bool TryGet<TItem>([NotNullWhen(true)] out TItem? item) where TItem : TFilter
            {
                int region = Lookup.RegionFrom(ID<TItem>.Order);
                if (region >= filters.Length)
                {
                    item = default;
                    return false; // Handles out-of-bounds.
                }

                uint flag = Lookup.FlagFrom(ID<TItem>.Order);
                if ((filters[region] & flag) == 0)
                {
                    item = default;
                    return false; // Item does not exist.
                }

                item = (TItem)list.GetInternalUnchecked(ID<TItem>.Order);
                return true;
            }

            /// <inheritdoc cref="CRTPList{TBase}.Get{TItem}()"/>
            public TItem Get<TItem>() where TItem : TFilter
            {
                int region = Lookup.RegionFrom(ID<TItem>.Order);
                if (region >= filters.Length)
                {
                    throw new ItemNotFoundException(typeof(TItem));
                }

                uint flag = Lookup.FlagFrom(ID<TItem>.Order);
                if ((filters[region] & flag) == 0)
                {
                    throw new ItemNotFoundException(typeof(TItem));
                }

                if (list.GetInternalUnchecked(ID<TItem>.Order) is not TItem item)
                {
                    throw new ItemNotFoundException(typeof(TItem));
                }

                return item;
            }

            /// <inheritdoc cref="CRTPList{TBase}.SafeGet{TItem}()"/>
            public TItem? SafeGet<TItem>() where TItem : class, TFilter
            {
                int region = Lookup.RegionFrom(ID<TItem>.Order);
                if (region >= filters.Length)
                {
                    return default;
                }

                uint flag = Lookup.FlagFrom(ID<TItem>.Order);
                if ((filters[region] & flag) == 0)
                {
                    return default;
                }

                if (list.GetInternalUnchecked(ID<TItem>.Order) is not TItem item)
                {
                    return default;
                }

                return item;
            }

            /// <inheritdoc cref="CRTPList{TBase}.Add{TItem}(TItem)"/>
            public bool Add<TItem>(TItem item) where TItem : TFilter
            {
                if (item is null)
                    throw new ArgumentNullException(nameof(item));

                if (typeof(TItem) != item.GetType())
                    throw new ItemTypeMismatchException(typeof(TItem), item.GetType());

                return AddInternal(ID<TItem>.Order, item);
            }

            /// <seealso cref="CRTPList{TBase}.AddInternal(int, TBase)"/>
            bool AddInternal<TItem>(int order, TItem item) where TItem : TFilter
            {
                EnsureFilterCapacity(order + 1);

                ref uint filter = ref filters[Lookup.RegionFrom(order)];
                uint flag = Lookup.FlagFrom(order);
                if ((filter & flag) != 0)
                {
                    return false;
                }

                filter |= flag;
                return list.AddInternal(order, item);
            }

            /// <inheritdoc cref="CRTPList{TBase}.Remove{TItem}()"/>
            public bool Remove<TItem>() where TItem : TFilter => RemoveInternal<TItem>(ID<TItem>.Order, default, out var _);

            /// <inheritdoc cref="CRTPList{TBase}.Remove{TItem}(out TItem)"/>
            public bool Remove<TItem>([NotNullWhen(true)] out TItem? item) where TItem : TFilter => RemoveInternal(ID<TItem>.Order, default, out item);

            /// <inheritdoc cref="CRTPList{TBase}.Remove{TItem}(TItem)"/>
            public bool Remove<TItem>(TItem item) where TItem : TFilter
            {
                if (item is null)
                    throw new ArgumentNullException(nameof(item));

                if (typeof(TItem) != item.GetType())
                    throw new ItemTypeMismatchException(typeof(TItem), item.GetType());

                return RemoveInternal(ID<TItem>.Order, item, out var _);
            }

            bool RemoveInternal<TItem>(int order, TItem? criteria, [NotNullWhen(true)] out TItem? item) where TItem : TFilter
            {
                int region = Lookup.RegionFrom(order);
                if (region >= filters.Length)
                {
                    item = default;
                    return false;
                }

                uint flag = Lookup.FlagFrom(order);
                if ((filters[region] & flag) == 0)
                {
                    item = default;
                    return false;
                }

                return list.RemoveInternal(order, criteria, out item);
            }

            // TODO: Add the rest of the methods, including an enumerator.
            /// <summary>
            /// Retrieves special struct-based enumerator for iterating over items, filtered with <typeparamref name="TFilter"/> type.
            /// </summary>
            /// <returns>Special struct-based enumerator for very fast type-safe iterations.</returns>
            public LookupEnumerator GetEnumerator() => new(list.items, total: list.stored, filters, stored);

            /// <summary>
            /// Struct-based enumerator for fast iterations over filtered items.
            /// </summary>
            public ref struct LookupEnumerator
            {
                // CRTP List data:
                readonly TBase[] items;
                // Lookup data:
                readonly uint[] filters;
                readonly int stored;
                int iterator = -1;
                /// <param name="items">Reference to the internal array of items from <see cref="CRTPList{TBase}"/>.</param>
                /// <param name="total">Total amount of items, stored in the <paramref name="items"/> array.</param>
                /// <param name="filters">Flags, describing which <typeparamref name="TFilter"/> items are stored in <paramref name="items"/> array.</param>
                /// <param name="stored">Amount of filtered items. When all will be found - iteration will stop earlier.</param>
                internal LookupEnumerator(
                    TBase[] items, int total, // CRTP List data.
                    uint[] filters, int stored) // Lookup data.
                {
                    this.items = items;
                    this.filters = filters;
                    this.stored = Math.Min(total, stored);
                }
                /// <summary>
                /// Retrieves item under current iterator state.
                /// </summary>
                /// <exception cref="ArgumentOutOfRangeException">
                /// Enumerator is either not started or accessed when all possible items were already iterated over.
                /// </exception>
                public readonly TFilter Current => (TFilter)items[iterator];
                /// <summary>
                /// Moves enumerator forward.
                /// </summary>
                /// <returns>
                /// <see langword="true"/> if there was more items to iterate over, and <see cref="Current"/> was assigned.
                /// <see langword="false"/> if there was no more items, and <see cref="Current"/> was set to <see langword="default"/> value.
                /// </returns>
                public bool MoveNext()
                {
                    while (++iterator < stored)
                    {
                        int region = Lookup.RegionFrom(iterator);
                        uint flag = Lookup.FlagFrom(iterator);
                        if ((filters[region] & flag) != 0)
                        {
                            break;
                        }
                    }

                    return iterator < stored;
                }
            }




            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
            /// .
            /// .                                               Private Methods
            /// .
            /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
            private void EnsureFilterCapacity(int order)
            {
                // TODO: Implement load factor.
                int filterSize = (order + 32) >> 5;
                if (filterSize > filters.Length)
                {
                    Array.Resize(ref filters, order);
                }
            }
        }
    }
}
