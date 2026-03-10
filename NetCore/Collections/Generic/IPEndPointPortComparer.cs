using System.Collections.Generic;
using System.Net;

namespace NetCore.Collections.Generic
{
    /// <summary>
    /// Compares only port from an <see cref="IPEndPoint"/>.
    /// </summary>
    public sealed class IPEndPointPortComparer : IEqualityComparer<IPEndPoint>
    {
        /// <summary>
        /// Default instance of a <see cref="IPEndPointPortComparer"/>.
        /// </summary>
        public static readonly IPEndPointPortComparer Default = new();

        /// <inheritdoc/>
        public bool Equals(IPEndPoint x, IPEndPoint y) => x.Port == y.Port;

        /// <inheritdoc/>
        public int GetHashCode(IPEndPoint obj) => obj.Port.GetHashCode();
    }
}
