namespace HexaEngine.Input.Events
{
    using HexaEngine.Core.Input.Events;

    public struct GamepadTouchPadMotionEvent(GamepadTouchpadMotionEventArgs eventArgs)
    {
        public int Touchpad = eventArgs.TouchpadId;
        public int Finger = eventArgs.Finger;
        public float X = eventArgs.X;
        public float Y = eventArgs.Y;
        public float Pressure = eventArgs.Pressure;

        public float GetAxis(int index)
        {
            if (index == 0)
            {
                return X;
            }
            else if (index == 1)
            {
                return Y;
            }
            else if (index == 2)
            {
                return Pressure;
            }
            return 0;
        }
    }
}