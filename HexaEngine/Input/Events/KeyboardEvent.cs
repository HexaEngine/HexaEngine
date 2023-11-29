namespace HexaEngine.Input.Events
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;

    public struct KeyboardEvent(KeyboardEventArgs eventArgs)
    {
        public Key Key = eventArgs.KeyCode;
        public KeyState State = eventArgs.State;
        public ScanCode ScanCode = eventArgs.ScanCode;
    }
}