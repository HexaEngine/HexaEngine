﻿namespace HexaEngine.Core.Windows.Events
{
    public class DropEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets the timestamp of an event.
        /// </summary>
        public uint Timestamp { get; set; }
    }
}