namespace HexaEngine.Core.Input2.Events
{
    using HexaEngine.Core.Input2;

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