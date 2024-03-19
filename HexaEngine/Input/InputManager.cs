namespace HexaEngine.Input
{
    using HexaEngine.Core;
    using HexaEngine.Core.Input;
    using HexaEngine.Input.Events;

    public class InputManager : IInputManager
    {
        private readonly List<VirtualAxis> virtualAxes = new();
        private readonly Dictionary<string, float> virtualAxisStates = new();
        private readonly Dictionary<string, float> virtualAxisStatesRaw = new();
        private readonly Dictionary<Key, KeyState> keyStates = new();
        private readonly Dictionary<MouseButton, MouseButtonState> mouseButtonStates = new();
        private readonly HashSet<string> frameUpAxis = new();
        private readonly HashSet<string> frameDownAxis = new();
        private readonly HashSet<Key> frameUpKeys = new();
        private readonly HashSet<Key> frameDownKeys = new();
        private readonly HashSet<MouseButton> frameUpMouseButtons = new();
        private readonly HashSet<MouseButton> frameDownMouseButtons = new();
        private readonly InputEventBuffer inputBuffer = new();
        private bool disposedValue;

        public InputManager()
        {
            Mouse.Moved += OnMouseMoved;
            Mouse.ButtonDown += OnMouseButtonDown;
            Mouse.ButtonUp += OnMouseButtonUp;
            Mouse.Wheel += OnMouseWheel;
            Keyboard.KeyDown += OnKeyboardKeyDown;
            Keyboard.KeyUp += OnKeyboardKeyUp;
            Joysticks.AxisMotion += OnJoysticksAxisMotion;
            Joysticks.BallMotion += OnJoysticksBallMotion;
            Joysticks.ButtonDown += OnJoysticksButtonDown;
            Joysticks.ButtonUp += OnJoysticksButtonUp;
            Joysticks.HatMotion += OnJoysticksHatMotion;
            Gamepads.AxisMotion += OnGamepadsAxisMotion;
            Gamepads.ButtonDown += OnGamepadsButtonDown;
            Gamepads.ButtonUp += OnGamepadsButtonUp;
            Gamepads.SensorUpdate += OnGamepadsSensorUpdate;
            Gamepads.TouchPadDown += OnGamepadsTouchPadDown;
            Gamepads.TouchPadUp += OnGamepadsTouchPadUp;
            Gamepads.TouchPadMotion += OnGamepadsTouchPadMotion;
            TouchDevices.TouchDown += OnTouchDevicesTouchDown;
            TouchDevices.TouchUp += OnTouchDevicesTouchUp;
            TouchDevices.TouchMotion += OnTouchDevicesTouchMotion;

            foreach (Key key in Keyboard.Keys)
            {
                keyStates.Add(key, Keyboard.States[key]);
            }

            foreach (MouseButton button in Mouse.Buttons)
            {
                mouseButtonStates.Add(button, Mouse.States[button]);
            }
        }

        public IReadOnlyList<VirtualAxis> VirtualAxes => virtualAxes;

        public void AddAxis(VirtualAxis axis)
        {
            virtualAxes.Add(axis);
            virtualAxisStates.Add(axis.Name, 0);
            virtualAxisStatesRaw.Add(axis.Name, 0);
        }

        public void RemoveAxis(VirtualAxis axis)
        {
            virtualAxes.Remove(axis);
            virtualAxisStates.Remove(axis.Name);
            virtualAxisStatesRaw.Remove(axis.Name);
        }

        public void SetAxis(VirtualAxis axis)
        {
            var index = virtualAxes.IndexOf(axis);
            virtualAxes[index] = axis;
        }

        public float GetAxis(string name)
        {
            return virtualAxisStates[name];
        }

        public float GetAxisRaw(string name)
        {
            return virtualAxisStatesRaw[name];
        }

        public bool GetButton(string name)
        {
            return virtualAxisStates[name] != 0;
        }

        public bool GetButtonDown(string name)
        {
            return frameDownAxis.Contains(name);
        }

        public bool GetButtonUp(string name)
        {
            return frameUpAxis.Contains(name);
        }

        public bool GetMouseButton(MouseButton button)
        {
            return mouseButtonStates[button] == MouseButtonState.Down;
        }

        public bool GetMouseButtonDown(MouseButton button)
        {
            return frameDownMouseButtons.Contains(button);
        }

        public bool GetMouseButtonUp(MouseButton button)
        {
            return frameUpMouseButtons.Contains(button);
        }

        public bool GetKey(Key key)
        {
            return keyStates[key] == KeyState.Down;
        }

        public bool GetKeyDown(Key key)
        {
            return frameDownKeys.Contains(key);
        }

        public bool GetKeyUp(Key key)
        {
            return frameUpKeys.Contains(key);
        }

        public InputEventBuffer InputBuffer => InputBuffer;

        internal void Update()
        {
            ((IInputManager)this).Update();
        }

        void IInputManager.Update()
        {
            frameDownAxis.Clear();
            frameUpAxis.Clear();
            frameDownKeys.Clear();
            frameUpKeys.Clear();
            frameDownMouseButtons.Clear();
            frameUpMouseButtons.Clear();
            InputEvent inputEvent = default;

            while (inputBuffer.PollEvent(ref inputEvent))
            {
                for (int i = 0; i < virtualAxes.Count; i++)
                {
                    var virtualAxis = virtualAxes[i];
                    if (virtualAxis.TryProcessEvent(inputEvent, out float direction))
                    {
                        var oldValue = virtualAxisStates[virtualAxis.Name];
                        float newValue = direction;
                        if (direction == 0 && virtualAxis.Gravity > 0)
                        {
                            if (oldValue == 0)
                            {
                                continue;
                            }

                            newValue = MathF.CopySign(Math.Max(Math.Abs(oldValue) - virtualAxis.Gravity * Time.Delta, 0), oldValue);
                        }

                        if (oldValue == 0 && newValue != 0)
                        {
                            frameDownAxis.Add(virtualAxis.Name);
                        }

                        if (oldValue != 0 && newValue == 0)
                        {
                            frameUpAxis.Add(virtualAxis.Name);
                        }

                        virtualAxisStates[virtualAxis.Name] = newValue;
                    }
                }

                switch (inputEvent.Type)
                {
                    case InputEventType.MouseUp:
                        frameUpMouseButtons.Add(inputEvent.MouseEvent.Button);
                        mouseButtonStates[inputEvent.MouseEvent.Button] = inputEvent.MouseEvent.State;
                        break;

                    case InputEventType.MouseDown:
                        frameDownMouseButtons.Add(inputEvent.MouseEvent.Button);
                        mouseButtonStates[inputEvent.MouseEvent.Button] = inputEvent.MouseEvent.State;
                        break;

                    case InputEventType.KeyboardKeyDown:
                        frameDownKeys.Add(inputEvent.KeyboardEvent.Key);
                        keyStates[inputEvent.KeyboardEvent.Key] = inputEvent.KeyboardEvent.State;
                        break;

                    case InputEventType.KeyboardKeyUp:
                        frameUpKeys.Add(inputEvent.KeyboardEvent.Key);
                        keyStates[inputEvent.KeyboardEvent.Key] = inputEvent.KeyboardEvent.State;
                        break;
                }
            }
        }

        #region EventHandlers

        private void OnTouchDevicesTouchMotion(object? sender, Core.Input.Events.TouchMoveEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnTouchDevicesTouchUp(object? sender, Core.Input.Events.TouchEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnTouchDevicesTouchDown(object? sender, Core.Input.Events.TouchEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnGamepadsTouchPadMotion(object? sender, Core.Input.Events.GamepadTouchpadMotionEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnGamepadsTouchPadUp(object? sender, Core.Input.Events.GamepadTouchpadEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnGamepadsTouchPadDown(object? sender, Core.Input.Events.GamepadTouchpadEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnGamepadsSensorUpdate(object? sender, Core.Input.Events.GamepadSensorUpdateEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnGamepadsButtonUp(object? sender, Core.Input.Events.GamepadButtonEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnGamepadsButtonDown(object? sender, Core.Input.Events.GamepadButtonEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnGamepadsAxisMotion(object? sender, Core.Input.Events.GamepadAxisMotionEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnJoysticksHatMotion(object? sender, Core.Input.Events.JoystickHatMotionEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnJoysticksButtonUp(object? sender, Core.Input.Events.JoystickButtonEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnJoysticksButtonDown(object? sender, Core.Input.Events.JoystickButtonEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnJoysticksBallMotion(object? sender, Core.Input.Events.JoystickBallMotionEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnJoysticksAxisMotion(object? sender, Core.Input.Events.JoystickAxisMotionEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnKeyboardKeyUp(object? sender, Core.Input.Events.KeyboardEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnKeyboardKeyDown(object? sender, Core.Input.Events.KeyboardEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnMouseWheel(object? sender, Core.Input.Events.MouseWheelEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnMouseButtonUp(object? sender, Core.Input.Events.MouseButtonEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnMouseButtonDown(object? sender, Core.Input.Events.MouseButtonEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        private void OnMouseMoved(object? sender, Core.Input.Events.MouseMoveEventArgs e)
        {
            inputBuffer.RecordEvent(new(e));
        }

        #endregion EventHandlers

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Mouse.Moved -= OnMouseMoved;
                Mouse.ButtonDown -= OnMouseButtonDown;
                Mouse.ButtonUp -= OnMouseButtonUp;
                Mouse.Wheel -= OnMouseWheel;
                Keyboard.KeyDown -= OnKeyboardKeyDown;
                Keyboard.KeyUp -= OnKeyboardKeyUp;
                Joysticks.AxisMotion -= OnJoysticksAxisMotion;
                Joysticks.BallMotion -= OnJoysticksBallMotion;
                Joysticks.ButtonDown -= OnJoysticksButtonDown;
                Joysticks.ButtonUp -= OnJoysticksButtonUp;
                Joysticks.HatMotion -= OnJoysticksHatMotion;
                Gamepads.AxisMotion -= OnGamepadsAxisMotion;
                Gamepads.ButtonDown -= OnGamepadsButtonDown;
                Gamepads.ButtonUp -= OnGamepadsButtonUp;
                Gamepads.SensorUpdate -= OnGamepadsSensorUpdate;
                Gamepads.TouchPadDown -= OnGamepadsTouchPadDown;
                Gamepads.TouchPadUp -= OnGamepadsTouchPadUp;
                Gamepads.TouchPadMotion -= OnGamepadsTouchPadMotion;
                TouchDevices.TouchDown -= OnTouchDevicesTouchDown;
                TouchDevices.TouchUp -= OnTouchDevicesTouchUp;
                TouchDevices.TouchMotion -= OnTouchDevicesTouchMotion;
                inputBuffer.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}