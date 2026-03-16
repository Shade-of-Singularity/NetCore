using System;

namespace NetCore
{
    /// <summary>
    /// Storage for bits for reading and writing in <see cref="HeaderReader"/> and <see cref="HeaderWriter"/> respectively.
    /// </summary>
    /// <param name="storage">Storage for storing the bits.</param>
    [Obsolete]
    public ref struct BitStorage(ulong[] storage)
    {
        /// <summary>
        /// How many bits this storage can hold.
        /// </summary>
        public readonly int BitCapacity => storage.Length << 6;
        /// <summary>
        /// How many bytes this storage can hold.
        /// </summary>
        public readonly int ByteCapacity => storage.Length;
        /// <summary>
        /// Storage of <see cref="ulong"/>s to store the bits in.
        /// </summary>
        public readonly ulong[] storage = storage;
        /// <summary>
        /// Whether the storage was released or not.
        /// </summary>
        private bool released = false;
        /// <summary>
        /// Returns the internal array to the pool.
        /// </summary>
        public void Dispose()
        {
            if (released) return;
            released = true;
            CustomHeaders.Return(storage);
        }
    }
}
