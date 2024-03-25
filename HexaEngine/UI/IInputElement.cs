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

        public event RoutedEventHandler<TextInputEventArgs> TextInput;

        public event RoutedEventHandler<KeyboardEventArgs> KeyDown;

        public event RoutedEventHandler<KeyboardEventArgs> KeyUp;

        public event RoutedEventHandler<MouseButtonEventArgs> MouseDown;

        public event RoutedEventHandler<MouseButtonEventArgs> MouseUp;

        public event RoutedEventHandler<MouseEventArgs> MouseLeave;

        public event RoutedEventHandler<MouseEventArgs> MouseEnter;

        public event RoutedEventHandler<MouseMoveEventArgs> MouseMove;

        public event RoutedEventHandler<MouseWheelEventArgs> MouseWheel;

        public event RoutedEventHandler<FocusGainedEventArgs> GotFocus;

        public event RoutedEventHandler<FocusLostEventArgs> LostFocus;

        public void RaiseEvent(RoutedEventArgs e);

        public void Focus();
    }
}