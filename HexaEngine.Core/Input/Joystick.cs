namespace HexaEngine.Core.Input
{
    using Hexa.NET.SDL2;
    using HexaEngine.Core.Input.Events;
    using System.Numerics;
    using System.Text;
    using static Extensions.SdlErrorHandlingExtensions;

    /// <summary>
    /// Represents a generic delegate for handling joystick events.
    /// </summary>
    /// <typeparam name="TEventArgs">The type of event-specific data or argument.</typeparam>
    /// <param name="sender">The object that raises the event.</param>
    /// <param name="e">The event-specific data or argument.</param>
    public delegate void JoystickEventHandler<TEventArgs>(Joystick sender, TEventArgs e);

    /// <summary>
    /// Represents a joystick input device.
    /// </summary>
    public unsafe class Joystick : IDisposable
    {
        private readonly int id;
        internal readonly SDLJoystick* joystick;
        private readonly Dictionary<int, short> axes = new();
        private readonly Dictionary<int, (int, int)> balls = new();
        private readonly Dictionary<int, JoystickButtonState> buttons = new();
        private readonly Dictionary<int, JoystickHatState> hats = new();

        private readonly JoystickAxisMotionEventArgs axisMotionEventArgs = new();
        private readonly JoystickBallMotionEventArgs ballMotionEventArgs = new();
        private readonly JoystickButtonEventArgs buttonEventArgs = new();
        private readonly JoystickHatMotionEventArgs hatMotionEventArgs = new();

        private readonly string guid;
        private short deadzone = 8000;

        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="Joystick"/> class with the specified ID.
        /// </summary>
        /// <param name="id">The ID of the joystick.</param>
        public Joystick(int id)
        {
            this.id = id;
            joystick = SDL.JoystickOpen(id);
            if (joystick == null)
            {
                SdlCheckError();
            }

            var axisCount = SDL.JoystickNumAxes(joystick);
            for (int i = 0; i < axisCount; i++)
            {
                short state;
                SDL.JoystickGetAxisInitialState(joystick, i, &state);
                axes.Add(i, state);
            }

            var ballCount = SDL.JoystickNumBalls(joystick);
            for (int i = 0; i < ballCount; i++)
            {
                int x;
                int y;
                SDL.JoystickGetBall(joystick, i, &x, &y);
                balls.Add(i, new(x, y));
            }

            var buttonCount = SDL.JoystickNumButtons(joystick);
            for (int i = 0; i < buttonCount; i++)
            {
                var state = (JoystickButtonState)SDL.JoystickGetButton(joystick, i);
                buttons.Add(i, state);
            }

            var hatCount = SDL.JoystickNumHats(joystick);
            for (int i = 0; i < hatCount; i++)
            {
                var state = (JoystickHatState)SDL.JoystickGetHat(joystick, i);
                hats.Add(i, state);
            }

            var guid = SDL.JoystickGetGUID(joystick);
            SdlCheckError();
            var buffer = AllocT<byte>(33);
            SDL.JoystickGetGUIDString(guid, buffer, 33);
            var size = StrLen(buffer);
            var value = Encoding.ASCII.GetString(buffer, size);
            Free(buffer);
            this.guid = value;
        }

        /// <summary>
        /// Gets the ID of the joystick.
        /// </summary>
        public int Id => id;

        /// <summary>
        /// Gets the name of the joystick.
        /// </summary>
        public string Name
        {
            get
            {
                var name = SDL.JoystickNameS(joystick);
                if (name == null)
                {
                    SdlCheckError();
                }

                return name ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the vendor ID of the joystick.
        /// </summary>
        public ushort Vendor => SDL.JoystickGetVendor(joystick);

        /// <summary>
        /// Gets the product ID of the joystick.
        /// </summary>
        public ushort Product => SDL.JoystickGetProduct(joystick);

        /// <summary>
        /// Gets the product version of the joystick.
        /// </summary>
        public ushort ProductVersion => SDL.JoystickGetProductVersion(joystick);

        /// <summary>
        /// Gets the serial number of the joystick.
        /// </summary>
        public string Serial => SDL.JoystickGetSerialS(joystick);

        /// <summary>
        /// Gets the unique identifier (GUID) of the joystick.
        /// </summary>
        public string Guid => guid;

        /// <summary>
        /// Gets a value indicating whether the joystick is attached and available.
        /// </summary>
        public bool IsAttached => SDL.JoystickGetAttached(joystick) == SDLBool.True;

        /// <summary>
        /// Gets a value indicating whether the joystick is a virtual joystick.
        /// </summary>
        public bool IsVirtual => SDL.JoystickIsVirtual(id) == SDLBool.True;

        /// <summary>
        /// Gets a value indicating whether the joystick has LED support.
        /// </summary>
        public bool HasLED => SDL.JoystickHasLED(joystick) == SDLBool.True;

        /// <summary>
        /// Gets the type of the joystick.
        /// </summary>
        public JoystickType Type => Helper.Convert(SDL.JoystickGetType(joystick));

        /// <summary>
        /// Gets or sets the deadzone value for joystick axes.
        /// </summary>
        public short Deadzone { get => deadzone; set => deadzone = value; }

        /// <summary>
        /// Gets or sets the player index assigned to the joystick.
        /// </summary>
        public int PlayerIndex { get => SDL.JoystickGetPlayerIndex(joystick); set => SDL.JoystickSetPlayerIndex(joystick, value); }

        /// <summary>
        /// Gets the current power level of the joystick.
        /// </summary>
        public JoystickPowerLevel PowerLevel => Helper.Convert(SDL.JoystickCurrentPowerLevel(joystick));

        /// <summary>
        /// Gets the state of joystick axes as a dictionary with axis ID and their values.
        /// </summary>
        public IReadOnlyDictionary<int, short> Axes => axes;

        /// <summary>
        /// Gets the state of joystick balls as a dictionary with ball ID and their relative motion values (X, Y).
        /// </summary>
        public IReadOnlyDictionary<int, (int, int)> Balls => balls;

        /// <summary>
        /// Gets the state of joystick buttons as a dictionary with button ID and their states.
        /// </summary>
        public IReadOnlyDictionary<int, JoystickButtonState> Buttons => buttons;

        /// <summary>
        /// Gets the state of joystick hats as a dictionary with hat ID and their states.
        /// </summary>
        public IReadOnlyDictionary<int, JoystickHatState> Hats => hats;

        /// <summary>
        /// Occurs when a joystick axis is moved.
        /// </summary>
        public event JoystickEventHandler<JoystickAxisMotionEventArgs>? AxisMotion;

        /// <summary>
        /// Occurs when a joystick ball is moved.
        /// </summary>
        public event JoystickEventHandler<JoystickBallMotionEventArgs>? BallMotion;

        /// <summary>
        /// Occurs when a joystick button is pressed.
        /// </summary>
        public event JoystickEventHandler<JoystickButtonEventArgs>? ButtonDown;

        /// <summary>
        /// Occurs when a joystick button is released.
        /// </summary>
        public event JoystickEventHandler<JoystickButtonEventArgs>? ButtonUp;

        /// <summary>
        /// Occurs when a joystick hat is moved.
        /// </summary>
        public event JoystickEventHandler<JoystickHatMotionEventArgs>? HatMotion;

        /// <summary>
        /// Initiates rumble feedback on the joystick using a low-frequency and high-frequency effect.
        /// </summary>
        /// <param name="lowFreq">The low-frequency effect strength (0 to 0xFFFF).</param>
        /// <param name="highFreq">The high-frequency effect strength (0 to 0xFFFF).</param>
        /// <param name="durationMs">The duration of the rumble effect in milliseconds.</param>
        public void Rumble(ushort lowFreq, ushort highFreq, uint durationMs)
        {
            SDL.JoystickRumble(joystick, lowFreq, highFreq, durationMs);
        }

        /// <summary>
        /// Initiates separate rumble feedback for the left and right triggers of the joystick.
        /// </summary>
        /// <param name="left">The strength of the rumble effect for the left trigger (0 to 0xFFFF).</param>
        /// <param name="right">The strength of the rumble effect for the right trigger (0 to 0xFFFF).</param>
        /// <param name="durationMs">The duration of the rumble effect in milliseconds.</param>
        public void RumbleTriggers(ushort left, ushort right, uint durationMs)
        {
            SDL.JoystickRumbleTriggers(joystick, left, right, durationMs);
        }

        /// <summary>
        /// Sets the LED color of the joystick using a Vector4 representing the color (RGBA).
        /// </summary>
        /// <param name="color">A Vector4 specifying the LED color (red, green, blue, and alpha components).</param>
        public void SetLED(Vector4 color)
        {
            SDL.JoystickSetLED(joystick, (byte)(color.X * 255), (byte)(color.Y * 255), (byte)(color.Z * 255));
        }

        /// <summary>
        /// Sets the LED color of the joystick using individual red, green, and blue color components.
        /// </summary>
        /// <param name="red">The red color component (0 to 255).</param>
        /// <param name="green">The green color component (0 to 255).</param>
        /// <param name="blue">The blue color component (0 to 255).</param>
        public void SetLED(byte red, byte green, byte blue)
        {
            SDL.JoystickSetLED(joystick, red, green, blue);
        }

        internal (Joystick Joystick, JoystickAxisMotionEventArgs AxisMotionEventArgs)? OnAxisMotion(SDLJoyAxisEvent even)
        {
            if (Math.Abs((int)even.Value) < deadzone)
            {
                even.Value = 0;
            }

            if (even.Value == axes[even.Axis])
            {
                return null;
            }

            axes[even.Axis] = even.Value;
            axisMotionEventArgs.Timestamp = even.Timestamp;
            axisMotionEventArgs.Handled = false;
            axisMotionEventArgs.JoystickId = even.Which;
            axisMotionEventArgs.Axis = even.Axis;
            axisMotionEventArgs.Value = even.Value;
            AxisMotion?.Invoke(this, axisMotionEventArgs);
            return (this, axisMotionEventArgs);
        }

        internal (Joystick Joystick, JoystickBallMotionEventArgs BallMotionEventArgs) OnBallMotion(SDLJoyBallEvent even)
        {
            balls[even.Ball] = (even.Xrel, even.Yrel);
            ballMotionEventArgs.Timestamp = even.Timestamp;
            ballMotionEventArgs.Handled = false;
            ballMotionEventArgs.JoystickId = even.Which;
            ballMotionEventArgs.Ball = even.Ball;
            ballMotionEventArgs.RelX = even.Xrel;
            ballMotionEventArgs.RelY = even.Yrel;
            BallMotion?.Invoke(this, ballMotionEventArgs);
            return (this, ballMotionEventArgs);
        }

        internal (Joystick Joystick, JoystickButtonEventArgs ButtonEventArgs) OnButtonDown(SDLJoyButtonEvent even)
        {
            buttons[even.Button] = JoystickButtonState.Down;
            buttonEventArgs.Timestamp = even.Timestamp;
            buttonEventArgs.Handled = false;
            buttonEventArgs.JoystickId = even.Which;
            buttonEventArgs.Button = even.Button;
            buttonEventArgs.State = JoystickButtonState.Down;
            ButtonDown?.Invoke(this, buttonEventArgs);
            return (this, buttonEventArgs);
        }

        internal (Joystick Joystick, JoystickButtonEventArgs ButtonEventArgs) OnButtonUp(SDLJoyButtonEvent even)
        {
            buttons[even.Button] = JoystickButtonState.Up;
            buttonEventArgs.Timestamp = even.Timestamp;
            buttonEventArgs.Handled = false;
            buttonEventArgs.JoystickId = even.Which;
            buttonEventArgs.Button = even.Button;
            buttonEventArgs.State = JoystickButtonState.Up;
            ButtonUp?.Invoke(this, buttonEventArgs);
            return (this, buttonEventArgs);
        }

        internal (Joystick Joystick, JoystickHatMotionEventArgs HatMotionEventArgs) OnHatMotion(SDLJoyHatEvent even)
        {
            hats[even.Hat] = (JoystickHatState)even.Value;
            hatMotionEventArgs.Timestamp = even.Timestamp;
            hatMotionEventArgs.Handled = false;
            hatMotionEventArgs.JoystickId = even.Which;
            hatMotionEventArgs.Hat = even.Hat;
            hatMotionEventArgs.State = (JoystickHatState)even.Value;
            HatMotion?.Invoke(this, hatMotionEventArgs);
            return (this, hatMotionEventArgs);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                SDL.JoystickClose(joystick);
                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}