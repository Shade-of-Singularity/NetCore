namespace NetCore
{
    /// <summary>
    /// Header encoding <see cref="RequestType"/>.
    /// </summary>
    public sealed class RequestTypeHeader : CustomHeader<RequestTypeHeader>
    {
        /// <inheritdoc/>
        public override int Size => 1;
        /// <summary>
        /// Encodes provided <see cref="RequestType"/> in a given <see cref="Header"/>.
        /// </summary>
        public static void Encode(in Header header, RequestType type)
        {
            header.Set<RequestTypeHeader, RequestType>(type);
        }
        /// <summary>
        /// Attempts to decode <see cref="RequestType"/> from a given <see cref="Header"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> - if <see cref="RequestTypeHeader"/> was present and was decoded.
        /// <c>false</c> - otherwise, and <paramref name="type"/> was given a default value.
        /// </returns>
        public static bool TryDecode(in Header header, out RequestType type)
        {
            return header.TryGet<RequestTypeHeader, RequestType>(out type);
        }
        /// <summary>
        /// Decodes <see cref="RequestType"/> from a given <see cref="Header"/>, or provides default request type.
        /// </summary>
        public static RequestType Decode(in Header header)
        {
            return header.GetEnum<RequestTypeHeader, RequestType>();
        }
        /// <summary>
        /// Decodes <see cref="RequestType"/> from a given <see cref="Header"/>.
        /// </summary>
        public static void Decode(in Header header, out RequestType type)
        {
            header.Get<RequestTypeHeader, RequestType>(out type);
        }
    }
}
