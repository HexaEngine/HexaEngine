namespace HexaEngine.UI
{
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Core.Windows.Events;

    public interface IInputElement
    {
        public bool Focusable { get; }

        public bool IsEnabled { get; }

        public bool IsKeyboardFocused { get; }

        public bool IsMouseCaptured { get; }

        public bool IsMouseOver { get; }

        public event EventHandler<KeyboardCharEventArgs>? TextInput;

        public event EventHandler<KeyboardEventArgs>? KeyDown;

        public event EventHandler<KeyboardEventArgs>? KeyUp;

        public event EventHandler<MouseButtonEventArgs>? MouseDown;

        public event EventHandler<MouseButtonEventArgs>? MouseUp;

        public event EventHandler<MouseButtonEventArgs>? DoubleClick;

        public event EventHandler<MouseEventArgs>? MouseLeave;

        public event EventHandler<MouseEventArgs>? MouseEnter;

        public event EventHandler<MouseMotionEventArgs>? MouseMove;

        public event EventHandler<MouseWheelEventArgs>? MouseWheel;

        public event EventHandler<FocusGainedEventArgs>? GotFocus;

        public event EventHandler<FocusLostEventArgs>? LostFocus;

        public void RaiseEvent(RoutedEventArgs e);

        public void Focus();
    }
}