using System;
using System.Threading;

namespace NetCore
{
    /// <summary>
    /// Source for CRTP IDs based on an base class <typeparamref name="TBase"/>.
    /// </summary>
    /// <typeparam name="TBase">Base class of an type.</typeparam>
    public static class CRTPIDSource<TBase>
    {
        /// <summary>
        /// Amount of already registered items under a given <typeparamref name="TBase"/>.
        /// </summary>
        public static int Amount => m_NextOrder;
        /// <summary>
        /// Internal increment for <see cref="NextOrder"/>.
        /// </summary>
        static int m_NextOrder;
        /// <summary>
        /// Evaluates initialization order for a new item, to use for CRTP identification.
        /// </summary>
        /// <remarks>
        /// Limited to <see cref="int.MaxValue"/>.
        /// </remarks>
        /// <returns>New order to use.</returns>
        public static int NextOrder()
        {
            int order = Interlocked.Increment(ref m_NextOrder);
            if (order < 0) throw new NotSupportedException($"Container system does not support more than {int.MaxValue} amount of containers.");

            return order;
        }
    }
}
