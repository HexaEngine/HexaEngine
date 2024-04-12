namespace HexaEngine.Input.Events
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;

    public struct MouseWheelEvent(MouseWheelEventArgs eventArgs)
    {
        public float X = eventArgs.Wheel.X;
        public float Y = eventArgs.Wheel.Y;
        public MouseWheelDirection Direction = eventArgs.Direction;

        public float GetAxis(int index)
        {
            if (index == 0)
            {
                return X;
            }

            if (index == 1)
            {
                return Y;
            }

            return 0;
        }
    }
}