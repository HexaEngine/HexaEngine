namespace HexaEngine.Core.Input.Events
{
    public class MouseMotionEventArgs : EventArgs
    {
        public MouseMotionEventArgs(int x, int y, int relX, int relY)
        {
            X = x;
            Y = y;
            RelX = relX;
            RelY = relY;
        }

        public int X;
        public int Y;
        public int RelX;
        public int RelY;
    }
}