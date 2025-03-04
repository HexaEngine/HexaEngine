namespace HexaEngine.Core.Input
{
    using Hexa.NET.SDL3;
    using HexaEngine.Core.Input.Events;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using static Extensions.SdlErrorHandlingExtensions;

    /// <summary>
    /// Represents a gamepad input device.
    /// </summary>
    public unsafe class Gamepad : IDisposable
    {
        private static readonly GamepadAxis[] axes = Enum.GetValues<GamepadAxis>();
        private static readonly string[] axisNames = new string[axes.Length];
        private static readonly GamepadButton[] buttons = Enum.GetValues<GamepadButton>();
        private static readonly string[] buttonNames = new string[buttons.Length];

        private static readonly GamepadSensorType[] sensorTypes = Enum.GetValues<GamepadSensorType>();

        private readonly int id;
        private readonly SDLGamepad* controller;
        internal readonly SDLJoystick* joystick;
        private readonly Dictionary<GamepadAxis, short> axisStates = new();
        private readonly Dictionary<GamepadButton, GamepadButtonState> buttonStates = new();
        private readonly Dictionary<GamepadSensorType, GamepadSensor> sensors = new();
        private readonly List<GamepadTouchpad> touchpads = new();
        private readonly Haptic? haptic;
        private readonly List<string> mappings = new();

        private readonly GamepadRemappedEventArgs remappedEventArgs = new();
        private readonly GamepadAxisMotionEventArgs axisMotionEventArgs = new();
        private readonly GamepadButtonEventArgs buttonEventArgs = new();

        private readonly string guid;
        private short deadzone = 8000;

        static Gamepad()
        {
            for (int i = 0; i < axes.Length; i++)
            {
                axisNames[i] = SDL.GetGamepadStringForAxisS(Helper.ConvertBack(axes[i]));
            }
            for (int i = 0; i < buttons.Length; i++)
            {
                buttonNames[i] = SDL.GetGamepadStringForButtonS(Helper.ConvertBack(buttons[i]));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Gamepad"/> class.
        /// </summary>
        /// <param name="id">The unique identifier for the gamepad.</param>
        public Gamepad(int id)
        {
            controller = SDL.OpenGamepad(id);
            if (controller == null)
            {
                SdlCheckError();
            }

            joystick = SDL.GetGamepadJoystick(controller);
            if (controller == null)
            {
                SdlCheckError();
            }

            this.id = SDL.GetJoystickID(joystick).SdlThrowIfNeg();
            var axes = Enum.GetValues<GamepadAxis>();
            for (int i = 0; i < axes.Length; i++)
            {
                if (SDL.GamepadHasAxis(controller, Helper.ConvertBack(axes[i])))
                {
                    axisStates.Add(axes[i], 0);
                }
            }
            var buttons = Enum.GetValues<GamepadButton>();
            for (int i = 0; i < buttons.Length; i++)
            {
                if (SDL.GamepadHasButton(controller, Helper.ConvertBack(buttons[i])))
                {
                    buttonStates.Add(buttons[i], GamepadButtonState.Up);
                }
            }

            var touchpadCount = SDL.GetNumGamepadTouchpads(controller);
            for (int i = 0; i < touchpadCount; i++)
            {
                touchpads.Add(new(i, controller));
            }

            var sensorTypes = Enum.GetValues<GamepadSensorType>();
            for (int i = 0; i < sensorTypes.Length; i++)
            {
                if (SDL.GamepadHasSensor(controller, Helper.ConvertBack(sensorTypes[i])))
                {
                    sensors.Add(sensorTypes[i], new(controller, sensorTypes[i]));
                }
            }

            int mappingCount;
            var outMappings = SDL.GetGamepadMappings(&mappingCount);
            for (int i = 0; i < mappingCount; i++)
            {
                var mapping = ToStringFromUTF8(outMappings[i]);
                if (mapping == null)
                {
                    SdlCheckError();
                }

                mappings.Add(mapping ?? string.Empty);
            }

            if (SDL.IsJoystickHaptic(joystick))
            {
                haptic = Haptic.OpenFromGamepad(this);
            }

            SdlGuid guid = SDL.GetJoystickGUID(joystick);
            SdlCheckError();
            this.guid = (*(Guid*)&guid).ToString();
        }

        protected uint Props => SDL.GetGamepadProperties(controller);

        /// <summary>
        /// Gets the list of available gamepad axes.
        /// </summary>
        public static IReadOnlyList<GamepadAxis> Axes => axes;

        /// <summary>
        /// Gets the names of available gamepad axes.
        /// </summary>
        public static IReadOnlyList<string> AxisNames => axisNames;

        /// <summary>
        /// Gets the list of available gamepad buttons.
        /// </summary>
        public static IReadOnlyList<GamepadButton> Buttons => buttons;

        /// <summary>
        /// Gets the names of available gamepad buttons.
        /// </summary>
        public static IReadOnlyList<string> ButtonNames => buttonNames;

        /// <summary>
        /// Gets the list of available gamepad sensor types.
        /// </summary>
        public static IReadOnlyList<GamepadSensorType> SensorTypes => sensorTypes;

        /// <summary>
        /// Gets the unique identifier of the gamepad.
        /// </summary>
        public int Id => id;

        /// <summary>
        /// Gets the name of the gamepad.
        /// </summary>
        public string Name
        {
            get
            {
                var name = SDL.GetGamepadNameS(controller);
                if (name == null)
                {
                    SdlCheckError();
                }

                return name ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the vendor ID of the gamepad.
        /// </summary>
        public ushort Vendor => SDL.GetGamepadVendor(controller);

        /// <summary>
        /// Gets the product ID of the gamepad.
        /// </summary>
        public ushort Product => SDL.GetGamepadProduct(controller);

        /// <summary>
        /// Gets the product version of the gamepad.
        /// </summary>
        public ushort ProductVersion => SDL.GetGamepadProductVersion(controller);

        /// <summary>
        /// Gets the serial number of the gamepad.
        /// </summary>
        public string Serial => SDL.GetGamepadSerialS(controller);

        /// <summary>
        /// Gets the globally unique identifier (GUID) of the gamepad.
        /// </summary>
        public string GUID => guid;

        /// <summary>
        /// Gets a value indicating whether the gamepad is currently attached.
        /// </summary>
        public bool IsAttached => SDL.GamepadConnected(controller);

        /// <summary>
        /// Gets a value indicating whether the gamepad has haptic feedback support.
        /// </summary>
        public bool IsHaptic => SDL.IsJoystickHaptic(joystick);

        /// <summary>
        /// Gets a value indicating whether the gamepad has LED support.
        /// </summary>
        public bool HasLED => SDL.HasProperty(Props, SDL.SDL_PROP_GAMEPAD_CAP_RGB_LED_BOOLEAN);

        /// <summary>
        /// Gets a value indicating whether the gamepad has rumble support.
        /// </summary>
        public bool HasRumble => SDL.HasProperty(Props, SDL.SDL_PROP_GAMEPAD_CAP_RUMBLE_BOOLEAN);

        /// <summary>
        /// Gets a value indicating whether the gamepad has trigger rumble support.
        /// </summary>
        public bool HasTriggerRumble => SDL.HasProperty(Props, SDL.SDL_PROP_GAMEPAD_CAP_TRIGGER_RUMBLE_BOOLEAN);

        /// <summary>
        /// Gets the type of the gamepad.
        /// </summary>
        public GamepadType Type => Helper.Convert(SDL.GetGamepadType(controller));

        /// <summary>
        /// Gets or sets the deadzone value for gamepad analog sticks.
        /// </summary>
        public short Deadzone { get => deadzone; set => deadzone = value; }

        /// <summary>
        /// Gets or sets the player index for the gamepad.
        /// </summary>
        public int PlayerIndex { get => SDL.GetGamepadPlayerIndex(controller); set => SDL.SetGamepadPlayerIndex(controller, value); }

        /// <summary>
        /// Gets the controller mapping string of the gamepad.
        /// </summary>
        public string Mapping
        {
            get
            {
                var mapping = SDL.GetGamepadMappingS(controller);
                if (mapping == null)
                {
                    SdlCheckError();
                }

                return mapping ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the current state of all gamepad axes.
        /// </summary>
        public IReadOnlyDictionary<GamepadAxis, short> AxisStates => axisStates;

        /// <summary>
        /// Gets the current state of all gamepad buttons.
        /// </summary>
        public IReadOnlyDictionary<GamepadButton, GamepadButtonState> ButtonStates => buttonStates;

        /// <summary>
        /// Gets the available gamepad sensors and their states.
        /// </summary>
        public IReadOnlyDictionary<GamepadSensorType, GamepadSensor> Sensors => sensors;

        /// <summary>
        /// Gets the list of touchpads associated with the gamepad.
        /// </summary>
        public IReadOnlyList<GamepadTouchpad> Touchpads => touchpads;

        /// <summary>
        /// Gets the list of gamepad mappings.
        /// </summary>
        public IReadOnlyList<string> Mappings => mappings;

        /// <summary>
        /// Gets the haptic feedback interface associated with the gamepad.
        /// </summary>
        public Haptic? Haptic => haptic;

        /// <summary>
        /// Occurs when the gamepad is remapped.
        /// </summary>
        public event GamepadEventHandler<GamepadRemappedEventArgs>? Remapped;

        /// <summary>
        /// Occurs when a gamepad axis motion event is detected.
        /// </summary>
        public event GamepadEventHandler<GamepadAxisMotionEventArgs>? AxisMotion;

        /// <summary>
        /// Occurs when a gamepad button down event is detected.
        /// </summary>
        public event GamepadEventHandler<GamepadButtonEventArgs>? ButtonDown;

        /// <summary>
        /// Occurs when a gamepad button up event is detected.
        /// </summary>
        public event GamepadEventHandler<GamepadButtonEventArgs>? ButtonUp;

        /// <summary>
        /// Checks if the gamepad has the specified button.
        /// </summary>
        /// <param name="button">The gamepad button to check for.</param>
        /// <returns>True if the gamepad has the specified button; otherwise, false.</returns>
        public bool HasButton(GamepadButton button)
        {
            return buttonStates.ContainsKey(button);
        }

        /// <summary>
        /// Checks if the gamepad has the specified axis.
        /// </summary>
        /// <param name="axis">The gamepad axis to check for.</param>
        /// <returns>True if the gamepad has the specified axis; otherwise, false.</returns>
        public bool HasAxis(GamepadAxis axis)
        {
            return axisStates.ContainsKey(axis);
        }

        /// <summary>
        /// Checks if the gamepad has the specified sensor type.
        /// </summary>
        /// <param name="sensor">The gamepad sensor type to check for.</param>
        /// <returns>True if the gamepad has the specified sensor type; otherwise, false.</returns>
        public bool HasSensor(GamepadSensorType sensor)
        {
            return sensors.ContainsKey(sensor);
        }

        /// <summary>
        /// Checks if the specified button on the gamepad is in the "down" state.
        /// </summary>
        /// <param name="button">The gamepad button to check.</param>
        /// <returns>True if the button is in the "down" state; otherwise, false.</returns>
        public bool IsDown(GamepadButton button)
        {
            return buttonStates[button] == GamepadButtonState.Down;
        }

        /// <summary>
        /// Checks if the specified button on the gamepad is in the "up" state.
        /// </summary>
        /// <param name="button">The gamepad button to check.</param>
        /// <returns>True if the button is in the "up" state; otherwise, false.</returns>
        public bool IsUp(GamepadButton button)
        {
            return buttonStates[button] == GamepadButtonState.Up;
        }

        /// <summary>
        /// Activates rumble feedback on the gamepad.
        /// </summary>
        /// <param name="lowFreq">The low-frequency (left) rumble value.</param>
        /// <param name="highFreq">The high-frequency (right) rumble value.</param>
        /// <param name="durationMs">The duration in milliseconds for the rumble.</param>
        public void Rumble(ushort lowFreq, ushort highFreq, uint durationMs)
        {
            SDL.RumbleGamepad(controller, lowFreq, highFreq, durationMs);
        }

        /// <summary>
        /// Activates rumble feedback on the gamepad's triggers.
        /// </summary>
        /// <param name="rightRumble">The rumble value for the right trigger.</param>
        /// <param name="leftRumble">The rumble value for the left trigger.</param>
        /// <param name="durationMs">The duration in milliseconds for the rumble.</param>
        public void RumbleTriggers(ushort rightRumble, ushort leftRumble, uint durationMs)
        {
            SDL.RumbleGamepadTriggers(controller, rightRumble, leftRumble, durationMs);
        }

        /// <summary>
        /// Sets the LED color on the gamepad using a Vector4 representation.
        /// </summary>
        /// <param name="color">The color represented as a Vector4 (red, green, blue, alpha).</param>
        public void SetLED(Vector4 color)
        {
            SDL.SetGamepadLED(controller, (byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255));
        }

        /// <summary>
        /// Sets the LED color on the gamepad using separate RGB color components.
        /// </summary>
        /// <param name="red">The red color component (0-255).</param>
        /// <param name="green">The green color component (0-255).</param>
        /// <param name="blue">The blue color component (0-255).</param>
        public void SetLED(byte red, byte green, byte blue)
        {
            SDL.SetGamepadLED(controller, red, green, blue);
        }

        /// <summary>
        /// Adds a custom gamepad mapping to the gamepad's list of mappings.
        /// </summary>
        /// <param name="mapping">The custom gamepad mapping string to add.</param>
        public void AddMapping(string mapping)
        {
            SDL.AddGamepadMapping(mapping).SdlThrowIfNeg();
            mappings.Add(mapping);
        }

        internal (Gamepad Gamepad, GamepadRemappedEventArgs EventArgs) OnRemapped()
        {
            remappedEventArgs.Handled = false;
            remappedEventArgs.GamepadId = id;
            remappedEventArgs.Mapping = Mapping;
            Remapped?.Invoke(this, remappedEventArgs);
            return (this, remappedEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal (Gamepad Gamepad, GamepadAxisMotionEventArgs EventArgs)? OnAxisMotion(SDLGamepadAxisEvent even)
        {
            var axis = Helper.Convert((SDLGamepadAxis)even.Axis);
            if (Math.Abs((int)even.Value) < deadzone)
            {
                even.Value = 0;
            }

            if (even.Value == axisStates[axis])
            {
                return null;
            }

            axisStates[axis] = even.Value;
            axisMotionEventArgs.Timestamp = even.Timestamp;
            axisMotionEventArgs.Handled = false;
            axisMotionEventArgs.GamepadId = even.Which;
            axisMotionEventArgs.Axis = axis;
            axisMotionEventArgs.Value = even.Value;
            AxisMotion?.Invoke(this, axisMotionEventArgs);
            return (this, axisMotionEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal (Gamepad Gamepad, GamepadButtonEventArgs EventArgs) OnButtonDown(SDLGamepadButtonEvent even)
        {
            var button = Helper.Convert((SDLGamepadButton)even.Button);
            buttonStates[button] = GamepadButtonState.Down;
            buttonEventArgs.Timestamp = even.Timestamp;
            buttonEventArgs.Handled = false;
            buttonEventArgs.GamepadId = even.Which;
            buttonEventArgs.Button = button;
            buttonEventArgs.State = GamepadButtonState.Down;
            ButtonDown?.Invoke(this, buttonEventArgs);
            return (this, buttonEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal (Gamepad Gamepad, GamepadButtonEventArgs EventArgs) OnButtonUp(SDLGamepadButtonEvent even)
        {
            var button = Helper.Convert((SDLGamepadButton)even.Button);
            buttonStates[button] = GamepadButtonState.Up;
            buttonEventArgs.Timestamp = even.Timestamp;
            buttonEventArgs.Handled = false;
            buttonEventArgs.GamepadId = even.Which;
            buttonEventArgs.Button = button;
            buttonEventArgs.State = GamepadButtonState.Up;
            ButtonUp?.Invoke(this, buttonEventArgs);
            return (this, buttonEventArgs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal (GamepadTouchpad Touchpad, GamepadTouchpadEventArgs EventArgs) OnTouchPadDown(SDLGamepadTouchpadEvent even)
        {
            return touchpads[even.Touchpad].OnTouchPadDown(even);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal (GamepadTouchpad Touchpad, GamepadTouchpadMotionEventArgs EventArgs) OnTouchPadMotion(SDLGamepadTouchpadEvent even)
        {
            return touchpads[even.Touchpad].OnTouchPadMotion(even);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal (GamepadTouchpad Touchpad, GamepadTouchpadEventArgs EventArgs) OnTouchPadUp(SDLGamepadTouchpadEvent even)
        {
            return touchpads[even.Touchpad].OnTouchPadUp(even);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal (GamepadSensor Sensor, GamepadSensorUpdateEventArgs EventArgs) OnSensorUpdate(SDLGamepadSensorEvent even)
        {
            return sensors[Helper.Convert((SDLSensorType)even.Sensor)].OnSensorUpdate(even);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            foreach (var sensor in sensors)
            {
                sensor.Value?.Dispose();
            }
            SDL.CloseGamepad(controller);
            SdlCheckError();
        }
    }
}