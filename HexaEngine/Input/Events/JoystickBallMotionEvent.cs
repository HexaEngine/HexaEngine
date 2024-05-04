namespace HexaEngine.Input.Events
{
    using HexaEngine.Core.Input.Events;

    public struct JoystickBallMotionEvent(JoystickBallMotionEventArgs eventArgs)
    {
        public int Ball = eventArgs.Ball;
        public int RelX = eventArgs.RelX;
        public int RelY = eventArgs.RelY;

        public readonly float GetAxis(int axis)
        {
            return axis switch
            {
                0 => RelX,
                1 => RelY,
                _ => 0
            };
        }

        public readonly float GetAxis(Axis axis)
        {
            return axis switch
            {
                Axis.X => RelX,
                Axis.Y => RelY,
                _ => 0
            };
        }
    }
}