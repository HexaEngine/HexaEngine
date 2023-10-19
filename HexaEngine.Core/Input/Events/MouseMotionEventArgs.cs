namespace HexaEngine.Core.Input.Events
{
    public class MouseMotionEventArgs : MouseEventArgs
    {
        public MouseMotionEventArgs()
        {
        }

        public float X { get; internal set; }

        public float Y { get; internal set; }

        public float RelX { get; internal set; }

        public float RelY { get; internal set; }
    }
}