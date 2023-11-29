namespace HexaEngine.Input.Events
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;

    public struct JoystickButtonEvent(JoystickButtonEventArgs eventArgs)
    {
        public int Button = eventArgs.Button;
        public JoystickButtonState State = eventArgs.State;
    }
}