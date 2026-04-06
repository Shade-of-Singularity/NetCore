using ComputerysBitStream;

namespace NetCore.Packets
{
    /// <summary>
    /// Base class for all valid network packets.
    /// </summary>
    /// <typeparam name="T">Type of a class, implementing this packet.</typeparam>
    public abstract class Packet<T> : Packet where T : Packet<T>
    {

    }

    /// <summary>
    /// Base class for network packets. Use <see cref="Packet{T}"/> for actually implementing the packets.
    /// </summary>
    public abstract class Packet
    {
        /// <summary>
        /// Reads packet data from given <see cref="ReadContext"/>.
        /// </summary>
        public abstract void Read(in ReadContext context);
        /// <summary>
        /// Writes packet data to a given <see cref="WriteContext"/>.
        /// </summary>
        public abstract void Write(in WriteContext context);
    }
}
