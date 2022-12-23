namespace HexaEngine.Core.Input.Events
{
    using Silk.NET.SDL;

    public class MouseWheelEventArgs : EventArgs
    {
        public int X { get; internal set; }

        public int Y { get; internal set; }

        public MouseWheelDirection Direction { get; internal set; }
    }
}