namespace HexaEngine.Input.Events
{
    using HexaEngine.Core.Input.Events;

    public struct TouchDeviceTouchMotionEvent(TouchMoveEventArgs eventArgs)
    {
        public long Finger = eventArgs.FingerId;
        public float X = eventArgs.X;
        public float Y = eventArgs.Y;
        public float Dx = eventArgs.Dx;
        public float Dy = eventArgs.Dy;
        public float Pressure = eventArgs.Pressure;

        public readonly float GetAxis(int axis)
        {
            return axis switch
            {
                0 => Dx,
                1 => Dy,
                2 => Pressure,
                _ => 0,
            };
        }

        public readonly float GetAxis(Axis axis)
        {
            return axis switch
            {
                Axis.X => Dx,
                Axis.Y => Dy,
                _ => Pressure,
            };
        }
    }
}