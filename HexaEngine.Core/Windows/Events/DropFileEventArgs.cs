namespace HexaEngine.Core.Windows.Events
{
    public unsafe class DropFileEventArgs : DropEventArgs
    {
        /// <summary>
        /// Gets the file path of the dropped file.
        /// </summary>
        public byte* File { get; set; }

        /// <summary>
        /// Gets the mouse X position of the drop event.
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Gets the mouse Y position of the drop event.
        /// </summary>
        public float Y { get; set; }

        public string? GetString()
        {
            return ToStringFromUTF8(File);
        }
    }
}