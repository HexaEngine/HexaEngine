namespace HexaEngine.Core.Unsafes
{
    /// <summary>
    /// Represents an iterator over a list of values.
    /// </summary>
    /// <typeparam name="T">The type of values in the list.</typeparam>
    public struct Iterator<T> where T : struct
    {
        private readonly List<T> values;
        private int index;

        /// <summary>
        /// Initializes a new instance of the <see cref="Iterator{T}"/> struct.
        /// </summary>
        /// <param name="values">The list of values to iterate over.</param>
        public Iterator(List<T> values)
        {
            this.values = values;
            index = 0;
        }

        /// <summary>
        /// Gets the current _value in the iteration.
        /// </summary>
        public readonly T Current => values[index];

        /// <summary>
        /// Gets a _value indicating whether the iteration has reached the end.
        /// </summary>
        public readonly bool End => index == values.Count;

        /// <summary>
        /// Increments the iterator to the next _value in the iteration.
        /// </summary>
        /// <param name="iterator">The iterator to increment.</param>
        /// <returns>The incremented iterator.</returns>
        public static Iterator<T> operator ++(Iterator<T> iterator)
        {
            iterator.index++;
            return iterator;
        }

        /// <summary>
        /// Decrements the iterator to the previous _value in the iteration.
        /// </summary>
        /// <param name="iterator">The iterator to decrement.</param>
        /// <returns>The decremented iterator.</returns>
        public static Iterator<T> operator --(Iterator<T> iterator)
        {
            iterator.index--;
            return iterator;
        }

        /// <summary>
        /// Advances the iterator by the specified _value.
        /// </summary>
        /// <param name="iterator">The iterator to advance.</param>
        /// <param name="value">The _value by which to advance the iterator.</param>
        /// <returns>The advanced iterator.</returns>
        public static Iterator<T> operator +(Iterator<T> iterator, int value)
        {
            iterator.index += value;
            return iterator;
        }

        /// <summary>
        /// Moves the iterator back by the specified _value.
        /// </summary>
        /// <param name="iterator">The iterator to move back.</param>
        /// <param name="value">The _value by which to move the iterator back.</param>
        /// <returns>The moved iterator.</returns>
        public static Iterator<T> operator -(Iterator<T> iterator, int value)
        {
            iterator.index -= value;
            return iterator;
        }

        /// <summary>
        /// Advances the iterator by the specified _value.
        /// </summary>
        /// <param name="iterator">The iterator to advance.</param>
        /// <param name="value">The _value by which to advance the iterator.</param>
        /// <returns>The advanced iterator.</returns>
        public static Iterator<T> operator +(Iterator<T> iterator, uint value)
        {
            iterator.index += (int)value;
            return iterator;
        }

        /// <summary>
        /// Moves the iterator back by the specified _value.
        /// </summary>
        /// <param name="iterator">The iterator to move back.</param>
        /// <param name="value">The _value by which to move the iterator back.</param>
        /// <returns>The moved iterator.</returns>
        public static Iterator<T> operator -(Iterator<T> iterator, uint value)
        {
            iterator.index -= (int)value;
            return iterator;
        }
    }
}