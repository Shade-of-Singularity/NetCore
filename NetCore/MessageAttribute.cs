using System;
using System.Reflection;

namespace NetCore
{
    /// <summary>
    /// Thrown when you provide a group type for <see cref="MessageAttribute"/>, which does not inhertit <see cref="MessageGroup"/> class.
    /// </summary>
    /// <param name="type">Provided type.</param>
    public sealed class InvalidGroupTypeException(Type type)
        : Exception($"Group type ({type.FullName}) provided to a {nameof(MessageAttribute)} does not inherit ({typeof(MessageGroup).FullName}) class.");

    /// <summary>
    /// Attribute for static methods, handling messages.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class MessageAttribute : Attribute
    {
        /// <summary>
        /// <see cref="BindingFlags"/> used for looking up for the methods.
        /// </summary>
        public const BindingFlags MethodBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        /// <summary>
        /// Group type to which message handler belongs.
        /// </summary>
        public readonly Type Group;
        /// <summary>
        /// Creates attribute and assigns it to a default group.
        /// </summary>
        public MessageAttribute() : this(typeof(GlobalMessageGroup)) { }
        /// <summary>
        /// Creates attribute and assigns it to a provided <paramref name="group"/>.
        /// </summary>
        /// <param name="group">Group type to assign a message handler to.</param>
        public MessageAttribute(Type group)
        {
            if (!typeof(MessageGroup).IsAssignableFrom(group))
            {
                throw new InvalidGroupTypeException(group);
            }

            Group = group;
        }
    }
}
