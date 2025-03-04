namespace HexaEngine.Core.Input
{
    using Hexa.NET.SDL3;

    /// <summary>
    /// Represents a haptic feedback device that can provide force feedback sensations.
    /// </summary>
    public unsafe class Haptic
    {
        private readonly uint id;
        private readonly SDLHaptic* haptic;

        private Haptic(SDLHaptic* haptic)
        {
            this.haptic = haptic;
            id = SDL.GetHapticID(haptic);
        }

        /// <summary>
        /// Gets the unique identifier of the haptic feedback device.
        /// </summary>
        public uint Id => id;

        /// <summary>
        /// Gets the name of the haptic feedback device.
        /// </summary>
        public string Name => SDL.GetHapticNameS(haptic);

        /// <summary>
        /// Gets the number of axes (directions) supported by the haptic feedback device.
        /// </summary>
        public int AxesCount => SDL.GetNumHapticAxes(haptic);

        /// <summary>
        /// Gets the number of effects (force feedback patterns) that can be stored and played by the haptic feedback device.
        /// </summary>
        public int EffectsCount => SDL.GetMaxHapticEffects(haptic);

        /// <summary>
        /// Gets the number of effects max playing on the haptic feedback device.
        /// </summary>
        public int EffectsPlayingCount => SDL.GetMaxHapticEffectsPlaying(haptic);

        /// <summary>
        /// Gets a value indicating whether the haptic feedback device supports rumble (vibration) effects.
        /// </summary>
        public bool RumbleSupported => SDL.HapticRumbleSupported(haptic);

        /// <summary>
        /// Gets a set of flags that specify the types of haptic effects supported by the device.
        /// </summary>
        public HapticEffectFlags EffectsSupported => (HapticEffectFlags)SDL.GetHapticFeatures(haptic);

        /// <summary>
        /// Opens a haptic device associated with a gamepad and returns a <see cref="Haptic"/> instance for it.
        /// </summary>
        /// <param name="gamepad">The gamepad associated with the haptic device.</param>
        /// <returns>A <see cref="Haptic"/> instance representing the haptic feedback device.</returns>
        public static Haptic OpenFromGamepad(Gamepad gamepad)
        {
            return new(SDL.OpenHapticFromJoystick(gamepad.joystick));
        }

        /// <summary>
        /// Opens a haptic device associated with a joystick and returns a <see cref="Haptic"/> instance for it.
        /// </summary>
        /// <param name="joystick">The joystick associated with the haptic device.</param>
        /// <returns>A <see cref="Haptic"/> instance representing the haptic feedback device.</returns>
        public static Haptic OpenFromJoystick(Joystick joystick)
        {
            return new(SDL.OpenHapticFromJoystick(joystick.joystick));
        }

        /// <summary>
        /// Opens a haptic device associated with a mouse and returns a <see cref="Haptic"/> instance for it.
        /// </summary>
        /// <returns>A <see cref="Haptic"/> instance representing the haptic feedback device.</returns>
        public static Haptic OpenFromMouse()
        {
            return new(SDL.OpenHapticFromMouse());
        }

        /// <summary>
        /// Opens a haptic device by index and returns a <see cref="Haptic"/> instance for it.
        /// </summary>
        /// <param name="index">The index of the haptic device to open.</param>
        /// <returns>A <see cref="Haptic"/> instance representing the haptic feedback device.</returns>
        public static Haptic OpenFromIndex(uint index)
        {
            return new(SDL.OpenHaptic(index));
        }
    }
}