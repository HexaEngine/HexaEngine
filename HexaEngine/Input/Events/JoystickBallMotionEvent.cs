namespace HexaEngine.Input
{
    using HexaEngine.Core.Input.Events;

    public struct JoystickBallMotionEvent(JoystickBallMotionEventArgs eventArgs)
    {
        public int Ball = eventArgs.Ball;
        public int RelX = eventArgs.RelX;
        public int RelY = eventArgs.RelY;

        public float GetAxis(int axis)
        {
            if (axis == 0)
            {
                return RelX;
            }
            else if (axis == 1)
            {
                return RelY;
            }
            else
            {
                return 0;
            }
        }
    }
}