namespace HexaEngine.Input.Events
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;

    public struct MouseWheelEvent
    {
        public float X;
        public float Y;
        public MouseWheelDirection Direction;

        public MouseWheelEvent(MouseWheelEventArgs eventArgs)
        {
            X = eventArgs.Wheel.X;
            Y = eventArgs.Wheel.Y;
            Direction = eventArgs.Direction;
        }

        public readonly float GetAxis(int index)
        {
            return index switch
            {
                0 => X,
                1 => Y,
                _ => 0,
            };
        }

        public readonly float GetAxis(Axis axis)
        {
            return axis switch
            {
                Axis.X => X,
                Axis.Y => Y,
                _ => 0,
            };
        }
    }
}