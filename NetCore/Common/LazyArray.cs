using System;
using System.Collections;
using System.Collections.Generic;

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
        /// <inheritdoc/>
        public readonly T this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }

        /// <inheritdoc/>
        public readonly int Count => list.Count;

        /// <inheritdoc/>
        public readonly bool IsReadOnly => false;




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
        
        /// <inheritdoc/>
        public void Add(T item)
        {
            list.Add(item);
            array = null;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            if (list.Count == 0)
            {
                return;
            }

            list.Clear();
            array = null;
        }

        /// <inheritdoc/>
        public readonly bool Contains(T item) => list.Contains(item);

        /// <inheritdoc/>
        public readonly void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

        /// <inheritdoc/>
        public readonly IEnumerator<T> GetEnumerator() => list.GetEnumerator();
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
    }
}
