using System;

namespace NetCore
{
    /// <summary>
    /// Indicates that header was not found in a target header holder.
    /// </summary>
    /// <param name="type">Type of the header.</param>
    public sealed class HeaderNotFoundException(Type type)
        : Exception($"{typeof(HeaderNotFoundException).FullName}: Header of a type ({type.FullName}) is not found.")
    { }
}
