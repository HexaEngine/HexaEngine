namespace HexaEngine.Input
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;

    public struct GamepadTouchPadEvent(GamepadTouchpadEventArgs eventArgs)
    {
        public int Touchpad = eventArgs.TouchpadId;
        public int Finger = eventArgs.Finger;
        public FingerState State = eventArgs.State;
        public float X = eventArgs.X;
        public float Y = eventArgs.Y;
        public float Pressure = eventArgs.Pressure;
    }
}