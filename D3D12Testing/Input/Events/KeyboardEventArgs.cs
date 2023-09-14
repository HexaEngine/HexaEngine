namespace D3D12Testing.Input.Events
{
    using D3D12Testing.Input;
    using Silk.NET.SDL;

    public class KeyboardEventArgs : EventArgs
    {
        public KeyboardEventArgs()
        {
        }

        public KeyboardEventArgs(Key keyCode, KeyState keyState, Scancode scancode)
        {
            KeyCode = keyCode;
            KeyState = keyState;
            Scancode = scancode;
        }

        public Key KeyCode { get; internal set; }

        public KeyState KeyState { get; internal set; }

        public Scancode Scancode { get; internal set; }
    }
}