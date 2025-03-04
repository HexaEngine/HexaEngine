namespace HexaEngine.Core.Windows.Events
{
    public unsafe class DropTextEventArgs : DropEventArgs
    {
        /// <summary>
        /// Gets the text of the dropped text.
        /// </summary>
        public byte* Text { get; set; }

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
            return ToStringFromUTF8(Text);
        }
    }
}