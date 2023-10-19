namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Input.Events;

    public unsafe class Finger
    {
        private readonly Silk.NET.SDL.Finger* finger;
        private readonly long id;
        private FingerState state;

        public Finger(Silk.NET.SDL.Finger* finger)
        {
            this.finger = finger;
            id = finger->Id;
            state = Pressure > 0 ? FingerState.Down : FingerState.Up;
        }

        public long Id => id;

        public FingerState State => state;

        public float X => finger->X;

        public float Y => finger->Y;

        public float Pressure => finger->Pressure;

        public event EventHandler<TouchEventArgs>? TouchUp;

        public event EventHandler<TouchEventArgs>? TouchDown;

        public event EventHandler<TouchMotionEventArgs>? TouchMotion;

        internal void OnFingerUp(TouchEventArgs touchEventArgs)
        {
            state = FingerState.Up;
            TouchUp?.Invoke(this, touchEventArgs);
        }

        internal void OnFingerDown(TouchEventArgs touchEventArgs)
        {
            state = FingerState.Down;
            TouchDown?.Invoke(this, touchEventArgs);
        }

        internal void OnFingerMotion(TouchMotionEventArgs touchMotionEventArgs)
        {
            TouchMotion?.Invoke(this, touchMotionEventArgs);
        }
    }
}