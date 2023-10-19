namespace HexaEngine.Core.Input
{
    using HexaEngine.Core.Input.Events;
    using Silk.NET.SDL;

    public unsafe class GamepadTouchpad
    {
        private static readonly Sdl sdl = Application.sdl;
        private readonly int id;
        private readonly GameController* controller;
        private readonly GamepadTouchpadFinger[] fingerStates;

        private readonly GamepadTouchpadEventArgs touchpadEventArgs = new();
        private readonly GamepadTouchpadMotionEventArgs motionEventArgs = new();

        public GamepadTouchpad(int id, GameController* controller)
        {
            this.id = id;
            this.controller = controller;
            var fingerCount = sdl.GameControllerGetNumTouchpadFingers(controller, id);
            fingerStates = new GamepadTouchpadFinger[fingerCount];
            for (int i = 0; i < fingerCount; i++)
            {
                byte state;
                float x;
                float y;
                float pressure;
                sdl.GameControllerGetTouchpadFinger(controller, id, i, &state, &x, &y, &pressure);
                fingerStates[i] = new(state == Sdl.Pressed ? FingerState.Down : FingerState.Up, x, y, pressure);
            }
        }

        public int Id => id;

        public int FingerCount => fingerStates.Length;

        public event EventHandler<GamepadTouchpadEventArgs>? TouchPadDown;

        public event EventHandler<GamepadTouchpadMotionEventArgs>? TouchPadMotion;

        public event EventHandler<GamepadTouchpadEventArgs>? TouchPadUp;

        internal (GamepadTouchpad Touchpad, GamepadTouchpadEventArgs EventArgs) OnTouchPadDown(ControllerTouchpadEvent even)
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

        internal (GamepadTouchpad Touchpad, GamepadTouchpadMotionEventArgs EventArgs) OnTouchPadMotion(ControllerTouchpadEvent even)
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

        internal (GamepadTouchpad Touchpad, GamepadTouchpadEventArgs EventArgs) OnTouchPadUp(ControllerTouchpadEvent even)
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

        public bool IsDown(int finger)
        {
            return fingerStates[finger].State == FingerState.Down;
        }

        public bool IsUp(int finger)
        {
            return fingerStates[finger].State == FingerState.Up;
        }

        public GamepadTouchpadFinger GetFinger(int index)
        {
            return fingerStates[index];
        }

        public void Flush()
        {
            for (int i = 0; i < fingerStates.Length; i++)
            {
                byte state;
                float x;
                float y;
                float pressure;
                sdl.GameControllerGetTouchpadFinger(controller, id, i, &state, &x, &y, &pressure);
                fingerStates[i] = new(state == Sdl.Pressed ? FingerState.Down : FingerState.Up, x, y, pressure);
            }
        }
    }
}