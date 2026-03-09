namespace NetCore.Common
{
    /// <summary>
    /// Holds <see cref="QuickIndex"/>es for all items that can be stored in a <see cref="QuickMap{T}"/>.
    /// </summary>
    /// <typeparam name="TItem">Item inheriting/implementing <typeparamref name="TCategory"/>.</typeparam>
    /// <typeparam name="TCategory">Base class/interface for an <typeparamref name="TItem"/>.</typeparam>
    public static class QuickID<TItem, TCategory> where TItem : class, TCategory
    {
        /// <summary>
        /// Cached index for an item in a current category.
        /// </summary>
        public static readonly QuickIndex Index = QuickIDCategory<TCategory>.GetNext();
    }

    /// <summary>
    /// Holds information needed to index items in a specific <typeparamref name="TCategory"/>.
    /// </summary>
    /// <typeparam name="TCategory">Base type for items in an collection.</typeparam>
    public static class QuickIDCategory<TCategory>
    {
        static int inUse;

        /// <summary>
        /// Retrieves next available <see cref="QuickIndex"/> for this category.
        /// </summary>
        /// <returns></returns>
        public static QuickIndex GetNext() => QuickIndexing.GetNextIndex(ref inUse);
    }
}
