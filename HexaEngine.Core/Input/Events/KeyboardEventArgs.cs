namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Input;
    using Silk.NET.SDL;

    public class KeyboardEventArgs : EventArgs
    {
        public KeyboardEventArgs(KeyCode keyCode, KeyState keyState)
        {
            KeyCode = keyCode;
            KeyState = keyState;
        }

        public KeyCode KeyCode { get; }

        public KeyState KeyState { get; }
    }
}