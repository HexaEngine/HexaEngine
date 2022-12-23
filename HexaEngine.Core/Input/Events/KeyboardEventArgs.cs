namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Input;

    public class KeyboardEventArgs : EventArgs
    {
        public KeyboardEventArgs()
        {
        }

        public KeyboardEventArgs(KeyCode keyCode, KeyState keyState)
        {
            KeyCode = keyCode;
            KeyState = keyState;
        }

        public KeyCode KeyCode { get; internal set; }

        public KeyState KeyState { get; internal set; }
    }
}