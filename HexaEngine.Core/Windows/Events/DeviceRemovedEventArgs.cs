namespace HexaEngine.Core.Windows.Events
{
    /// <summary>
    /// Event arguments for the device removed event.
    /// </summary>
    public class DeviceRemovedEventArgs : EventArgs
    {
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

        public override string ToString()
        {
            return Message;
        }
    }
}