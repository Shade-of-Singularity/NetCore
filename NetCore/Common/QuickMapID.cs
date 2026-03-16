namespace NetCore.Common
{
    /// <summary>
    /// Holds <see cref="QuickMapIndex"/>es for all items that can be stored in a <see cref="QuickMap{T}"/>.
    /// </summary>
    /// <typeparam name="TItem">Item inheriting/implementing <typeparamref name="TCategory"/>.</typeparam>
    /// <typeparam name="TCategory">Base class/interface for an <typeparamref name="TItem"/>.</typeparam>
    public static class QuickMapID<TItem, TCategory> where TItem : class, TCategory
    {
        /// <summary>
        /// Cached index for an item in a current category.
        /// </summary>
        public static readonly QuickMapIndex Index = QuickIDCategory<TCategory>.GetNext();
        /// <inheritdoc cref="QuickMapIndex.Mask"/>
        public static readonly QuickMapIndexMask Mask = Index.Mask;
        /// <inheritdoc cref="QuickMapIndex.Position"/>
        public static readonly QuickMapIndexPosition Position = Index.Position;
        /// <inheritdoc cref="QuickMapIndex.BitFlag"/>
        public static readonly uint BitFlag = Index.BitFlag;
    }

    /// <summary>
    /// Holds information needed to index items in a specific <typeparamref name="TCategory"/>.
    /// </summary>
    /// <typeparam name="TCategory">Base type for items in an collection.</typeparam>
    public static class QuickIDCategory<TCategory>
    {
        static ushort inUse;

        /// <summary>
        /// Retrieves next available <see cref="QuickMapIndex"/> for this category.
        /// </summary>
        /// <returns></returns>
        public static QuickMapIndex GetNext() => QuickMapIndexing.GetNextIndex(ref inUse);
    }
}
