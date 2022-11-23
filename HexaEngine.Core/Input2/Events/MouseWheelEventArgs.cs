namespace HexaEngine.Core.Input2.Events
{
    using Silk.NET.SDL;

    public class MouseWheelEventArgs : EventArgs
    {
        public MouseWheelEventArgs(int x, int y, MouseWheelDirection direction)
        {
            X = x;
            Y = y;
            Direction = direction;
        }

        public int X { get; }

        public int Y { get; }

        public MouseWheelDirection Direction { get; }
    }
}