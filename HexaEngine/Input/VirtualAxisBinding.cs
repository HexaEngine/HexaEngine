namespace HexaEngine.Input
{
    using HexaEngine.Core.Input;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [JsonConverter(typeof(JsonConverterVirtualAxisBinding))]
    [StructLayout(LayoutKind.Explicit)]
    public struct VirtualAxisBinding : IXmlSerializable
    {
        private const int UnionOffset = 12;

        [FieldOffset(0)]
        public VirtualAxisBindingType Type;

        [FieldOffset(4)]
        public int DeviceId;

        [FieldOffset(8)]
        public bool Invert;

        [FieldOffset(UnionOffset)]
        public VirtualAxisKeyboardKeyBinding KeyboardKeyBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisMouseButtonBinding MouseButtonBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisJoystickButtonBinding JoystickButtonBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisGamepadButtonBinding GamepadButtonBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisGamepadTouchBinding GamepadTouchBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisGamepadTouchPressureBinding GamepadTouchPressureBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisTouchBinding TouchBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisTouchPressureBinding TouchPressureBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisMouseWheelBinding MouseWheelBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisMouseMovementBinding MouseMovementBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisJoystickBallBinding JoystickBallBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisJoystickAxisBinding JoystickAxisBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisJoystickHatBinding JoystickHatBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisGamepadAxisBinding GamepadAxisBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisGamepadTouchMovementBinding GamepadTouchMovementBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisGamepadSensorBinding GamepadSensorBinding;

        [FieldOffset(UnionOffset)]
        public VirtualAxisTouchMovementBinding TouchMovementBinding;

        public XmlSchema? GetSchema()
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();

            bool isEmptyElement = reader.IsEmptyElement;

            Type = Enum.Parse<VirtualAxisBindingType>(reader.GetAttribute("Type")!);
            DeviceId = int.Parse(reader.GetAttribute("DeviceId")!);
            Invert = bool.Parse(reader.GetAttribute("Invert")!);

            if (!isEmptyElement)
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "Binding")
                    {
                        break;
                    }

                    switch (Type)
                    {
                        case VirtualAxisBindingType.KeyboardKey:
                            ReadEnum(reader, "Key", ref KeyboardKeyBinding.Key);
                            break;

                        case VirtualAxisBindingType.MouseButton:
                            ReadEnum(reader, "Button", ref MouseButtonBinding.Button);
                            break;

                        case VirtualAxisBindingType.JoystickButton:
                            ReadInt(reader, "Button", ref JoystickButtonBinding.Button);
                            break;

                        case VirtualAxisBindingType.GamepadButton:
                            ReadEnum(reader, "Button", ref GamepadButtonBinding.Button);
                            break;

                        case VirtualAxisBindingType.GamepadTouch:
                            ReadInt(reader, "TouchpadId", ref GamepadTouchBinding.TouchpadId);
                            break;

                        case VirtualAxisBindingType.GamepadTouchPressure:
                            ReadInt(reader, "TouchpadId", ref GamepadTouchPressureBinding.TouchpadId);
                            break;

                        case VirtualAxisBindingType.MouseWheel:
                            ReadEnum(reader, "Wheel", ref MouseWheelBinding.Wheel);
                            break;

                        case VirtualAxisBindingType.MouseMovement:
                            ReadEnum(reader, "Axis", ref MouseMovementBinding.Axis);
                            ReadFloat(reader, "Sensitivity", ref MouseMovementBinding.Sensitivity);
                            break;

                        case VirtualAxisBindingType.JoystickBall:
                            ReadInt(reader, "Ball", ref JoystickBallBinding.Ball);
                            ReadEnum(reader, "Axis", ref JoystickBallBinding.Axis);
                            break;

                        case VirtualAxisBindingType.JoystickAxis:
                            ReadInt(reader, "Axis", ref JoystickAxisBinding.Axis);
                            ReadInt(reader, "Deadzone", ref JoystickAxisBinding.Deadzone);
                            ReadFloat(reader, "Sensitivity", ref JoystickAxisBinding.Sensitivity);
                            break;

                        case VirtualAxisBindingType.JoystickHat:
                            ReadInt(reader, "Hat", ref JoystickHatBinding.Hat);
                            ReadEnum(reader, "State", ref JoystickHatBinding.State);
                            break;

                        case VirtualAxisBindingType.GamepadAxis:
                            ReadEnum(reader, "Axis", ref GamepadAxisBinding.Axis);
                            ReadInt(reader, "Deadzone", ref GamepadAxisBinding.Deadzone);
                            ReadFloat(reader, "Sensitivity", ref GamepadAxisBinding.Sensitivity);
                            break;

                        case VirtualAxisBindingType.GamepadTouchMovement:
                            ReadEnum(reader, "Axis", ref GamepadTouchMovementBinding.Axis);
                            break;

                        case VirtualAxisBindingType.GamepadSensor:
                            ReadEnum(reader, "Type", ref GamepadSensorBinding.Type);
                            ReadEnum(reader, "Axis", ref GamepadSensorBinding.Axis);
                            break;

                        case VirtualAxisBindingType.TouchMovement:
                            ReadEnum(reader, "Axis", ref TouchMovementBinding.Axis);
                            break;
                    }
                }
            }

            reader.ReadEndElement();
        }

        private static void ReadFloat(XmlReader reader, string name, ref float value)
        {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != name) return;
            reader.ReadStartElement(name);
            value = reader.ReadContentAsFloat();
            reader.ReadEndElement();
        }

        private static void ReadInt(XmlReader reader, string name, ref int value)
        {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != name) return;
            reader.ReadStartElement(name);
            value = reader.ReadContentAsInt();
            reader.ReadEndElement();
        }

        private static void ReadEnum<T>(XmlReader reader, string name, ref T value) where T : struct, Enum
        {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != name) return;
            reader.ReadStartElement(name);
            value = Enum.Parse<T>(reader.ReadContentAsString());
            reader.ReadEndElement();
        }

        public readonly void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Type", Type.ToString());
            writer.WriteAttributeString("DeviceId", DeviceId.ToString());
            writer.WriteAttributeString("Invert", Invert.ToString());

            switch (Type)
            {
                case VirtualAxisBindingType.KeyboardKey:
                    writer.WriteElementString("Key", KeyboardKeyBinding.Key.ToString());
                    break;

                case VirtualAxisBindingType.MouseButton:
                    writer.WriteElementString("Button", MouseButtonBinding.Button.ToString());
                    break;

                case VirtualAxisBindingType.JoystickButton:
                    writer.WriteElementString("Button", JoystickButtonBinding.Button.ToString());
                    break;

                case VirtualAxisBindingType.GamepadButton:
                    writer.WriteElementString("Button", GamepadButtonBinding.Button.ToString());
                    break;

                case VirtualAxisBindingType.GamepadTouch:
                    writer.WriteElementString("TouchpadId", GamepadTouchBinding.TouchpadId.ToString());
                    break;

                case VirtualAxisBindingType.GamepadTouchPressure:
                    writer.WriteElementString("TouchpadId", GamepadTouchPressureBinding.TouchpadId.ToString());
                    break;

                case VirtualAxisBindingType.Touch:
                    //writer.WriteElementString("Touch", "");
                    break;

                case VirtualAxisBindingType.TouchPressure:
                    //writer.WriteElementString("TouchPressure", "");
                    break;

                case VirtualAxisBindingType.MouseWheel:
                    writer.WriteElementString("Wheel", MouseWheelBinding.Wheel.ToString());
                    break;

                case VirtualAxisBindingType.MouseMovement:
                    writer.WriteElementString("Axis", MouseMovementBinding.Axis.ToString());
                    writer.WriteElementString("Sensitivity", MouseMovementBinding.Sensitivity.ToString());
                    break;

                case VirtualAxisBindingType.JoystickBall:
                    writer.WriteElementString("Ball", JoystickBallBinding.Ball.ToString());
                    writer.WriteElementString("Axis", JoystickBallBinding.Axis.ToString());
                    break;

                case VirtualAxisBindingType.JoystickAxis:
                    writer.WriteElementString("Axis", JoystickAxisBinding.Axis.ToString());
                    writer.WriteElementString("Deadzone", JoystickAxisBinding.Deadzone.ToString());
                    writer.WriteElementString("Sensitivity", JoystickAxisBinding.Sensitivity.ToString());
                    break;

                case VirtualAxisBindingType.JoystickHat:
                    writer.WriteElementString("Hat", JoystickHatBinding.Hat.ToString());
                    writer.WriteElementString("State", JoystickHatBinding.State.ToString());
                    break;

                case VirtualAxisBindingType.GamepadAxis:
                    writer.WriteElementString("Axis", GamepadAxisBinding.Axis.ToString());
                    writer.WriteElementString("Deadzone", GamepadAxisBinding.Deadzone.ToString());
                    writer.WriteElementString("Sensitivity", GamepadAxisBinding.Sensitivity.ToString());
                    break;

                case VirtualAxisBindingType.GamepadTouchMovement:
                    writer.WriteElementString("Axis", GamepadTouchMovementBinding.Axis.ToString());
                    break;

                case VirtualAxisBindingType.GamepadSensor:
                    writer.WriteElementString("Type", GamepadSensorBinding.Type.ToString());
                    writer.WriteElementString("Axis", GamepadSensorBinding.Axis.ToString());
                    break;

                case VirtualAxisBindingType.TouchMovement:
                    writer.WriteElementString("Axis", TouchMovementBinding.Axis.ToString());
                    break;
            }
        }

        public struct VirtualAxisKeyboardKeyBinding
        {
            public Key Key;
        }

        public struct VirtualAxisMouseButtonBinding
        {
            public MouseButton Button;
        }

        public struct VirtualAxisJoystickButtonBinding
        {
            public int Button;
        }

        public struct VirtualAxisGamepadButtonBinding
        {
            public GamepadButton Button;
        }

        public struct VirtualAxisGamepadTouchBinding
        {
            public int TouchpadId;
        }

        public struct VirtualAxisGamepadTouchPressureBinding
        {
            public int TouchpadId;
        }

        public struct VirtualAxisTouchBinding
        {
        }

        public struct VirtualAxisTouchPressureBinding
        {
        }

        public struct VirtualAxisMouseWheelBinding
        {
            public Axis Wheel;
        }

        public struct VirtualAxisMouseMovementBinding
        {
            public Axis Axis;
            public float Sensitivity;
        }

        public struct VirtualAxisJoystickBallBinding
        {
            public int Ball;
            public Axis Axis;
        }

        public struct VirtualAxisJoystickAxisBinding
        {
            public int Axis;
            public int Deadzone;
            public float Sensitivity;
        }

        public struct VirtualAxisJoystickHatBinding
        {
            public int Hat;
            public JoystickHatState State;
        }

        public struct VirtualAxisGamepadAxisBinding
        {
            public GamepadAxis Axis;
            public int Deadzone;
            public float Sensitivity;
        }

        public struct VirtualAxisGamepadTouchMovementBinding
        {
            public Axis Axis;
        }

        public struct VirtualAxisGamepadSensorBinding
        {
            public GamepadSensorType Type;
            public SensorAxis Axis;
        }

        public struct VirtualAxisTouchMovementBinding
        {
            public Axis Axis;
        }
    }
}