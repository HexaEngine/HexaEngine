namespace HexaEngine.Core.Windows.Events
{
    /// <summary>
    /// Event arguments for the device removed event.
    /// </summary>
    public class DeviceRemovedEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new instance of <see cref="DeviceRemovedEventArgs"/>.
        /// </summary>
        /// <param name="message">The message for the device removal.</param>
        /// <param name="code">The api native result code for the event.</param>
        public DeviceRemovedEventArgs(string message, int code)
        {
            Message = message;
            Code = code;
        }

        /// <summary>
        /// Gets the message of the reason device removed event.
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Gets the event code of the device removed event.
        /// </summary>
        public int Code { get; internal set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Message;
        }
    }
}