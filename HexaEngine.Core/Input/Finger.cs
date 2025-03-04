namespace HexaEngine.Core.Input
{
    using Hexa.NET.SDL3;
    using HexaEngine.Core.Input.Events;

    /// <summary>
    /// Represents a finger in an input system.
    /// </summary>
    public unsafe class Finger
    {
        private readonly SDLFinger* finger;
        private readonly long id;
        private FingerState state;

        /// <summary>
        /// Initializes a new instance of the <see cref="Finger"/> class.
        /// </summary>
        /// <param name="finger">A pointer to the native SDL finger.</param>
        public Finger(SDLFinger* finger)
        {
            this.finger = finger;
            id = finger->Id;
            state = Pressure > 0 ? FingerState.Down : FingerState.Up;
        }

        /// <summary>
        /// Gets the unique identifier for the finger.
        /// </summary>
        public long Id => id;

        /// <summary>
        /// Gets the current state of the finger.
        /// </summary>
        public FingerState State => state;

        /// <summary>
        /// Gets the X-coordinate of the finger's position.
        /// </summary>
        public float X => finger->X;

        /// <summary>
        /// Gets the Y-coordinate of the finger's position.
        /// </summary>
        public float Y => finger->Y;

        /// <summary>
        /// Gets the pressure applied by the finger.
        /// </summary>
        public float Pressure => finger->Pressure;

        /// <summary>
        /// Occurs when the finger is released (up).
        /// </summary>
        public event EventHandler<TouchEventArgs>? TouchUp;

        /// <summary>
        /// Occurs when the finger is pressed (down).
        /// </summary>
        public event EventHandler<TouchEventArgs>? TouchDown;

        /// <summary>
        /// Occurs when the finger's position is updated.
        /// </summary>
        public event EventHandler<TouchMoveEventArgs>? TouchMotion;

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

        internal void OnFingerMotion(TouchMoveEventArgs touchMotionEventArgs)
        {
            TouchMotion?.Invoke(this, touchMotionEventArgs);
        }
    }
}