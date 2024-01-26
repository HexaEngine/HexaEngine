namespace HexaEngine.Editor
{
    /// <summary>
    /// Represents a context for history actions with specific typed parameters.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    public struct HistoryContext<T1, T2> : IHistoryContext<T1, T2>
    {
        /// <summary>
        /// Gets or sets the target object associated with the history action.
        /// </summary>
        public T1 Target;

        /// <summary>
        /// Gets or sets the old value associated with the history action.
        /// </summary>
        public T2 OldValue;

        /// <summary>
        /// Gets or sets the new value associated with the history action.
        /// </summary>
        public T2 NewValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryContext{T1, T2}"/> struct.
        /// </summary>
        /// <param name="actionName">The action name associated with the history action.</param>
        /// <param name="target">The target object associated with the history action.</param>
        /// <param name="oldValue">The old value associated with the history action.</param>
        /// <param name="newValue">The new value associated with the history action.</param>
        public HistoryContext(T1 target, T2 oldValue, T2 newValue)
        {
            Target = target;
            OldValue = oldValue;
            NewValue = newValue;
        }

        T1 IHistoryContext<T1, T2>.Target { get => Target; set => Target = value; }

        T2 IHistoryContext<T1, T2>.OldValue { get => OldValue; set => OldValue = value; }

        T2 IHistoryContext<T1, T2>.NewValue { get => NewValue; set => NewValue = value; }
    }
}