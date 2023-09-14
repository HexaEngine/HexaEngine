namespace HexaEngine.Core.Windows.Events
{
    public class DeviceRemovedEventArgs : EventArgs
    {
        public DeviceRemovedEventArgs(string message, int code)
        {
            Message = message;
            Code = code;
        }

        public string Message { get; set; }

        public int Code { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}