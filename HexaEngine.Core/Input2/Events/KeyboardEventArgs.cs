namespace HexaEngine.Core.Input2.Events
{
    using HexaEngine.Core.Input2;

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