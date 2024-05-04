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

        public readonly float GetAxis(int index)
        {
            return index switch
            {
                0 => X,
                1 => Y,
                2 => Pressure,
                _ => 0,
            };
        }

        public readonly float GetAxis(Axis index)
        {
            return index switch
            {
                Axis.X => X,
                Axis.Y => Y,
                _ => Pressure,
            };
        }
    }
}