namespace HexaEngine.Input
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Input.Events;
    using Newtonsoft.Json.Linq;
    using Silk.NET.SDL;
    using System.Runtime.InteropServices;

    public enum Axis
    {
        X,
        Y
    }

    public class VirtualAxis1
    {
        public string Name;
        public List<VirtualAxisBinding> Bindings;

        public VirtualAxis1(string name)
        {
            Name = name;
            Bindings = new();
        }

        public VirtualAxis1(string name, IEnumerable<VirtualAxisBinding> bindings) : this(name)
        {
            Bindings = new(bindings);
        }
    }

    public enum VirtualAxisBindingType
    {
        KeyboardKey,
        MouseButton,
        JoystickButton,
        GamepadButton,
        GamepadTouch,
        GamepadTouchPressure,
        Touch,
        TouchPressure,
        MouseWheel,
        MouseMovement,
        JoystickBall,
        JoystickAxis,
        JoystickHat,
        GamepadAxis,
        GamepadTouchMovement,
        GamepadSensor,
        TouchMovement,
    }

    public class JsonConverterVirtualAxisBinding : JsonConverter<VirtualAxisBinding>
    {
        public override VirtualAxisBinding ReadJson(JsonReader reader, Type objectType, VirtualAxisBinding existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return default;

            var virtualAxisBinding = new VirtualAxisBinding();

            JObject obj = JObject.Load(reader);

            var typeToken = obj["type"];
            if (typeToken == null)
                throw new JsonSerializationException("Missing 'type' property in VirtualAxisBinding JSON.");

            var type = typeToken.Value<string>();
            if (!Enum.TryParse(type, out virtualAxisBinding.Type))
                throw new JsonSerializationException($"Invalid 'type' value '{type}' for VirtualAxisBinding.");

            var valueToken = obj["value"];
            if (valueToken == null)
                throw new JsonSerializationException("Missing 'value' property in VirtualAxisBinding JSON.");

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

    [JsonConverter(typeof(JsonConverterVirtualAxisBinding))]
    [StructLayout(LayoutKind.Explicit)]
    public struct VirtualAxisBinding
    {
        [FieldOffset(0)]
        public VirtualAxisBindingType Type;

        [FieldOffset(4)]
        public VirtualAxisKeyboardKeyBinding KeyboardKeyBinding;

        [FieldOffset(4)]
        public VirtualAxisMouseButtonBinding MouseButtonBinding;

        [FieldOffset(4)]
        public VirtualAxisJoystickButtonBinding JoystickButtonBinding;

        [FieldOffset(4)]
        public VirtualAxisGamepadButtonBinding GamepadButtonBinding;

        [FieldOffset(4)]
        public VirtualAxisGamepadTouchBinding GamepadTouchBinding;

        [FieldOffset(4)]
        public VirtualAxisGamepadTouchPressureBinding GamepadTouchPressureBinding;

        [FieldOffset(4)]
        public VirtualAxisTouchBinding TouchBinding;

        [FieldOffset(4)]
        public VirtualAxisTouchPressureBinding TouchPressureBinding;

        [FieldOffset(4)]
        public VirtualAxisMouseWheelBinding MouseWheelBinding;

        [FieldOffset(4)]
        public VirtualAxisMouseMovementBinding MouseMovementBinding;

        [FieldOffset(4)]
        public VirtualAxisJoystickBallBinding JoystickBallBinding;

        [FieldOffset(4)]
        public VirtualAxisJoystickAxisBinding JoystickAxisBinding;

        [FieldOffset(4)]
        public VirtualAxisJoystickHatBinding JoystickHatBinding;

        [FieldOffset(4)]
        public VirtualAxisGamepadAxisBinding GamepadAxisBinding;

        [FieldOffset(4)]
        public VirtualAxisGamepadTouchMovementBinding GamepadTouchMovementBinding;

        [FieldOffset(4)]
        public VirtualAxisGamepadSensorBinding GamepadSensorBinding;

        [FieldOffset(4)]
        public VirtualAxisTouchMovementBinding TouchMovementBinding;

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
            public int DeviceId;
        }

        public struct VirtualAxisGamepadTouchPressureBinding
        {
            public int DeviceId;
        }

        public struct VirtualAxisTouchBinding
        {
            public int DeviceId;
        }

        public struct VirtualAxisTouchPressureBinding
        {
            public int DeviceId;
        }

        public struct VirtualAxisMouseWheelBinding
        {
            public Axis Wheel;
        }

        public struct VirtualAxisMouseMovementBinding
        {
            public Axis Axis;
        }

        public struct VirtualAxisJoystickBallBinding
        {
            public int Ball;
            public Axis Axis;
        }

        public struct VirtualAxisJoystickAxisBinding
        {
            public int Axis;
        }

        public struct VirtualAxisJoystickHatBinding
        {
            public JoystickHatState Hat;
        }

        public struct VirtualAxisGamepadAxisBinding
        {
            public GamepadAxis Axis;
        }

        public struct VirtualAxisGamepadTouchMovementBinding
        {
            public int Movement;
        }

        public struct VirtualAxisGamepadSensorBinding
        {
            public SensorType Sensor;
        }

        public struct VirtualAxisTouchMovementBinding
        {
            public int DeviceId;
        }
    }

    /// <summary>
    /// A thread-safe InputBuffer optimized for single-producer single-consumer.
    /// </summary>
    public class InputEventBuffer : IDisposable
    {
        private readonly UnsafeCircularBuffer<InputEvent> inputQueue = new();
        private readonly object _lock = new();

        private uint maxDelay = 15;
        private int maxEvents = 10000;
        private InputEventType filter = InputEventType.None;
        private uint lastTimestamp;
        private bool disposedValue;

        public InputEventBuffer()
        {
            inputQueue = new(maxEvents);
        }

        public InputEventBuffer(TimeSpan inputDelay, int maxEvents, InputEventType filter)
        {
            MaxDelay = inputDelay;
            MaxEvents = maxEvents;
            Filter = filter;
            MaxDelay = inputDelay;
            Filter = filter;
            inputQueue = new(maxEvents);
        }

        /// <summary>
        /// The maximum delay an event can have (default: 15ms) after the event is older than x it will be discarded.
        /// </summary>
        public TimeSpan MaxDelay { get => new(maxDelay); set => maxDelay = (uint)value.Milliseconds; }

        /// <summary>
        /// The maximum amount of events (default: 10000)
        /// </summary>
        public int MaxEvents
        {
            get => maxEvents; set
            {
                maxEvents = value;
                lock (_lock)
                {
                    inputQueue.Resize(value);
                }
            }
        }

        /// <summary>
        /// Allows to filter events, when set to <see cref="InputEventType.None"/> the filter is disabled.
        /// </summary>
        public InputEventType Filter { get => filter; set => filter = value; }

        /// <summary>
        /// The current count of elements in the queue. To access it thread-safe use <see cref="SyncObject"/>
        /// </summary>
        public int Count => inputQueue.Count;

        /// <summary>
        /// The sync object used in lock(SyncObject) expressions.
        /// </summary>
        public object SyncObject => _lock;

        /// <summary>
        /// Records a new event.
        /// </summary>
        /// <param name="inputEvent">The event.</param>
        public void RecordEvent(InputEvent inputEvent)
        {
            // doesn't need to lock here just doing filtering, early exit
            if (filter != InputEventType.None && (inputEvent.Type & filter) == 0)
                return;

            lock (_lock)
            {
                lastTimestamp = inputEvent.Timestamp;
                inputQueue.Enqueue(inputEvent);
            }
        }

        /// <summary>
        /// Polls an event from the queue.
        /// </summary>
        /// <param name="inputEvent">The polled event.</param>
        /// <returns>returns <see langword="true" /> if successful, otherwise <see langword="false" /> if the queue is <see langword="Empty" />.</returns>
        public bool PollEvent(ref InputEvent inputEvent)
        {
            lock (_lock)
            {
                while (inputQueue.TryDequeue(out inputEvent))
                {
                    if (lastTimestamp - inputEvent.Timestamp <= maxDelay)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool PeekEvent(ref InputEvent inputEvent)
        {
            lock (_lock)
            {
                while (inputQueue.TryPeek(out inputEvent))
                {
                    if (lastTimestamp - inputEvent.Timestamp <= maxDelay)
                    {
                        return true;
                    }
                    else
                    {
                        // discard event.
                        inputQueue.Dequeue();
                    }
                }
            }

            return false;
        }

        public void Clear()
        {
            lock (_lock)
            {
                inputQueue.Clear();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                inputQueue.Dispose();
                disposedValue = true;
            }
        }

        ~InputEventBuffer()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}