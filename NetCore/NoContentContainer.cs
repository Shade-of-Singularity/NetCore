namespace NetCore
{
    /// <summary>
    /// Container for storing a flag-only container value.
    /// </summary>
    /// <typeparam name="TValue">Value type this container stores.</typeparam>
    public sealed class NoContentContainer<TValue> : FlagsContainer where TValue : INoContentFlag
    {
        /// <inheritdoc cref="InitOrder"/>
        public static readonly int Order = CRTPIDSource<INoContentFlag>.NextOrder();
        /// <inheritdoc/>
        public override int InitOrder => Order;
    }
}
