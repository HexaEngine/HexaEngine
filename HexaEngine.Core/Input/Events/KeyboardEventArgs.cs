namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Windows.Events;

    public class KeyboardEventArgs : RoutedEventArgs
    {
        public KeyboardEventArgs()
        {
        }

        public KeyboardEventArgs(Key keyCode, KeyState keyState, ScanCode scancode)
        {
            KeyCode = keyCode;
            State = keyState;
            ScanCode = scancode;
        }

        public Key KeyCode { get; internal set; }

        public KeyState State { get; internal set; }

        public ScanCode ScanCode { get; internal set; }
    }
}