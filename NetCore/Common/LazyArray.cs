using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace NetCore.Common
{
    /// <summary>
    /// Resizable array, which are only initialized when you actually use ToArray() method.
    /// </summary>
    /// <typeparam name="T">Item internal array and list contain.</typeparam>
    public struct LazyArray<T>() : IList<T>
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Public Properties
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Gets or sets value in the underlying list/array.
        /// </summary>
        public readonly T this[int index]
        {
            get => list[index];
            set
            {
                if (array is not null)
                {
                    array[index] = value;
                }

                list[index] = value;
            }
        }

        /// <summary>
        /// Amount of items in the array.
        /// </summary>
        public readonly int Count => list.Count;

        /// <inheritdoc/>
        readonly bool ICollection<T>.IsReadOnly => false;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        private readonly List<T> list = [];
        private T[]? array;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Turns internal list into array, and caches it to be returned again next time.
        /// </summary>
        public T[] ToArray() => array ??= [.. list];

        /// <summary>
        /// Adds item to the list.
        /// </summary>
        /// <param name="item">Item to add.</param>
        public void Add(T item)
        {
            list.Add(item);
            array = null;
        }

        /// <summary>
        /// Clears the list.
        /// </summary>
        public void Clear()
        {
            if (list.Count == 0)
            {
                return;
            }

            list.Clear();
            array = null;
        }

        /// <summary>
        /// Checks if list contains target item.
        /// </summary>
        /// <param name="item">Item to check for.</param>
        /// <returns>
        /// <c>true</c> - if <paramref name="item"/> was found.
        /// <c>false</c> - otherwise.
        /// </returns>
        public readonly bool Contains(T item) => list.Contains(item);

        /// <summary>
        /// Copies all items from the list to a target array.
        /// </summary>
        /// <param name="array">Array to cope the items to.</param>
        /// <param name="arrayIndex">Index from which to start.</param>
        public readonly void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

        /// <summary>
        /// Retrieves enumerator, enumerating over the entire list.
        /// </summary>
        public Enumerator GetEnumerator() => new(ToArray());
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => list.GetEnumerator();
        readonly IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

        /// <inheritdoc/>
        public readonly int IndexOf(T item) => list.IndexOf(item);

        /// <inheritdoc/>
        public void Insert(int index, T item)
        {
            list.Insert(index, item);
            array = null;
        }

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            if (list.Remove(item))
            {
                array = null;
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
            array = null;
        }

        /// <summary>
        /// Custom struct-based enumerator for iterating over <see cref="LazyArray{T}"/>.
        /// </summary>
        /// <remarks>
        /// Using it will automatically trigger <see cref="ToArray"/>.
        /// </remarks>
        /// <param name="items">Items to iterate over.</param>
        public ref struct Enumerator(T[] items)
        {
            private readonly T[] items = items;
            private int iterator = -1;
            /// <summary>
            /// Retrieves currently enumerated item.
            /// </summary>
            public readonly T Current => iterator < items.Length ? items[iterator] : default!;
            /// <summary>
            /// Moves the enumerator forward.
            /// </summary>
            /// <returns>
            /// <c>true</c> - there are more items to iterate over, and <see cref="Current"/> was assigned.
            /// <c>false</c> - there are no more items to iterate over.
            /// </returns>
            public bool MoveNext() => ++iterator < items.Length;
        }
    }
}
