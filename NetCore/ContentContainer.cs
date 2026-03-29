using NetCore.Common;

namespace NetCore
{
    /// <summary>
    /// Container for storing an item within a sending <see cref="Flags"/> storage.
    /// </summary>
    /// <typeparam name="TValue">Value this container stores.</typeparam>
    public class ContentContainer<TValue> : FlagsContainer where TValue : IContentFlags
    {
        /// <inheritdoc cref="InitOrder"/>
        public static readonly int Order = CRTPIDSource<IContentFlags>.NextOrder();
        /// <inheritdoc/>
        public override int InitOrder => Order;
        /// <summary>
        /// Value this container stores.
        /// </summary>
        public TValue? Value => m_Value;
        /// <summary>
        /// Value stored in this container.
        /// </summary>
        private TValue? m_Value;
        /// <summary>
        /// Rents an instance of a container from a <see cref="CRTPPool{T}"/>.
        /// </summary>
        /// <returns></returns>
        public static ContentContainer<TValue> Rent() => CRTPPool<ContentContainer<TValue>>.Rent();
        /// <summary>
        /// Returns an instance of a <paramref name="container"/> to a <see cref="CRTPPool{T}"/>.
        /// </summary>
        /// <param name="container">Container to return.</param>
        public static void Return(ContentContainer<TValue> container)
        {
            if (container is null) return;
            CRTPPool<ContentContainer<TValue>>.Return(container);
        }
        /// <summary>
        /// Sets value to a <see cref="Value"/> property.
        /// </summary>
        /// <param name="value">Value to set to a <see cref="Value"/> property.</param>
        /// <returns>A reference to itself.</returns>
        public ContentContainer<TValue> Set(TValue? value)
        {
            m_Value = value;
            return this;
        }
        /// <summary>
        /// Resets <see cref="Value"/> to a default value.
        /// </summary>
        /// <returns>A reference to itself.</returns>
        public ContentContainer<TValue> Reset()
        {
            m_Value = default;
            return this;
        }
    }
}
