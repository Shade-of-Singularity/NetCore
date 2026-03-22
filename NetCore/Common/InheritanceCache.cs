namespace NetCore.Common
{
    /// <summary>
    /// Checks if <typeparamref name="TTarget"/> implements/inherits <typeparamref name="TBase"/>
    /// and caches the result to be reused again.
    /// </summary>
    /// <typeparam name="TTarget">Target type to check for.</typeparam>
    /// <typeparam name="TBase">Target base type to seek in a <typeparamref name="TTarget"/> type.</typeparam>
    internal static class Inheritance<TTarget, TBase>
    {
        /// <summary>
        /// Whether <typeparamref name="TTarget"/> implements/inherits <typeparamref name="TBase"/>
        /// </summary>
        public static readonly bool Inherits = typeof(TBase).IsAssignableFrom(typeof(TTarget));
    }
}
