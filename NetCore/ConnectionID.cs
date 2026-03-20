using NetCore.Transports;
using System;

namespace NetCore
{
    /// <summary>
    /// Represents a unique identifier of an active/inactive connection of a <see cref="ITransport"/>.
    /// </summary>
    public readonly struct ConnectionID(ulong raw) : IEquatable<ConnectionID>
    {
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                               Private Fields
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        public readonly ulong raw = raw;




        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===<![CDATA[
        /// .
        /// .                                              Implementations
        /// .
        /// ===     ===     ===     ===    ===  == =  -                        -  = ==  ===    ===     ===     ===     ===]]>
        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is ConnectionID cid && cid.raw == raw;

        /// <inheritdoc/>
        public override int GetHashCode() => raw.GetHashCode();

        /// <inheritdoc/>
        public override string ToString() => raw.ToString();

        /// <inheritdoc/>
        public bool Equals(ConnectionID other) => other.raw == raw;

        /// <summary>
        /// Compares raw values of the structs.
        /// </summary>
        public static bool operator ==(ConnectionID a, ConnectionID b) => a.raw == b.raw;

        /// <summary>
        /// Compares raw values of the structs.
        /// </summary>
        public static bool operator !=(ConnectionID a, ConnectionID b) => a.raw != b.raw;

        /// <summary>
        /// Casts connection ID directly to its raw representation.
        /// </summary>
        public static explicit operator ulong(ConnectionID cid) => cid.raw;

        /// <summary>
        /// Casts raw connection ID to a struct.
        /// </summary>
        public static explicit operator ConnectionID(ulong cid) => new(cid);
    }
}
