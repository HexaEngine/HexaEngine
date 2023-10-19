namespace HexaEngine.Input
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;

    public struct GamepadButtonEvent(GamepadButtonEventArgs eventArgs)
    {
        public GamepadButton Button = eventArgs.Button;
        public GamepadButtonState State = eventArgs.State;
    }
}