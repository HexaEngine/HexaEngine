namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Windows.Events;

    public class GamepadEventArgs : RoutedEventArgs
    {
        public int GamepadId { get; internal set; }

        public Gamepad Gamepad => Gamepads.GetById(GamepadId);
    }
}