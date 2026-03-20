using System;

namespace NetCore
{
    /// <summary>
    /// Describes an ID, which identifies a request from a specific client.
    /// </summary>
    /// <param name="raw">Raw representation of the ID.</param>
    /// TODO: 
    public readonly struct RequestID(uint raw) : IEquatable<RequestID>
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                                 Constants
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <summary>
        /// Size of this <see cref="RequestID"/> when encoded as <see cref="CustomHeader{T}"/>, in bytes.
        /// </summary>
        public const int SizeInBytes = sizeof(uint);
        /// <summary>
        /// Size of this <see cref="RequestID"/> when encoded as <see cref="CustomHeader{T}"/>, in bits.
        /// </summary>
        public const int SizeInBits = SizeInBytes * 8;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        public readonly uint raw = raw;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Implementations
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is RequestID rid && rid.raw == raw;

        /// <inheritdoc/>
        public override int GetHashCode() => raw.GetHashCode();

        /// <inheritdoc/>
        public override string ToString() => raw.ToString();

        /// <inheritdoc/>
        public bool Equals(RequestID other) => other.raw == raw;

        /// <summary>
        /// Compares raw values of the structs.
        /// </summary>
        public static bool operator ==(RequestID a, RequestID b) => a.raw == b.raw;

        /// <summary>
        /// Compares raw values of the structs.
        /// </summary>
        public static bool operator !=(RequestID a, RequestID b) => a.raw != b.raw;

        /// <summary>
        /// Casts connection ID directly to its raw representation.
        /// </summary>
        public static explicit operator uint(RequestID rid) => rid.raw;

        /// <summary>
        /// Casts raw connection ID to a struct.
        /// </summary>
        public static explicit operator RequestID(uint rid) => new(rid);
    }
}
