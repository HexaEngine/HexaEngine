namespace HexaEngine.Core.Windows.Events
{
    /// <summary>
    /// An event with a Timestamp.
    /// </summary>
    /// <seealso cref="System.EventArgs" />
    public class TimestampEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the timestamp of an event.
        /// </summary>
        public uint Timestamp { get; internal set; }
    }
}