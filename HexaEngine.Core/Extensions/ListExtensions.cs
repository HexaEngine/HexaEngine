namespace HexaEngine.Core.Extensions
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides extension methods for <see cref="List{T}"/> and accessing the internal array.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// [Obsolete] Use a non hacky way of accessing the local array of a <see cref="List{T}"/>.
        /// This method will be removed in the next version.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list for which to access the internal array.</param>
        /// <returns>The internal array of the list.</returns>
        [Obsolete("Use a non hacky way of accessing the local array of a List<T>, will be removed next version")]
        public static T[] GetInternalArray<T>(this List<T> list)
        {
            return ArrayAccessor<T>.Getter(list);
        }
    }
}