namespace HexaEngine.Core.Input
{
    public struct GamepadTouchpadFinger
    {
        public FingerState State;
        public float X;
        public float Y;
        public float Pressure;

        public GamepadTouchpadFinger(FingerState state, float x, float y, float pressure)
        {
            State = state;
            X = x;
            Y = y;
            Pressure = pressure;
        }
    }
}