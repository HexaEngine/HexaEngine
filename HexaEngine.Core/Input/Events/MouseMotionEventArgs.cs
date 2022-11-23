namespace HexaEngine.Core.Input.Events
{
    public class MouseMotionEventArgs : EventArgs
    {
        public MouseMotionEventArgs()
        {
        }

        public MouseMotionEventArgs(float x, float y, float relX, float relY)
        {
            X = x;
            Y = y;
            RelX = relX;
            RelY = relY;
        }

        public float X;
        public float Y;
        public float RelX;
        public float RelY;
    }
}