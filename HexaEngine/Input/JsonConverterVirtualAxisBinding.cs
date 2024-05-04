namespace HexaEngine.Input
{
    using Newtonsoft.Json.Linq;

    public class JsonConverterVirtualAxisBinding : JsonConverter<VirtualAxisBinding>
    {
        public override VirtualAxisBinding ReadJson(JsonReader reader, Type objectType, VirtualAxisBinding existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return default;
            }

            var virtualAxisBinding = new VirtualAxisBinding();

            JObject obj = JObject.Load(reader);

            var typeToken = obj["type"];
            if (typeToken == null)
            {
                throw new JsonSerializationException("Missing 'type' property in VirtualAxisBinding JSON.");
            }

            var type = typeToken.Value<string>();
            if (!Enum.TryParse(type, out virtualAxisBinding.Type))
            {
                throw new JsonSerializationException($"Invalid 'type' value '{type}' for VirtualAxisBinding.");
            }

            var valueToken = obj["value"];
            if (valueToken == null)
            {
                throw new JsonSerializationException("Missing 'value' property in VirtualAxisBinding JSON.");
            }

            switch (virtualAxisBinding.Type)
            {
                case VirtualAxisBindingType.KeyboardKey:
                    virtualAxisBinding.KeyboardKeyBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisKeyboardKeyBinding>(serializer);
                    break;

                case VirtualAxisBindingType.MouseButton:
                    virtualAxisBinding.MouseButtonBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisMouseButtonBinding>(serializer);
                    break;

                case VirtualAxisBindingType.JoystickButton:
                    virtualAxisBinding.JoystickButtonBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisJoystickButtonBinding>(serializer);
                    break;

                case VirtualAxisBindingType.GamepadButton:
                    virtualAxisBinding.GamepadButtonBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisGamepadButtonBinding>(serializer);
                    break;

                case VirtualAxisBindingType.GamepadTouch:
                    virtualAxisBinding.GamepadTouchBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisGamepadTouchBinding>(serializer);
                    break;

                case VirtualAxisBindingType.GamepadTouchPressure:
                    virtualAxisBinding.GamepadTouchPressureBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisGamepadTouchPressureBinding>(serializer);
                    break;

                case VirtualAxisBindingType.Touch:
                    virtualAxisBinding.TouchBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisTouchBinding>(serializer);
                    break;

                case VirtualAxisBindingType.TouchPressure:
                    virtualAxisBinding.TouchPressureBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisTouchPressureBinding>(serializer);
                    break;

                case VirtualAxisBindingType.MouseWheel:
                    virtualAxisBinding.MouseWheelBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisMouseWheelBinding>(serializer);
                    break;

                case VirtualAxisBindingType.MouseMovement:
                    virtualAxisBinding.MouseMovementBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisMouseMovementBinding>(serializer);
                    break;

                case VirtualAxisBindingType.JoystickBall:
                    virtualAxisBinding.JoystickBallBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisJoystickBallBinding>(serializer);
                    break;

                case VirtualAxisBindingType.JoystickAxis:
                    virtualAxisBinding.JoystickAxisBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisJoystickAxisBinding>(serializer);
                    break;

                case VirtualAxisBindingType.JoystickHat:
                    virtualAxisBinding.JoystickHatBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisJoystickHatBinding>(serializer);
                    break;

                case VirtualAxisBindingType.GamepadAxis:
                    virtualAxisBinding.GamepadAxisBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisGamepadAxisBinding>(serializer);
                    break;

                case VirtualAxisBindingType.GamepadTouchMovement:
                    virtualAxisBinding.GamepadTouchMovementBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisGamepadTouchMovementBinding>(serializer);
                    break;

                case VirtualAxisBindingType.GamepadSensor:
                    virtualAxisBinding.GamepadSensorBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisGamepadSensorBinding>(serializer);
                    break;

                case VirtualAxisBindingType.TouchMovement:
                    virtualAxisBinding.TouchMovementBinding = valueToken.ToObject<VirtualAxisBinding.VirtualAxisTouchMovementBinding>(serializer);
                    break;

                default:
                    throw new JsonSerializationException($"Unsupported VirtualAxisBindingType '{virtualAxisBinding.Type}'.");
            }

            return virtualAxisBinding;
        }

        public override void WriteJson(JsonWriter writer, VirtualAxisBinding value, JsonSerializer serializer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("type");
            writer.WriteValue(value.Type);

            writer.WritePropertyName("value");
            switch (value.Type)
            {
                case VirtualAxisBindingType.KeyboardKey:
                    serializer.Serialize(writer, value.KeyboardKeyBinding);
                    break;

                case VirtualAxisBindingType.MouseButton:
                    serializer.Serialize(writer, value.MouseButtonBinding);
                    break;

                case VirtualAxisBindingType.JoystickButton:
                    serializer.Serialize(writer, value.JoystickButtonBinding);
                    break;

                case VirtualAxisBindingType.GamepadButton:
                    serializer.Serialize(writer, value.GamepadButtonBinding);
                    break;

                case VirtualAxisBindingType.GamepadTouch:
                    serializer.Serialize(writer, value.GamepadTouchBinding);
                    break;

                case VirtualAxisBindingType.GamepadTouchPressure:
                    serializer.Serialize(writer, value.GamepadTouchPressureBinding);
                    break;

                case VirtualAxisBindingType.Touch:
                    serializer.Serialize(writer, value.TouchBinding);
                    break;

                case VirtualAxisBindingType.TouchPressure:
                    serializer.Serialize(writer, value.TouchPressureBinding);
                    break;

                case VirtualAxisBindingType.MouseWheel:
                    serializer.Serialize(writer, value.MouseWheelBinding);
                    break;

                case VirtualAxisBindingType.MouseMovement:
                    serializer.Serialize(writer, value.MouseMovementBinding);
                    break;

                case VirtualAxisBindingType.JoystickBall:
                    serializer.Serialize(writer, value.JoystickBallBinding);
                    break;

                case VirtualAxisBindingType.JoystickAxis:
                    serializer.Serialize(writer, value.JoystickAxisBinding);
                    break;

                case VirtualAxisBindingType.JoystickHat:
                    serializer.Serialize(writer, value.JoystickHatBinding);
                    break;

                case VirtualAxisBindingType.GamepadAxis:
                    serializer.Serialize(writer, value.GamepadAxisBinding);
                    break;

                case VirtualAxisBindingType.GamepadTouchMovement:
                    serializer.Serialize(writer, value.GamepadTouchMovementBinding);
                    break;

                case VirtualAxisBindingType.GamepadSensor:
                    serializer.Serialize(writer, value.GamepadSensorBinding);
                    break;

                case VirtualAxisBindingType.TouchMovement:
                    serializer.Serialize(writer, value.TouchMovementBinding);
                    break;

                default:
                    throw new NotSupportedException();
            }

            writer.WriteEndObject();
        }
    }
}