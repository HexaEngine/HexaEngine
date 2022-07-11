namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Input;

    public class MouseButtonEventArgs : EventArgs
    {
        public MouseButtonEventArgs(MouseButton mouseButton, KeyState keyState, int clicks)
        {
            MouseButton = mouseButton;
            KeyState = keyState;
            Clicks = clicks;
        }

        public MouseButton MouseButton { get; }

        public KeyState KeyState { get; }

        public int Clicks { get; }
    }
}