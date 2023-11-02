namespace HexaEngine.Core.Windows.Events
{
    public class TimestampEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the timestamp of an event.
        /// </summary>
        public uint Timestamp { get; internal set; }
    }
}