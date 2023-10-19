namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Windows.Events;

    public class JoystickEventArgs : RoutedEventArgs
    {
        public int JoystickId { get; internal set; }

        public Joystick Joystick => Joysticks.GetById(JoystickId);
    }
}