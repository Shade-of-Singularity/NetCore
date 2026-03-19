using System;
using System.Buffers;

namespace NetCore
{
    /// <summary>
    /// Message header which allows writing custom data to it.
    /// </summary>
    /// <remarks>
    /// Can be created using <see cref="Get"/> or <see cref="GetLocked"/>.
    /// </remarks>
    /// TODO: Add a way to parse a beginning of the message into a valid header without using <see cref="NetworkMember"/> as a base.
    ///  This will be useful if people would want to create custom relays which does not rely on <see cref="ITransport"/>s.
    /// TODO: Support dynamic header size.
    /// TODO: Rename to simply "Header" (?)
    public ref struct Header(byte[] headers, byte[] content)
    {
        /// <summary>
        /// Array, encoding all the bits, representing used headers.
        /// </summary>
        /// TODO: Support array being null when using <c>default</c> declaration.
        public readonly byte[] headers = headers;
        /// <summary>
        /// Array, holding unpacked <see cref="CustomHeader{T}"/> data.
        /// Layer or will be packed depending on specified <see cref="CustomHeaderUsage"/>.
        /// </summary>
        /// TODO: Support array being null when using <c>default</c> declaration.
        public readonly byte[] content = content;
        /// <summary>
        /// Stores all flags this header defines. Includes:
        /// <para>- <see cref="CustomHeaderUsage"/> (assigned automatically).</para>
        /// </summary>
        /// <remarks>
        /// Flags that are assigned automatically usually should not be touched.
        /// If you want to touch them though - use methods from <see cref="HeaderFlagsHelpers"/>.
        /// </remarks>
        public HeaderFlags flags;
        /// <summary>
        /// Whether instance is disposed or not.
        /// </summary>
        private bool disposed;
        /// <summary>
        /// Amount of sources using this header in a <![CDATA[using (var header = ...) { }]]> context.
        /// </summary>
        private ushort locks;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Static Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Creates <see cref="Header"/>, optimized for usage with currently registered <see cref="ITransport"/>s.
        /// </summary>
        /// <remarks>
        /// Don't forget to call <see cref="Dispose"/> when you are done using it!
        /// Or use <see cref="HeaderHelpers.Lock(ref Header)"/> and use it inside of an
        /// <![CDATA[using (var header = Header.Get().Lock()) {}]]> block.
        /// </remarks>
        public static Header Get()
        {
            byte[] headers = ArrayPool<byte>.Shared.Rent((CustomHeaders.Amount + 7) >> 3);
            byte[] content = ArrayPool<byte>.Shared.Rent(CustomHeaders.MaxContentSizeInBytes);
            return new Header(headers, content);
        }

        /// <summary>
        /// <inheritdoc cref="Get"/>
        /// </summary>
        /// <remarks>
        /// Automatically locks the <see cref="Header"/> before returning it, for usage in
        /// <![CDATA[using (var header = Header.GetLocked()) {}]]> block.
        /// </remarks>
        /// <returns><see cref="Header"/> locked once.</returns>
        public static Header GetLocked()
        {
            Header result = Get();
            result.IncrementLocks();
            return result;
        }




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Public Methods
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Checks if a specific <see cref="CustomHeader{T}"/> is defined or not.
        /// </summary>
        public readonly bool Has<T>() where T : CustomHeader<T>, new()
        {
            return (headers[CustomHeader<T>.Region] & CustomHeader<T>.RegionFlag) != 0;
        }

        /// <summary>
        /// Removes target <see cref="CustomHeader{T}"/> and wipes the data under it.
        /// </summary>
        public readonly void Remove<T>() where T : CustomHeader<T>, new()
        {
            if (CustomHeader<T>.IsSensitive && CustomHeader<T>.SizeInBytes != 0)
            {
                content.AsSpan(CustomHeader<T>.ContentPosition, CustomHeader<T>.SizeInBytes).Fill(default);
            }

            ref byte flags = ref headers[CustomHeader<T>.Region];
            flags &= (byte)~CustomHeader<T>.RegionFlag;
        }

        /// <summary>
        /// Reads a header data from the <see cref="content"/> buffer into provided <paramref name="buffer"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <exception cref="HeaderNotFoundException">Header was not defined yet.</exception>
        /// <exception cref="InsufficientBufferException"><paramref name="buffer"/> is too small to contain a header.</exception>
        public readonly void Get<T>(in Span<byte> buffer) where T : CustomHeader<T>, new()
        {
            if ((headers[CustomHeader<T>.Region] & CustomHeader<T>.RegionFlag) == 0)
            {
                throw new HeaderNotFoundException(typeof(T));
            }

            if (buffer.Length < CustomHeader<T>.SizeInBytes)
            {
                throw new InsufficientBufferException(nameof(buffer), buffer.Length, CustomHeader<T>.SizeInBytes);
            }

            content.AsSpan(CustomHeader<T>.ContentPosition, CustomHeader<T>.SizeInBytes).CopyTo(buffer);
        }

        /// <summary>
        /// Tries to read a header data from the <see cref="content"/> buffer into provided <paramref name="buffer"/> <see cref="Span{T}"/>.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns>
        /// <c>true</c> - if header was present and were read successfully.
        /// <c>false</c> - if header was not present.
        /// </returns>
        /// <exception cref="InsufficientBufferException"><paramref name="buffer"/> is too small to contain a header.</exception>
        public readonly bool TryGet<T>(in Span<byte> buffer) where T : CustomHeader<T>, new()
        {
            if ((headers[CustomHeader<T>.Region] & CustomHeader<T>.RegionFlag) == 0)
            {
                return false;
            }

            if (buffer.Length < CustomHeader<T>.SizeInBytes)
            {
                throw new InsufficientBufferException(nameof(buffer), buffer.Length, CustomHeader<T>.SizeInBytes);
            }

            content.AsSpan(CustomHeader<T>.ContentPosition, CustomHeader<T>.SizeInBytes).CopyTo(buffer);
            return true;
        }

        /// <summary>
        /// Flags custom header as set, setting all the bits it encodes (if any) to 0.
        /// </summary>
        /// <remarks>
        /// If header is already set - does not overwrite the data.
        /// </remarks>
        public readonly void Set<T>() where T : CustomHeader<T>, new()
        {
            ref byte flags = ref headers[CustomHeader<T>.Region];
            if ((flags & CustomHeader<T>.RegionFlag) != 0)
            {
                return;
            }

            if (CustomHeader<T>.SizeInBytes != 0)
            {
                // Wipes any previous data.
                content.AsSpan(CustomHeader<T>.ContentPosition, CustomHeader<T>.SizeInBytes).Fill(default);
            }

            flags |= CustomHeader<T>.RegionFlag;
        }

        /// <summary>
        /// Writes a byte data to the target header.
        /// </summary>
        /// <remarks>
        /// Does overwrite any existing data.
        /// </remarks>
        public readonly void Set<T>(in ReadOnlySpan<byte> bytes) where T : CustomHeader<T>, new()
        {
            if (CustomHeader<T>.SizeInBytes != 0)
            {
                if (bytes.Length > CustomHeader<T>.SizeInBytes)
                {
                    throw new ArgumentOutOfRangeException($"Span '{nameof(bytes)}' provides more buffer ({bytes.Length}) than the header can hold ({CustomHeader<T>.SizeInBytes}). Please slice the span if you only need to write a part of it, or increase header size.");
                }

                var span = content.AsSpan(CustomHeader<T>.ContentPosition, CustomHeader<T>.SizeInBytes);
                bytes.CopyTo(span);

                // If there are any unset bytes beyond what input span provides - they are set to 0.
                if (bytes.Length != CustomHeader<T>.SizeInBytes)
                    span[bytes.Length..].Fill(default);
            }

            ref byte flags = ref headers[CustomHeader<T>.Region];
            flags |= CustomHeader<T>.RegionFlag;
        }

        /// <summary>
        /// Increments the amount of locks used on the header.
        /// </summary>
        public void IncrementLocks()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(Header));

            locks++;
        }

        /// <summary>
        /// Releases all internal resources.
        /// </summary>
        /// <exception cref="ObjectDisposedException"><see cref="Header"/> is already disposed.</exception>
        /// Note: We can mutate what Dispose does only because we use ref struct.
        ///  Ref structs cannot define interfaces as of right now, so we cannot use <see cref="IDisposable"/>.
        ///  Thus its functionality is open for innovations.
        public void Dispose()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(Header));

            switch (locks)
            {
                case 0: break;
                case 1: locks--; break;
                default: locks--; return; // Do not dispose.
            }

            // Wipes sensitive data before returning.
            foreach (var supplier in CustomHeaders.GetSensitiveRegions())
            {
                supplier(out int contentPosition, out int sizeInBytes);
                for (int i = 0; i < sizeInBytes; i++)
                {
                    content[contentPosition + i] = default;
                }
            }

            ArrayPool<byte>.Shared.Return(headers);
            ArrayPool<byte>.Shared.Return(content);
            disposed = true;
        }
    }
}
