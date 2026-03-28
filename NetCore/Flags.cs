using NetCore.Transports;
using System;
using System.Buffers;
using System.Runtime.InteropServices;

namespace NetCore
{
    /// <summary>
    /// (Not thread-safe!)
    /// Container for flags, which doesn't meant to move over the network, so they can be packed better.
    /// </summary>
    /// <remarks>
    /// Optimized for adding new flags, but removal is expensive.
    /// If possible - avoid removing flags, but instead overwrite a value they store, if possible.
    /// </remarks>
    /// TODO: Improve resizing logic.
    [StructLayout(LayoutKind.Explicit)]
    public ref struct Flags(uint[] flags, uint[] positions, FlagsContainer[] containers)
    {
        /// <summary>
        /// Amount of stored containers.
        /// </summary>
        public readonly int ContainerCount => containersCount;
        /// <summary>
        /// Flags representing which <see cref="INoContentFlag"/> are registered.
        /// </summary>
        [FieldOffset(0)] public uint[] flags = flags;
        /// <summary>
        /// Positions of all stored <see cref="FlagsContainer"/> encoded using tightly packed bits.
        /// </summary>
        [FieldOffset(8)] public uint[] positions = positions;
        /// <summary>
        /// References to all containers stored in this <see cref="Flags"/> instance.
        /// </summary>
        [FieldOffset(16)] public FlagsContainer[] containers = containers;
        /// <summary>
        /// Amount of containers stored in <see cref="containers"/>.
        /// </summary>
        [FieldOffset(24)] public int containersCount;
        /// <summary>
        /// Indicates that this <see cref="Flags"/> instance was disposed.
        /// </summary>
        [FieldOffset(28)] private bool disposed;
        /// <summary>
        /// Amount of locks this <see cref="Flags"/> instance currently holds.
        /// </summary>
        [FieldOffset(29)] private volatile ushort locks;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Static Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Creates <see cref="Flags"/>, optimized for usage with <see cref="NetworkMember"/> and <see cref="ITransport"/>s.
        /// </summary>
        /// <remarks>
        /// Don't forget to call <see cref="Dispose"/> when you are done using it!
        /// Or use <see cref="HeaderHelpers.Lock(ref Header)"/> and use it inside of an
        /// <![CDATA[using (var header = Header.Get().Lock()) {}]]> block.
        /// </remarks>
        public static Flags Get()
        {
            uint[] flags = ArrayPool<uint>.Shared.Rent((CRTPIDSource<INoContentFlag>.Amount + 32) >> 5);
            uint[] positions = ArrayPool<uint>.Shared.Rent(/*(CRTPIDSource<INoContentFlag>.Amount)*/ 4); // TODO: Calculate a proper pre-allocation amount.
            FlagsContainer[] containers = ArrayPool<FlagsContainer>.Shared.Rent(8); // TODO: Maybe provide better renting method for references?
            return new Flags(flags, positions, containers);
        }

        /// <summary>
        /// <inheritdoc cref="Get"/>
        /// </summary>
        /// <remarks>
        /// Automatically locks the <see cref="Flags"/> before returning it, for usage in
        /// <![CDATA[using (var header = Header.GetLocked()) {}]]> block.
        /// </remarks>
        /// <returns><see cref="Flags"/> locked once.</returns>
        public static Flags GetLocked()
        {
            Flags result = Get();
            result.IncrementLocks();
            return result;
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Increments the amount of locks used on the flags instance.
        /// </summary>
        public void IncrementLocks()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(Flags));

            locks++;
        }

        /// <summary>
        /// Releases one lock. Releases all internal resources if there are no more locks.
        /// </summary>
        /// <exception cref="ObjectDisposedException"><see cref="Header"/> is already disposed.</exception>
        /// Note: We can mutate what Dispose does only because we use ref struct.
        ///  Ref structs cannot define interfaces as of right now, so we cannot use <see cref="IDisposable"/>.
        ///  Thus its functionality is open for innovations.
        public void Dispose()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(Flags));

            switch (locks)
            {
                case 0: break;
                case 1: locks--; break;
                default: locks--; return; // Do not dispose.
            }

            ArrayPool<uint>.Shared.Return(flags, clearArray: true);
            ArrayPool<uint>.Shared.Return(positions, clearArray: true);
            ArrayPool<FlagsContainer>.Shared.Return(containers, clearArray: true);
            disposed = true;
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                          Public Methods: for flags
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Checks if a specific <see cref="INoContentFlag"/> is defined.
        /// </summary>
        public readonly bool HasFlag<T>() where T : INoContentFlag
        {
            if ((NoContentContainer<T>.Order >> 5) >= flags.Length)
            {
                return false;
            }

            return (flags[NoContentContainer<T>.Order >> 5] & (uint)NoContentContainer<T>.Order & 0b11111u) != 0;
        }
        /// <summary>
        /// Flips a bit (to true) which indicates that an <see cref="INoContentFlag"/> was set.
        /// </summary>
        public void SetFlag<T>() where T : INoContentFlag
        {
            if ((NoContentContainer<T>.Order >> 5) >= flags.Length)
            {
                Array.Resize(ref flags, (NoContentContainer<T>.Order + 32) >> 5);
            }

            flags[NoContentContainer<T>.Order >> 5] |= (uint)NoContentContainer<T>.Order & 0b11111u;
        }
        /// <summary>
        /// Removes an <see cref="INoContentFlag"/> indicator.
        /// </summary>
        public readonly void ResetFlags<T>() where T : INoContentFlag
        {
            if ((NoContentContainer<T>.Order >> 5) >= flags.Length)
            {
                return;
            }

            flags[NoContentContainer<T>.Order >> 5] &= ~((uint)NoContentContainer<T>.Order & 0b11111u);
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                       Public Methods: for containers
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Checks if a specific container is defined.
        /// </summary>
        public readonly bool Has<T>() where T : IContentFlags
        {
            throw new NotImplementedException();
        }
    }
}
