namespace HexaEngine.UI
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Core.Windows.Events;
    using System.Diagnostics;

    public partial class UIElement : IInputElement
    {
        private bool isMouseOver;

        static UIElement()
        {
            TextInputEvent.AddClassHandler<UIElement>((x, args) => x?.OnTextInput(args));
            KeyUpEvent.AddClassHandler<UIElement>((x, args) => x?.OnKeyUp(args));
            KeyDownEvent.AddClassHandler<UIElement>((x, args) => x?.OnKeyDown(args));
            MouseUpEvent.AddClassHandler<UIElement>(HandleMouseUp);
            MouseDownEvent.AddClassHandler<UIElement>(HandleMouseDown);
            MouseLeftButtonUpEvent.AddClassHandler<UIElement>((x, args) => x?.OnMouseLeftButtonUp(args));
            MouseLeftButtonDownEvent.AddClassHandler<UIElement>((x, args) => x?.OnMouseLeftButtonDown(args));
            MouseRightButtonUpEvent.AddClassHandler<UIElement>((x, args) => x?.OnMouseRightButtonUp(args));
            MouseRightButtonDownEvent.AddClassHandler<UIElement>((x, args) => x?.OnMouseRightButtonDown(args));
            MouseEnterEvent.AddClassHandler<UIElement>(HandleMouseEnter);
            MouseLeaveEvent.AddClassHandler<UIElement>(HandleMouseLeave);
            MouseMoveEvent.AddClassHandler<UIElement>((x, args) => x?.OnMouseMove(args));
            MouseWheelEvent.AddClassHandler<UIElement>((x, args) => x?.OnMouseWheel(args));
            GotFocusEvent.AddClassHandler<UIElement>((x, args) => x?.OnGotFocus(args));
            LostFocusEvent.AddClassHandler<UIElement>((x, args) => x?.OnLostFocus(args));
            TouchEnterEvent.AddClassHandler<UIElement>((x, args) => x?.OnTouchEnter(args));
            TouchLeaveEvent.AddClassHandler<UIElement>((x, args) => x?.OnTouchLeave(args));
            TouchMoveEvent.AddClassHandler<UIElement>((x, args) => x?.OnTouchMove(args));
            TouchDownEvent.AddClassHandler<UIElement>((x, args) => x?.OnTouchDown(args));
            TouchUpEvent.AddClassHandler<UIElement>((x, args) => x?.OnTouchUp(args));
        }

        public static readonly RoutedEvent<TextInputEventArgs> TextInputEvent = EventManager.Register<UIElement, TextInputEventArgs>(nameof(TextInput), RoutingStrategy.Direct);

        public event RoutedEventHandler<TextInputEventArgs> TextInput
        {
            add => AddHandler(TextInputEvent, value);
            remove => RemoveHandler(TextInputEvent, value);
        }

        public static readonly RoutedEvent<KeyboardEventArgs> KeyUpEvent = EventManager.Register<UIElement, KeyboardEventArgs>(nameof(KeyUp), RoutingStrategy.Direct);

        public event RoutedEventHandler<KeyboardEventArgs> KeyUp
        {
            add => AddHandler(KeyUpEvent, value);
            remove => RemoveHandler(KeyUpEvent, value);
        }

        public static readonly RoutedEvent<KeyboardEventArgs> KeyDownEvent = EventManager.Register<UIElement, KeyboardEventArgs>(nameof(KeyDown), RoutingStrategy.Direct);

        public event RoutedEventHandler<KeyboardEventArgs> KeyDown
        {
            add => AddHandler(KeyDownEvent, value);
            remove => RemoveHandler(KeyDownEvent, value);
        }

        public static readonly RoutedEvent<MouseButtonEventArgs> MouseUpEvent = EventManager.Register<UIElement, MouseButtonEventArgs>(nameof(MouseUp), RoutingStrategy.Direct);

        public event RoutedEventHandler<MouseButtonEventArgs> MouseUp
        {
            add => AddHandler(MouseUpEvent, value);
            remove => RemoveHandler(MouseUpEvent, value);
        }

        public static readonly RoutedEvent<MouseButtonEventArgs> MouseDownEvent = EventManager.Register<UIElement, MouseButtonEventArgs>(nameof(MouseDown), RoutingStrategy.Direct);

        public event RoutedEventHandler<MouseButtonEventArgs> MouseDown
        {
            add => AddHandler(MouseDownEvent, value);
            remove => RemoveHandler(MouseDownEvent, value);
        }

        public static readonly RoutedEvent<MouseButtonEventArgs> MouseLeftButtonUpEvent = EventManager.Register<UIElement, MouseButtonEventArgs>(nameof(MouseLeftButtonUp), RoutingStrategy.Direct);

        public event RoutedEventHandler<MouseButtonEventArgs> MouseLeftButtonUp
        {
            add => AddHandler(MouseLeftButtonUpEvent, value);
            remove => RemoveHandler(MouseLeftButtonUpEvent, value);
        }

        public static readonly RoutedEvent<MouseButtonEventArgs> MouseRightButtonDownEvent = EventManager.Register<UIElement, MouseButtonEventArgs>(nameof(MouseRightButtonDown), RoutingStrategy.Direct);

        public event RoutedEventHandler<MouseButtonEventArgs> MouseRightButtonDown
        {
            add => AddHandler(MouseRightButtonDownEvent, value);
            remove => RemoveHandler(MouseRightButtonDownEvent, value);
        }

        public static readonly RoutedEvent<MouseButtonEventArgs> MouseRightButtonUpEvent = EventManager.Register<UIElement, MouseButtonEventArgs>(nameof(MouseRightButtonUp), RoutingStrategy.Direct);

        public event RoutedEventHandler<MouseButtonEventArgs> MouseRightButtonUp
        {
            add => AddHandler(MouseRightButtonUpEvent, value);
            remove => RemoveHandler(MouseRightButtonUpEvent, value);
        }

        public static readonly RoutedEvent<MouseButtonEventArgs> MouseLeftButtonDownEvent = EventManager.Register<UIElement, MouseButtonEventArgs>(nameof(MouseLeftButtonDown), RoutingStrategy.Direct);

        public event RoutedEventHandler<MouseButtonEventArgs> MouseLeftButtonDown
        {
            add => AddHandler(MouseLeftButtonDownEvent, value);
            remove => RemoveHandler(MouseLeftButtonDownEvent, value);
        }

        public static readonly RoutedEvent<MouseEventArgs> MouseEnterEvent = EventManager.Register<UIElement, MouseEventArgs>(nameof(MouseEnter), RoutingStrategy.Tunnel);

        public event RoutedEventHandler<MouseEventArgs> MouseEnter
        {
            add => AddHandler(MouseEnterEvent, value);
            remove => RemoveHandler(MouseEnterEvent, value);
        }

        public static readonly RoutedEvent<MouseEventArgs> MouseLeaveEvent = EventManager.Register<UIElement, MouseEventArgs>(nameof(MouseLeave), RoutingStrategy.Bubble);

        public event RoutedEventHandler<MouseEventArgs> MouseLeave
        {
            add => AddHandler(MouseLeaveEvent, value);
            remove => RemoveHandler(MouseLeaveEvent, value);
        }

        public static readonly RoutedEvent<MouseMoveEventArgs> MouseMoveEvent = EventManager.Register<UIElement, MouseMoveEventArgs>(nameof(MouseMove), RoutingStrategy.Tunnel);

        public event RoutedEventHandler<MouseMoveEventArgs> MouseMove
        {
            add => AddHandler(MouseMoveEvent, value);
            remove => RemoveHandler(MouseMoveEvent, value);
        }

        public static readonly RoutedEvent<MouseWheelEventArgs> MouseWheelEvent = EventManager.Register<UIElement, MouseWheelEventArgs>(nameof(MouseWheel), RoutingStrategy.Tunnel);

        public event RoutedEventHandler<MouseWheelEventArgs> MouseWheel
        {
            add => AddHandler(MouseWheelEvent, value);
            remove => RemoveHandler(MouseWheelEvent, value);
        }

        public static readonly RoutedEvent<FocusGainedEventArgs> GotFocusEvent = EventManager.Register<UIElement, FocusGainedEventArgs>(nameof(GotFocus), RoutingStrategy.Direct);

        public event RoutedEventHandler<FocusGainedEventArgs> GotFocus
        {
            add => AddHandler(GotFocusEvent, value);
            remove => RemoveHandler(GotFocusEvent, value);
        }

        public static readonly RoutedEvent<FocusLostEventArgs> LostFocusEvent = EventManager.Register<UIElement, FocusLostEventArgs>(nameof(LostFocus), RoutingStrategy.Direct);

        public event RoutedEventHandler<FocusLostEventArgs> LostFocus
        {
            add => AddHandler(LostFocusEvent, value);
            remove => RemoveHandler(LostFocusEvent, value);
        }

        public static readonly RoutedEvent<TouchEventArgs> TouchEnterEvent = EventManager.Register<UIElement, TouchEventArgs>(nameof(TouchEnter), RoutingStrategy.Tunnel);

        public event RoutedEventHandler<TouchEventArgs> TouchEnter
        {
            add => AddHandler(TouchEnterEvent, value);
            remove => RemoveHandler(TouchEnterEvent, value);
        }

        public static readonly RoutedEvent<TouchEventArgs> TouchLeaveEvent = EventManager.Register<UIElement, TouchEventArgs>(nameof(TouchLeave), RoutingStrategy.Bubble);

        public event RoutedEventHandler<TouchEventArgs> TouchLeave
        {
            add => AddHandler(TouchLeaveEvent, value);
            remove => RemoveHandler(TouchLeaveEvent, value);
        }

        public static readonly RoutedEvent<TouchMoveEventArgs> TouchMoveEvent = EventManager.Register<UIElement, TouchMoveEventArgs>(nameof(TouchMove), RoutingStrategy.Tunnel);

        public event RoutedEventHandler<TouchMoveEventArgs> TouchMove
        {
            add => AddHandler(TouchMoveEvent, value);
            remove => RemoveHandler(TouchMoveEvent, value);
        }

        public static readonly RoutedEvent<TouchEventArgs> TouchDownEvent = EventManager.Register<UIElement, TouchEventArgs>(nameof(TouchDown), RoutingStrategy.Direct);

        public event RoutedEventHandler<TouchEventArgs> TouchDown
        {
            add => AddHandler(TouchDownEvent, value);
            remove => RemoveHandler(TouchDownEvent, value);
        }

        public static readonly RoutedEvent<TouchEventArgs> TouchUpEvent = EventManager.Register<UIElement, TouchEventArgs>(nameof(TouchUp), RoutingStrategy.Direct);

        public event RoutedEventHandler<TouchEventArgs> TouchUp
        {
            add => AddHandler(TouchUpEvent, value);
            remove => RemoveHandler(TouchUpEvent, value);
        }

        public static IInputElement? Focused { get; internal set; }

        public static IInputElement? Captured { get; internal set; }

        public bool IsKeyboardFocused
        {
            get => Focused == this;
        }

        public bool IsMouseCaptured
        {
            get => Captured == this;
        }

        public bool IsMouseOver => isMouseOver;

        private static void HandleMouseUp(UIElement? sender, MouseButtonEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            sender.OnMouseUp(e);

            switch (e.Button)
            {
                case MouseButton.Left:
                    e.RoutedEvent = MouseLeftButtonUpEvent;
                    sender.RaiseEvent(e);
                    break;

                case MouseButton.Right:
                    e.RoutedEvent = MouseRightButtonUpEvent;
                    sender.RaiseEvent(e);
                    break;
            }
        }

        private static void HandleMouseDown(UIElement? sender, MouseButtonEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            if (e.Button == MouseButton.Left)
            {
                sender.Focus();
            }

            sender.OnMouseDown(e);

            switch (e.Button)
            {
                case MouseButton.Left:
                    e.RoutedEvent = MouseLeftButtonDownEvent;
                    sender.RaiseEvent(e);
                    break;

                case MouseButton.Right:
                    e.RoutedEvent = MouseRightButtonDownEvent;
                    sender.RaiseEvent(e);
                    break;
            }
        }

        private static void HandleMouseEnter(UIElement? sender, MouseEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            if (sender.isMouseOver)
            {
                return;
            }

            sender.isMouseOver = true;
            sender.OnMouseEnter(e);
        }

        private static void HandleMouseLeave(UIElement? sender, MouseEventArgs e)
        {
            if (sender == null)
            {
                return;
            }

            if (!sender.isMouseOver)
            {
                return;
            }

            sender.isMouseOver = false;
            sender.OnMouseLeave(e);
        }

        protected virtual void OnGotFocus(FocusGainedEventArgs args)
        {
        }

        protected virtual void OnLostFocus(FocusLostEventArgs args)
        {
        }

        protected virtual void OnTextInput(TextInputEventArgs args)
        {
        }

        protected virtual void OnKeyDown(KeyboardEventArgs args)
        {
        }

        protected virtual void OnKeyUp(KeyboardEventArgs args)
        {
        }

        protected virtual void OnMouseDown(MouseButtonEventArgs args)
        {
        }

        protected virtual void OnMouseUp(MouseButtonEventArgs args)
        {
        }

        protected virtual void OnMouseLeftButtonDown(MouseButtonEventArgs args)
        {
        }

        protected virtual void OnMouseLeftButtonUp(MouseButtonEventArgs args)
        {
        }

        protected virtual void OnMouseRightButtonDown(MouseButtonEventArgs args)
        {
        }

        protected virtual void OnMouseRightButtonUp(MouseButtonEventArgs args)
        {
        }

        protected virtual void OnMouseEnter(MouseEventArgs args)
        {
        }

        protected virtual void OnMouseLeave(MouseEventArgs args)
        {
        }

        protected virtual void OnMouseMove(MouseMoveEventArgs args)
        {
        }

        protected virtual void OnMouseWheel(MouseWheelEventArgs args)
        {
        }

        protected virtual void OnTouchEnter(TouchEventArgs args)
        {
        }

        protected virtual void OnTouchLeave(TouchEventArgs args)
        {
        }

        protected virtual void OnTouchMove(TouchMoveEventArgs args)
        {
        }

        protected virtual void OnTouchDown(TouchEventArgs args)
        {
        }

        protected virtual void OnTouchUp(TouchEventArgs args)
        {
        }

        public void Focus()
        {
            if (Focusable)
            {
                SetFocus(this);
            }
        }

        internal static bool SetFocus(IInputElement? element)
        {
            if (element == Focused)
            {
                return true;
            }

            if (element != null && !element.Focusable)
            {
                return false;
            }

            Focused?.RaiseEvent(new FocusLostEventArgs(LostFocusEvent));
            Focused = element;
            Focused?.RaiseEvent(new FocusGainedEventArgs(GotFocusEvent));

            return true;
        }
    }
}