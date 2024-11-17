namespace HexaEngine.Input
{
    using HexaEngine.Input.Events;
    using System.ComponentModel;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public class VirtualAxis : IXmlSerializable
    {
        private VirtualAxisBindingState[] bindingStates = [];

        [XmlAttribute]
        public string Name { get; set; } = null!;

        [XmlArrayItem(ElementName = "Binding")]
        public List<VirtualAxisBinding> Bindings { get; }

        /// <summary>
        /// Speed in units per second that the axis falls toward neutral when no input is present.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(0.0f)]
        public float Gravity;

        [XmlIgnore]
        public VirtualAxisState State;

        public VirtualAxis()
        {
            Bindings = [];
        }

        public VirtualAxis(string name)
        {
            Name = name;
            Bindings = new();
        }

        public VirtualAxis(string name, IEnumerable<VirtualAxisBinding> bindings) : this(name)
        {
            Bindings = new(bindings);
        }

        public void Init()
        {
            bindingStates = new VirtualAxisBindingState[Bindings.Count];
        }

        public void Clear()
        {
            Array.Clear(bindingStates);
            State = default;
        }

        public void Flush()
        {
            float axisValue = 0;
            for (var i = 0; i < bindingStates.Length; i++)
            {
                var bindingState = bindingStates[i];
                if ((bindingState.Flags & VirtualAxisBindingStateFlags.Hold) == 0)
                {
                    var oldValue = bindingState.Value;
                    float newValue;
                    if (Gravity > 0)
                    {
                        if (oldValue == 0)
                        {
                            continue;
                        }

                        newValue = MathF.CopySign(Math.Max(Math.Abs(oldValue) - Gravity * Time.Delta, 0), oldValue);
                    }
                    else
                    {
                        newValue = 0;
                    }
                    bindingState.Value = newValue;
                    bindingStates[i] = bindingState;
                }

                axisValue += bindingState.Value;
            }

            State.Flags &= ~VirtualAxisStateFlags.Pressed;
            State.Flags &= ~VirtualAxisStateFlags.Released;
            State.Value = axisValue;
        }

        public bool TryProcessEvent(InputEvent e, float oldAxisValue, out float newAxisValue)
        {
            newAxisValue = oldAxisValue;

            var axis = Helper.ConvertToAxisBindingType(e.Type);

            bool result = false;

            for (int i = 0; i < Bindings.Count; i++)
            {
                float newState = 0;
                if (ProcessBinding(e, ref newState, axis, Bindings[i], out var flags))
                {
                    var stateBefore = bindingStates[i];
                    bindingStates[i] = new(newState, flags);
                    newAxisValue -= stateBefore.Value;
                    newAxisValue += newState;
                    result = true;
                }
            }

            return result;
        }

        private static bool ProcessBinding(InputEvent e, ref float direction, VirtualAxisBindingType axis, VirtualAxisBinding binding, out VirtualAxisBindingStateFlags flags)
        {
            if (binding.DeviceId != -1 && e.DeviceId != binding.DeviceId)
            {
                flags = VirtualAxisBindingStateFlags.None;
                return false;
            }

            if (axis != binding.Type)
            {
                flags = VirtualAxisBindingStateFlags.None;
                return false;
            }

            float factor = 1;
            if (binding.Invert)
            {
                factor = -1;
            }

            switch (axis)
            {
                case VirtualAxisBindingType.KeyboardKey:
                    var key = e.KeyboardEvent.Key;
                    if (key == binding.KeyboardKeyBinding.Key)
                    {
                        direction = e.KeyboardEvent.State == Core.Input.KeyState.Down ? factor : 0;
                        flags = VirtualAxisBindingStateFlags.Hold;
                        return true;
                    }
                    break;

                case VirtualAxisBindingType.MouseButton:
                    var mouseButton = e.MouseEvent.Button;
                    if (mouseButton == binding.MouseButtonBinding.Button)
                    {
                        direction = e.MouseEvent.State == Core.Input.MouseButtonState.Down ? factor : 0;
                        flags = VirtualAxisBindingStateFlags.Hold;
                        return true;
                    }
                    break;

                case VirtualAxisBindingType.JoystickButton:
                    var joystickButton = e.JoystickButtonEvent.Button;
                    if (joystickButton == binding.JoystickButtonBinding.Button)
                    {
                        direction = e.JoystickButtonEvent.State == Core.Input.JoystickButtonState.Down ? factor : 0;
                        flags = VirtualAxisBindingStateFlags.Hold;
                        return true;
                    }
                    break;

                case VirtualAxisBindingType.GamepadButton:
                    var gamepadButton = e.GamepadButtonEvent.Button;
                    if (gamepadButton == binding.GamepadButtonBinding.Button)
                    {
                        direction = e.GamepadButtonEvent.State == Core.Input.GamepadButtonState.Down ? factor : 0;
                        flags = VirtualAxisBindingStateFlags.Hold;
                        return true;
                    }
                    break;

                case VirtualAxisBindingType.GamepadTouch:
                    var gamepadTouchpad = e.GamepadTouchPadEvent.Touchpad;
                    if (gamepadTouchpad == binding.GamepadTouchBinding.TouchpadId)
                    {
                        direction = e.GamepadTouchPadEvent.State == Core.Input.FingerState.Down ? factor : 0;
                        flags = VirtualAxisBindingStateFlags.Hold;
                        return true;
                    }
                    break;

                case VirtualAxisBindingType.GamepadTouchPressure:
                    var gamepadTouchpad1 = e.GamepadTouchPadEvent.Touchpad;
                    if (gamepadTouchpad1 == binding.GamepadTouchPressureBinding.TouchpadId)
                    {
                        direction = e.GamepadTouchPadEvent.Pressure * factor;
                        flags = VirtualAxisBindingStateFlags.None;
                        return true;
                    }
                    break;

                case VirtualAxisBindingType.Touch:
                    direction = e.TouchDeviceTouchEvent.State == Core.Input.FingerState.Down ? factor : 0;
                    flags = VirtualAxisBindingStateFlags.Hold;
                    return true;

                case VirtualAxisBindingType.TouchPressure:
                    direction = e.TouchDeviceTouchEvent.Pressure * factor;
                    flags = VirtualAxisBindingStateFlags.None;
                    return true;

                case VirtualAxisBindingType.MouseWheel:
                    direction = e.MouseWheelEvent.GetAxis(binding.MouseWheelBinding.Wheel);
                    flags = VirtualAxisBindingStateFlags.None;
                    return true;

                case VirtualAxisBindingType.MouseMovement:
                    direction = e.MouseMovedEvent.GetAxis(binding.MouseMovementBinding.Axis) * binding.MouseMovementBinding.Sensitivity * factor;
                    flags = VirtualAxisBindingStateFlags.None;
                    return true;

                case VirtualAxisBindingType.JoystickBall:
                    direction = e.JoystickBallMotionEvent.GetAxis(binding.JoystickBallBinding.Axis);
                    flags = VirtualAxisBindingStateFlags.None;
                    return true;

                case VirtualAxisBindingType.JoystickAxis:
                    if (e.JoystickAxisMotionEvent.Axis == binding.JoystickAxisBinding.Axis && Math.Abs(e.JoystickAxisMotionEvent.Value) >= binding.JoystickAxisBinding.Deadzone)
                    {
                        direction = (e.JoystickAxisMotionEvent.Value + 32767) / (float)65534 - 0.5f;
                        flags = VirtualAxisBindingStateFlags.Hold;
                        return true;
                    }
                    break;

                case VirtualAxisBindingType.JoystickHat:
                    if (e.JoystickHatMotionEvent.Hat != binding.JoystickHatBinding.Hat)
                    {
                        flags = VirtualAxisBindingStateFlags.Hold;
                        return false;
                    }
                    var hat = e.JoystickHatMotionEvent.State;

                    if (hat == binding.JoystickHatBinding.State)
                    {
                        direction = 1;
                        flags = VirtualAxisBindingStateFlags.Hold;
                        return true;
                    }
                    else
                    {
                        direction = 0;
                        flags = VirtualAxisBindingStateFlags.Hold;
                        return true;
                    }

                case VirtualAxisBindingType.GamepadAxis:
                    if (e.GamepadAxisMotionEvent.Axis == binding.GamepadAxisBinding.Axis && Math.Abs((int)e.GamepadAxisMotionEvent.Value) >= binding.GamepadAxisBinding.Deadzone)
                    {
                        direction = ((e.GamepadAxisMotionEvent.Value + 32767) / (float)65534 - 0.5f) * binding.GamepadAxisBinding.Sensitivity * factor;
                        flags = VirtualAxisBindingStateFlags.Hold;
                        return true;
                    }
                    break;

                case VirtualAxisBindingType.GamepadTouchMovement:
                    direction = e.GamepadTouchPadMotionEvent.GetAxis(binding.GamepadTouchMovementBinding.Axis);
                    flags = VirtualAxisBindingStateFlags.None;
                    return true;

                case VirtualAxisBindingType.GamepadSensor:
                    if (e.GamepadSensorUpdateEvent.Type == binding.GamepadSensorBinding.Type)
                    {
                        direction = e.GamepadSensorUpdateEvent.GetAxis(binding.GamepadSensorBinding.Axis);
                        flags = VirtualAxisBindingStateFlags.None;
                        return true;
                    }
                    break;

                case VirtualAxisBindingType.TouchMovement:
                    direction = e.TouchDeviceTouchMotionEvent.GetAxis(binding.TouchMovementBinding.Axis);
                    flags = VirtualAxisBindingStateFlags.None;
                    return true;
            }

            flags = VirtualAxisBindingStateFlags.None;
            return false;
        }

        public XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();

            Name = reader.GetAttribute("Name")!;
            string? gravityAttr = reader.GetAttribute("Gravity");
            if (gravityAttr != null)
            {
                Gravity = float.Parse(gravityAttr);
            }

            reader.ReadStartElement();

            if (reader.IsEmptyElement)
            {
                reader.ReadEndElement();
                return;
            }

            reader.ReadStartElement("Bindings");

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Bindings")
                    break;

                VirtualAxisBinding binding = new();
                binding.ReadXml(reader);
                Bindings.Add(binding);
            }

            reader.ReadEndElement(); // End of VirtualAxis element
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", Name);
            if (Gravity != 0)
            {
                writer.WriteAttributeString("Gravity", Gravity.ToString());
            }

            writer.WriteStartElement("Bindings");
            foreach (var binding in Bindings)
            {
                writer.WriteStartElement("Binding");
                binding.WriteXml(writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}