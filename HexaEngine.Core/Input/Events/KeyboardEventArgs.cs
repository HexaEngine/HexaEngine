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

        public KeyCode KeyCode { get; set; }

        public KeyState KeyState { get; set; }
    }
}