namespace HexaEngine.Input
{
    using HexaEngine.Core.Input.Events;

    public struct TouchDeviceTouchMotionEvent(TouchMotionEventArgs eventArgs)
    {
        public long Finger = eventArgs.FingerId;
        public float X = eventArgs.X;
        public float Y = eventArgs.Y;
        public float Dx = eventArgs.Dx;
        public float Dy = eventArgs.Dy;
        public float Pressure = eventArgs.Pressure;

        public float GetAxis(int axis)
        {
            if (axis == 0)
            {
                return Dx;
            }
            else if (axis == 1)
            {
                return Dy;
            }
            else if (axis == 2)
            {
                return Pressure;
            }
            return 0;
        }
    }
}