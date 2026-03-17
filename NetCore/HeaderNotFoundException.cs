using System;

namespace NetCore
{
    /// <summary>
    /// Indicates that header was not found in a target header holder.
    /// </summary>
    /// <param name="type">Type of the header.</param>
    public sealed class HeaderNotFoundException(Type type) : Exception
    {
        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{typeof(HeaderNotFoundException).FullName}: HeaderHelpers of a type ({type.FullName}) is not found.\n{StackTrace}";
        }
    }
}
