namespace NetCore
{
    /// <summary>
    /// Encodes <see cref="RequestID"/>.
    /// Usually only checked for if <see cref="RequestTypeHeader"/> is defined as well.
    /// </summary>
    public sealed class RequestIDHeader : CustomHeader<RequestIDHeader>
    {
        /// <inheritdoc/>
        public override int Size => RequestID.SizeInBits;
        /// <summary>
        /// Encodes provided <see cref="RequestType"/> in a given <see cref="Header"/>.
        /// </summary>
        public static void Encode(in Header header, RequestID rid)
        {
            header.Set<RequestTypeHeader>(rid.raw);
        }
        /// <summary>
        /// Attempts to decode <see cref="RequestID"/> from a given <see cref="Header"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> - if <see cref="RequestTypeHeader"/> was present and was decoded.
        /// <c>false</c> - otherwise, and <paramref name="type"/> was given a default value.
        /// </returns>
        public static bool TryDecode(in Header header, out RequestID type)
        {
            if (header.TryGet<RequestTypeHeader>(out uint result))
            {
                type = (RequestID)result;
                return true;
            }

            type = default;
            return false;
        }
        /// <summary>
        /// Decodes <see cref="RequestID"/> from a given <see cref="Header"/>, or provides default request type.
        /// </summary>
        public static RequestID Decode(in Header header)
        {
            return (RequestID)header.GetUInt<RequestTypeHeader>();
        }
        /// <summary>
        /// Decodes <see cref="RequestID"/> from a given <see cref="Header"/>.
        /// </summary>
        public static void Decode(in Header header, out RequestID type)
        {
            type = (RequestID)header.GetUInt<RequestTypeHeader>();
        }
    }
}
