namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Input;

    public class KeyboardEventArgs : EventArgs
    {
        public KeyboardEventArgs(Keys keyCode, KeyState keyState)
        {
            KeyCode = keyCode;
            KeyState = keyState;
        }

        public Keys KeyCode { get; }

        public KeyState KeyState { get; }
    }
}