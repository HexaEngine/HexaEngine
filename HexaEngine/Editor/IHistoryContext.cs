namespace HexaEngine.Editor
{
    /// <summary>
    /// Represents a context for history actions with specific typed parameters.
    /// </summary>
    /// <typeparam name="T1">The type of the first parameter.</typeparam>
    /// <typeparam name="T2">The type of the second parameter.</typeparam>
    public interface IHistoryContext<T1, T2>
    {
        /// <summary>
        /// Gets or sets the target object associated with the history action.
        /// </summary>
        T1 Target { get; set; }

        /// <summary>
        /// Gets or sets the old value associated with the history action.
        /// </summary>
        T2 OldValue { get; set; }

        /// <summary>
        /// Gets or sets the new value associated with the history action.
        /// </summary>
        T2 NewValue { get; set; }
    }
}