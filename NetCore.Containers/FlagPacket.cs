using ComputerysBitStream;
using System.ComponentModel;

namespace NetCore.Packets
{
    /// <summary>
    /// Packet with no content.
    /// Optimized by some internal systems, but not by much - feel free to use <see cref="Packet{T}"/> instead.
    /// </summary>
    /// <remarks>
    /// <see cref="Packet.Read"/> and <see cref="Packet.Write"/> methods are sealed, we assume <see cref="FlagPacket{T}"/> define no fields.
    /// </remarks>
    public abstract class FlagPacket<T> : Packet<T> where T : FlagPacket<T>
    {
        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override void Read(in ReadContext context) { }

        /// <inheritdoc/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public sealed override void Write(in WriteContext context) { }
    }
}
