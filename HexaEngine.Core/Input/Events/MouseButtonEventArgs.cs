namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Input;

    public class MouseButtonEventArgs : EventArgs
    {
        public MouseButton MouseButton { get; internal set; }

        public KeyState KeyState { get; internal set; }

        public int Clicks { get; internal set; }
    }
}