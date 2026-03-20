using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NetCore.Identity
{
    /// <summary>
    /// Temporary identifier used for identifying the initial connection.
    /// Encoded as <see cref="TemporaryIdentifierHeader"/>.
    /// </summary>
    public readonly struct TemporaryIdentifier(ulong raw) : IEquatable<TemporaryIdentifier>
    {
        /// <summary>
        /// Size of <see cref="TemporaryIdentifier"/> in bits.
        /// </summary>
        public const int SizeInBits = sizeof(ulong) * 8;
        /// <summary>
        /// Size of <see cref="TemporaryIdentifier"/> in bytes.
        /// </summary>
        public const int SizeInBytes = (SizeInBits + 7) / 8;
        /// <summary>
        /// Size of <see cref="TemporaryIdentifier"/> when encoded as Base64.
        /// </summary>
        public const int SizeInBase64 = (SizeInBits + 5) / 6;
        /// <summary>
        /// Size of <see cref="TemporaryIdentifier"/> when encoded as Base256.
        /// </summary>
        public const int SizeInBase256 = SizeInBytes;
        /// <summary>
        /// Raw representation of the identifier.
        /// </summary>
        public readonly ulong raw = raw;
        /// <summary>
        /// Creates new temporary identifier.
        /// </summary>
        public TemporaryIdentifier Get()
        {
            Span<byte> bytes = stackalloc byte[8];
            RandomNumberGenerator.Fill(bytes);
            return new (Unsafe.As<byte, ulong>(ref bytes[0]));
        }
        /// <summary>
        /// Encodes this <see cref="TemporaryIdentifier"/> as Base64.
        /// </summary>
        /// <param name="span">Span to fill-in with identifier data. Must have a size of at least <see cref="SizeInBase64"/></param>
        public void EncodeBase64(Span<byte> span)
        {
            throw new NotImplementedException("Base64 is not supported yet - we need to port it from ServiceCore.");
            //Span<ulong> storage = stackalloc ulong[1];
            //storage[0] = raw;
            //Span<byte> bytes = MemoryMarshal.AsBytes(storage);
            //bytes.CopyTo(span);
        }
        /// <summary>
        /// Encodes this <see cref="TemporaryIdentifier"/> as Base256.
        /// </summary>
        /// <param name="span">Span to fill-in with identifier data. Must have a size of at least <see cref="SizeInBase256"/></param>
        public void EncodeBase256(Span<byte> span)
        {
            Span<ulong> storage = stackalloc ulong[1];
            storage[0] = raw;
            Span<byte> bytes = MemoryMarshal.AsBytes(storage);
            bytes.CopyTo(span);
        }
        /// <inheritdoc/>
        public bool Equals(TemporaryIdentifier other) => other.raw == raw;
        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is TemporaryIdentifier identifier && Equals(identifier);
        /// <inheritdoc/>
        public override int GetHashCode() => raw.GetHashCode();
        /// <inheritdoc/>
        public override string ToString() => raw.ToString(); // TODO: Return as Base64.
    }
}
