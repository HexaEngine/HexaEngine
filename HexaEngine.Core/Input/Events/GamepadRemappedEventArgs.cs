namespace HexaEngine.Core.Input.Events
{
    /// <summary>
    /// Provides data for a gamepad remapping event.
    /// </summary>
    public class GamepadRemappedEventArgs : GamepadEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadRemappedEventArgs"/> class with an empty mapping.
        /// </summary>
        public GamepadRemappedEventArgs()
        {
            Mapping = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GamepadRemappedEventArgs"/> class with a specified mapping.
        /// </summary>
        /// <param name="mapping">The new mapping for the gamepad.</param>
        public GamepadRemappedEventArgs(string mapping)
        {
            Mapping = mapping;
        }

        /// <summary>
        /// Gets the new mapping for the gamepad after remapping.
        /// </summary>
        public string Mapping { get; internal set; }
    }
}