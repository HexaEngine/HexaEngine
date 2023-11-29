namespace HexaEngine.Input
{
    using HexaEngine.Input.Events;

    public struct VirtualAxis : IEquatable<VirtualAxis>
    {
        /// <summary>
        /// The axis name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Negative button, can be a keyboard key, mouse button, joystick button or gamepad button.
        /// </summary>
        public int NegativeButton;

        /// <summary>
        /// Positive button, can be a keyboard key, mouse button, joystick button or gamepad button.
        /// </summary>
        public int PositiveButton;

        /// <summary>
        /// Alternative Negative button, can be a keyboard key, mouse button, joystick button or gamepad button.
        /// </summary>
        public int AltNegativeButton;

        /// <summary>
        /// Alternative Positive button, can be a keyboard key, mouse button, joystick button or gamepad button.
        /// </summary>
        public int AltPositiveButton;

        /// <summary>
        /// Speed in units per second that the axis falls toward neutral when no input is present.
        /// </summary>
        public float Gravity;

        /// <summary>
        /// How far the user needs to move an analog stick before your application registers the movement.
        /// </summary>
        public int Deadzone;

        /// <summary>
        /// Speed in units per second that the axis will move toward the target value. This is for digital devices only.
        /// </summary>
        public float Sensitivity;

        /// <summary>
        /// If enabled, the axis value will reset to zero when pressing a button that corresponds to the opposite direction.
        /// </summary>
        public bool Snap;

        /// <summary>
        /// The type of input that controls the axis
        /// </summary>
        public AxisType Type;

        /// <summary>
        /// The axis of a connected device that controls this axis.
        /// </summary>
        public int Axis;

        /// <summary>
        /// The device that controls this axis, set to -1 to query input from all devices.
        /// </summary>
        public int DeviceId;

        public override bool Equals(object? obj)
        {
            return obj is VirtualAxis axis && Equals(axis);
        }

        public bool Equals(VirtualAxis other)
        {
            return Name == other.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public bool TryProcessEvent(InputEvent e, out float direction)
        {
            direction = 0;

            if (DeviceId != -1 && e.DeviceId != DeviceId)
            {
                return false;
            }

            var axis = Helper.ConvertToAxisType(e.Type);

            if (axis != Type)
            {
                return false;
            }

            switch (axis)
            {
                case AxisType.KeyboardKey:
                    var key = (int)e.KeyboardEvent.Key;
                    if (key == PositiveButton || key == AltPositiveButton)
                    {
                        direction = e.KeyboardEvent.State == Core.Input.KeyState.Down ? 1 : 0;
                        return true;
                    }
                    if (key == NegativeButton || key == AltNegativeButton)
                    {
                        direction = e.KeyboardEvent.State == Core.Input.KeyState.Down ? -1 : 0;
                        return true;
                    }
                    break;

                case AxisType.MouseButton:
                    var mouseButton = (int)e.MouseEvent.Button;
                    if (mouseButton == PositiveButton || mouseButton == AltPositiveButton)
                    {
                        direction = e.MouseEvent.State == Core.Input.MouseButtonState.Down ? 1 : 0;
                        return true;
                    }
                    if (mouseButton == NegativeButton || mouseButton == AltNegativeButton)
                    {
                        direction = e.MouseEvent.State == Core.Input.MouseButtonState.Down ? -1 : 0;
                        return true;
                    }
                    break;

                case AxisType.JoystickButton:
                    var joystickButton = e.JoystickButtonEvent.Button;
                    if (joystickButton == PositiveButton || joystickButton == AltPositiveButton)
                    {
                        direction = e.JoystickButtonEvent.State == Core.Input.JoystickButtonState.Down ? 1 : 0;
                        return true;
                    }
                    if (joystickButton == NegativeButton || joystickButton == AltNegativeButton)
                    {
                        direction = e.JoystickButtonEvent.State == Core.Input.JoystickButtonState.Down ? -1 : 0;
                        return true;
                    }
                    break;

                case AxisType.GamepadButton:
                    var gamepadButton = (int)e.GamepadButtonEvent.Button;
                    if (gamepadButton == PositiveButton || gamepadButton == AltPositiveButton)
                    {
                        direction = e.GamepadButtonEvent.State == Core.Input.GamepadButtonState.Down ? 1 : 0;
                        return true;
                    }
                    if (gamepadButton == NegativeButton || gamepadButton == AltNegativeButton)
                    {
                        direction = e.GamepadButtonEvent.State == Core.Input.GamepadButtonState.Down ? -1 : 0;
                        return true;
                    }
                    break;

                case AxisType.GamepadTouch:
                    var gamepadTouchpad = e.GamepadTouchPadEvent.Touchpad;
                    if (gamepadTouchpad == Axis)
                    {
                        direction = e.GamepadTouchPadEvent.State == Core.Input.FingerState.Down ? 1 : 0;
                        return true;
                    }
                    break;

                case AxisType.GamepadTouchPressure:
                    var gamepadTouchpad1 = e.GamepadTouchPadEvent.Touchpad;
                    if (gamepadTouchpad1 == Axis)
                    {
                        direction = e.GamepadTouchPadEvent.Pressure;
                        return true;
                    }
                    break;

                case AxisType.Touch:
                    direction = e.TouchDeviceTouchEvent.State == Core.Input.FingerState.Down ? 1 : 0;
                    return true;

                case AxisType.TouchPressure:
                    direction = e.TouchDeviceTouchEvent.Pressure;
                    return true;

                case AxisType.MouseWheel:
                    direction = e.MouseWheelEvent.GetAxis(Axis);
                    return true;

                case AxisType.MouseMovement:
                    direction = e.MouseMovedEvent.GetAxis(Axis) * Sensitivity;
                    return true;

                case AxisType.JoystickBall:
                    direction = e.JoystickBallMotionEvent.GetAxis(Axis);
                    return true;

                case AxisType.JoystickAxis:
                    if (e.JoystickAxisMotionEvent.Axis != Axis || Math.Abs(e.JoystickAxisMotionEvent.Value) < Deadzone)
                    {
                        return false;
                    }
                    direction = (e.JoystickAxisMotionEvent.Value + 32767) / (float)65534 - 0.5f;
                    return true;

                case AxisType.JoystickHat:
                    if (e.JoystickHatMotionEvent.Hat != Axis)
                    {
                        return false;
                    }
                    var hat = (int)e.JoystickHatMotionEvent.State;
                    if (hat == 0)
                    {
                        direction = 0;
                        return true;
                    }
                    if (hat == PositiveButton || hat == AltPositiveButton)
                    {
                        direction = 1;
                        return true;
                    }
                    if (hat == NegativeButton || hat == AltNegativeButton)
                    {
                        direction = -1;
                        return true;
                    }
                    return true;

                case AxisType.GamepadAxis:
                    if ((int)e.GamepadAxisMotionEvent.Axis != Axis || Math.Abs(e.GamepadAxisMotionEvent.Value) < Deadzone)
                    {
                        return false;
                    }
                    direction = (e.GamepadAxisMotionEvent.Value + 32767) / (float)65534 - 0.5f;
                    return true;

                case AxisType.GamepadTouchMovement:
                    direction = e.GamepadTouchPadMotionEvent.GetAxis(Axis);
                    return true;

                case AxisType.GamepadSensor:
                    direction = e.GamepadSensorUpdateEvent.GetAxis(Axis);
                    return true;

                case AxisType.TouchMovement:
                    direction = e.TouchDeviceTouchMotionEvent.GetAxis(Axis);
                    return true;
            }

            return false;
        }

        public static bool operator ==(VirtualAxis left, VirtualAxis right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VirtualAxis left, VirtualAxis right)
        {
            return !(left == right);
        }
    }

    public static class Helper
    {
        public static AxisType ConvertToAxisType(InputEventType type)
        {
            return type switch
            {
                InputEventType.None => throw new NotSupportedException(),
                InputEventType.MouseMoved => AxisType.MouseMovement,
                InputEventType.MouseUp => AxisType.MouseButton,
                InputEventType.MouseDown => AxisType.MouseButton,
                InputEventType.MouseWheel => AxisType.MouseWheel,
                InputEventType.KeyboardKeyDown => AxisType.KeyboardKey,
                InputEventType.KeyboardKeyUp => AxisType.KeyboardKey,
                InputEventType.JoystickAxisMotion => AxisType.JoystickAxis,
                InputEventType.JoystickBallMotion => AxisType.JoystickBall,
                InputEventType.JoystickButtonDown => AxisType.JoystickButton,
                InputEventType.JoystickButtonUp => AxisType.JoystickButton,
                InputEventType.JoystickHatMotion => AxisType.JoystickHat,
                InputEventType.GamepadAxisMotion => AxisType.GamepadAxis,
                InputEventType.GamepadButtonDown => AxisType.GamepadButton,
                InputEventType.GamepadButtonUp => AxisType.GamepadButton,
                InputEventType.GamepadSensorUpdate => AxisType.GamepadSensor,
                InputEventType.GamepadTouchPadDown => AxisType.GamepadTouch,
                InputEventType.GamepadTouchPadUp => AxisType.GamepadTouch,
                InputEventType.GamepadTouchPadMotion => AxisType.GamepadTouchMovement,
                InputEventType.TouchDeviceTouchDown => AxisType.Touch,
                InputEventType.TouchDeviceTouchUp => AxisType.Touch,
                InputEventType.TouchDeviceTouchMotion => AxisType.TouchMovement,
                _ => throw new NotSupportedException(),
            };
        }
    }
}