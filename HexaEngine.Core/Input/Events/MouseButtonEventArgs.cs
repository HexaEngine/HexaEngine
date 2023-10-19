namespace HexaEngine.Core.Input.Events
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Windows.Events;

    public class MouseEventArgs : RoutedEventArgs
    {
        public uint MouseId { get; internal set; }
    }

    public class MouseButtonEventArgs : MouseEventArgs
    {
        public MouseButton Button { get; internal set; }

        public MouseButtonState State { get; internal set; }

        public int Clicks { get; internal set; }
    }
}