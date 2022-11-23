namespace HexaEngine.Core.Input2.Events
{
    public class MouseMotionEventArgs : EventArgs
    {
        public MouseMotionEventArgs()
        {
        }

        public float X;
        public float Y;
        public float RelX;
        public float RelY;
    }
}