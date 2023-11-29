namespace HexaEngine.Input.Events
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;

    public struct TouchDeviceTouchEvent(TouchEventArgs eventArgs)
    {
        public long Finger = eventArgs.FingerId;
        public FingerState State = eventArgs.State;
        public float X = eventArgs.X;
        public float Y = eventArgs.Y;
        public float Pressure = eventArgs.Pressure;
    }
}