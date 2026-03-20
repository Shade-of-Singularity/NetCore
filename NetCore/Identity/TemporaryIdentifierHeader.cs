namespace NetCore.Identity
{
    /// <summary>
    /// Header for <see cref="IReadOnlyConnectionArgs.TemporaryIdentifier"/>.
    /// </summary>
    public sealed class TemporaryIdentifierHeader : CustomHeader<TemporaryIdentifierHeader>
    {
        /// <inheritdoc/>
        public override int Size => TemporaryIdentifier.SizeInBits;
    }
}
