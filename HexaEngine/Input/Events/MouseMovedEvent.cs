namespace HexaEngine.Input.Events
{
    using HexaEngine.Core.Input.Events;

    public struct MouseMovedEvent(MouseMoveEventArgs eventArgs)
    {
        public float X = eventArgs.X;
        public float Y = eventArgs.Y;
        public float RelX = eventArgs.RelX;
        public float RelY = eventArgs.RelY;

        public readonly float GetAxis(int axis)
        {
            return axis switch
            {
                0 => RelX,
                1 => RelY,
                _ => 0,
            };
        }

        public readonly float GetAxis(Axis axis)
        {
            return axis switch
            {
                Axis.X => RelX,
                Axis.Y => RelY,
                _ => 0,
            };
        }
    }
}