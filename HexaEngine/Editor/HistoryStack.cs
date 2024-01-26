namespace HexaEngine.Editor
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a stack that keeps a history of items.
    /// </summary>
    /// <typeparam name="T">The type of elements in the stack.</typeparam>
    public class HistoryStack<T> : List<T>
    {
        /// <summary>
        /// Adds an item to the stack.
        /// </summary>
        /// <param name="item">The item to push onto the stack.</param>
        public void Push(T item)
        {
            Add(item);
        }

        /// <summary>
        /// Removes and returns the top item from the stack.
        /// </summary>
        /// <returns>The top item from the stack.</returns>
        public T Pop()
        {
            var lastIndex = Count - 1;
            T item = this[lastIndex];
            RemoveAt(lastIndex);
            return item;
        }

        /// <summary>
        /// Tries to remove and return the top item from the stack.
        /// </summary>
        /// <param name="item">When this method returns, contains the top item from the stack, if the stack is not empty; otherwise, the default value for the type of the item parameter.</param>
        /// <returns><c>true</c> if an item was removed and returned successfully; otherwise, <c>false</c>.</returns>
        public bool TryPop([MaybeNullWhen(false)] out T? item)
        {
            var lastIndex = Count - 1;
            if (lastIndex < 0)
            {
                item = default;
                return false;
            }

            item = this[lastIndex];
            RemoveAt(lastIndex);
            return true;
        }
    }
}