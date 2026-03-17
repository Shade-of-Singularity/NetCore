namespace NetCore
{
    /// <summary>
    /// Encompasses information about <see cref="CustomHeader{T}"/>, needed for header initialization.
    /// </summary>
    /// <param name="Provider">Header data provider. When headers are removed - automatically called with default values.</param>
    /// <param name="Supplier">Supplies things like <see cref="Header"/> with data, required to wipe the header data.</param>
    internal readonly record struct CustomHeaderDescriptor(
        CustomHeaderParameterProvider Provider,
        CustomHeaderRegionSupplier Supplier);
    /// <summary>
    /// Provides data, essential for <see cref="CustomHeader{T}"/> initialization.
    /// </summary>
    internal delegate void CustomHeaderParameterProvider(int order, int contentPosition);
    /// <summary>
    /// Supplies others with data about the content region in which an <see cref="CustomHeader{T}"/> is located.
    /// </summary>
    /// <param name="contentPosition">Position of the header in a byte-based content region.</param>
    /// <param name="sizeInBytes">Size of the header in bytes.</param>
    internal delegate void CustomHeaderRegionSupplier(out int contentPosition, out int sizeInBytes);
}
