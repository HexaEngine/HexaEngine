namespace HexaEngine.Core.Input
{
    using Hexa.NET.SDL2;
    using HexaEngine.Core.Input.Events;

    /// <summary>
    /// Represents a delegate for handling events related to a gamepad touchpad.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of event arguments associated with the event.</typeparam>
    /// <param name="sender">The sender of the event, which is the <see cref="GamepadTouchpad"/> triggering the event.</param>
    /// <param name="e">The event arguments containing information about the event.</param>
    public delegate void GamepadTouchpadEventHandler<TEventArgs>(GamepadTouchpad sender, TEventArgs e);

    /// <summary>
    /// Represents a touchpad on a gamepad controller.
    /// </summary>
    public unsafe class GamepadTouchpad
    {
        private readonly int id;
        private readonly SDLGameController* controller;
        private readonly GamepadTouchpadFinger[] fingerStates;

        private readonly GamepadTouchpadEventArgs touchpadEventArgs = new();
        private readonly GamepadTouchpadMotionEventArgs motionEventArgs = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadTouchpad"/> class.
        /// </summary>
        /// <param name="id">The ID of the touchpad.</param>
        /// <param name="controller">The game controller associated with the touchpad.</param>
        public GamepadTouchpad(int id, SDLGameController* controller)
        {
            this.id = id;
            this.controller = controller;
            var fingerCount = SDL.SDLGameControllerGetNumTouchpadFingers(controller, id);
            fingerStates = new GamepadTouchpadFinger[fingerCount];
            for (int i = 0; i < fingerCount; i++)
            {
                byte state;
                float x;
                float y;
                float pressure;
                SDL.SDLGameControllerGetTouchpadFinger(controller, id, i, &state, &x, &y, &pressure);
                fingerStates[i] = new(state == SDL.SDL_PRESSED ? FingerState.Down : FingerState.Up, x, y, pressure);
            }
        }

        /// <summary>
        /// Gets the ID of the touchpad.
        /// </summary>
        public int Id => id;

        /// <summary>
        /// Gets the number of fingers on the touchpad.
        /// </summary>
        public int FingerCount => fingerStates.Length;

        /// <summary>
        /// Occurs when a finger touches down on the touchpad.
        /// </summary>
        public event GamepadTouchpadEventHandler<GamepadTouchpadEventArgs>? TouchPadDown;

        /// <summary>
        /// Occurs when a finger moves on the touchpad.
        /// </summary>
        public event GamepadTouchpadEventHandler<GamepadTouchpadMotionEventArgs>? TouchPadMotion;

        /// <summary>
        /// Occurs when a finger lifts up from the touchpad.
        /// </summary>
        public event GamepadTouchpadEventHandler<GamepadTouchpadEventArgs>? TouchPadUp;

        internal (GamepadTouchpad Touchpad, GamepadTouchpadEventArgs EventArgs) OnTouchPadDown(SDLControllerTouchpadEvent even)
        {
            var state = fingerStates[even.Finger];
            state.State = FingerState.Down;
            state.X = even.X;
            state.Y = even.Y;
            state.Pressure = even.Pressure;
            fingerStates[even.Finger] = state;

            touchpadEventArgs.Timestamp = even.Timestamp;
            touchpadEventArgs.Handled = false;
            touchpadEventArgs.GamepadId = even.Which;
            touchpadEventArgs.TouchpadId = even.Touchpad;
            touchpadEventArgs.State = FingerState.Down;
            touchpadEventArgs.Finger = even.Finger;
            touchpadEventArgs.X = even.X;
            touchpadEventArgs.Y = even.Y;
            touchpadEventArgs.Pressure = even.Pressure;
            TouchPadDown?.Invoke(this, touchpadEventArgs);
            return (this, touchpadEventArgs);
        }

        internal (GamepadTouchpad Touchpad, GamepadTouchpadMotionEventArgs EventArgs) OnTouchPadMotion(SDLControllerTouchpadEvent even)
        {
            var state = fingerStates[even.Finger];
            state.X = even.X;
            state.Y = even.Y;
            state.Pressure = even.Pressure;
            fingerStates[even.Finger] = state;

            motionEventArgs.Timestamp = even.Timestamp;
            motionEventArgs.Handled = false;
            motionEventArgs.GamepadId = even.Which;
            motionEventArgs.TouchpadId = even.Touchpad;
            motionEventArgs.Finger = even.Finger;
            motionEventArgs.X = even.X;
            motionEventArgs.Y = even.Y;
            motionEventArgs.Pressure = even.Pressure;
            TouchPadMotion?.Invoke(this, motionEventArgs);
            return (this, motionEventArgs);
        }

        internal (GamepadTouchpad Touchpad, GamepadTouchpadEventArgs EventArgs) OnTouchPadUp(SDLControllerTouchpadEvent even)
        {
            var state = fingerStates[even.Finger];
            state.State = FingerState.Up;
            state.X = even.X;
            state.Y = even.Y;
            state.Pressure = even.Pressure;
            fingerStates[even.Finger] = state;

            touchpadEventArgs.Timestamp = even.Timestamp;
            touchpadEventArgs.Handled = false;
            touchpadEventArgs.GamepadId = even.Which;
            touchpadEventArgs.TouchpadId = even.Touchpad;
            touchpadEventArgs.State = FingerState.Up;
            touchpadEventArgs.Finger = even.Finger;
            touchpadEventArgs.X = even.X;
            touchpadEventArgs.Y = even.Y;
            touchpadEventArgs.Pressure = even.Pressure;
            TouchPadUp?.Invoke(this, touchpadEventArgs);
            return (this, touchpadEventArgs);
        }

        /// <summary>
        /// Determines whether a finger on the touchpad is currently down.
        /// </summary>
        /// <param name="finger">The index of the finger on the touchpad.</param>
        /// <returns><c>true</c> if the finger is currently down; otherwise, <c>false</c>.</returns>
        public bool IsDown(int finger)
        {
            return fingerStates[finger].State == FingerState.Down;
        }

        /// <summary>
        /// Determines whether a finger on the touchpad is currently up.
        /// </summary>
        /// <param name="finger">The index of the finger on the touchpad.</param>
        /// <returns><c>true</c> if the finger is currently up; otherwise, <c>false</c>.</returns>
        public bool IsUp(int finger)
        {
            return fingerStates[finger].State == FingerState.Up;
        }

        /// <summary>
        /// Gets the state of the finger at the specified index.
        /// </summary>
        /// <param name="index">The index of the finger on the touchpad.</param>
        /// <returns>The state of the finger at the specified index.</returns>
        public GamepadTouchpadFinger GetFinger(int index)
        {
            return fingerStates[index];
        }

        /// <summary>
        /// Flushes the state of the touchpad, updating the finger states.
        /// </summary>
        public void Flush()
        {
            for (int i = 0; i < fingerStates.Length; i++)
            {
                byte state;
                float x;
                float y;
                float pressure;
                SDL.SDLGameControllerGetTouchpadFinger(controller, id, i, &state, &x, &y, &pressure);
                fingerStates[i] = new(state == SDL.SDL_PRESSED ? FingerState.Down : FingerState.Up, x, y, pressure);
            }
        }
    }
}