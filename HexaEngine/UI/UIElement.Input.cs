namespace HexaEngine.UI
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Core.Windows.Events;
    using System.Numerics;

    public partial class UIElement : IInputElement
    {
        private bool isMouseOver;

        public static readonly RoutedEvent<TextInputEventArgs> TextInputEvent = EventManager.Register<UIElement, TextInputEventArgs>(nameof(TextInputEvent), RoutingStrategy.Direct);

        public static readonly RoutedEvent<KeyboardEventArgs> KeyUpEvent = EventManager.Register<UIElement, KeyboardEventArgs>(nameof(KeyUpEvent), RoutingStrategy.Direct);

        public static readonly RoutedEvent<KeyboardEventArgs> KeyDownEvent = EventManager.Register<UIElement, KeyboardEventArgs>(nameof(KeyDownEvent), RoutingStrategy.Direct);

        public static readonly RoutedEvent<MouseButtonEventArgs> MouseUpEvent = EventManager.Register<UIElement, MouseButtonEventArgs>(nameof(MouseUpEvent), RoutingStrategy.Direct);

        public static readonly RoutedEvent<MouseButtonEventArgs> MouseDownEvent = EventManager.Register<UIElement, MouseButtonEventArgs>(nameof(MouseDownEvent), RoutingStrategy.Direct);

        public static readonly RoutedEvent<MouseButtonEventArgs> MouseDoubleClickEvent = EventManager.Register<UIElement, MouseButtonEventArgs>(nameof(MouseDoubleClickEvent), RoutingStrategy.Direct);

        public static readonly RoutedEvent<MouseEventArgs> MouseEnterEvent = EventManager.Register<UIElement, MouseEventArgs>(nameof(MouseEnterEvent), RoutingStrategy.Tunnel);

        public static readonly RoutedEvent<MouseEventArgs> MouseLeaveEvent = EventManager.Register<UIElement, MouseEventArgs>(nameof(MouseLeaveEvent), RoutingStrategy.Bubble);

        public static readonly RoutedEvent<MouseMoveEventArgs> MouseMoveEvent = EventManager.Register<UIElement, MouseMoveEventArgs>(nameof(MouseMoveEvent), RoutingStrategy.Tunnel);

        public static readonly RoutedEvent<MouseWheelEventArgs> MouseWheelEvent = EventManager.Register<UIElement, MouseWheelEventArgs>(nameof(MouseWheelEvent), RoutingStrategy.Tunnel);

        public static readonly RoutedEvent<FocusGainedEventArgs> FocusGrainedEvent = EventManager.Register<UIElement, FocusGainedEventArgs>(nameof(FocusGrainedEvent), RoutingStrategy.Direct);

        public static readonly RoutedEvent<FocusLostEventArgs> FocusLostEvent = EventManager.Register<UIElement, FocusLostEventArgs>(nameof(FocusLostEvent), RoutingStrategy.Direct);

        public static readonly RoutedEvent<TouchEventArgs> TouchEnterEvent = EventManager.Register<UIElement, TouchEventArgs>(nameof(TouchEnterEvent), RoutingStrategy.Tunnel);

        public static readonly RoutedEvent<TouchEventArgs> TouchLeaveEvent = EventManager.Register<UIElement, TouchEventArgs>(nameof(TouchLeaveEvent), RoutingStrategy.Bubble);

        public static readonly RoutedEvent<TouchMoveEventArgs> TouchMoveEvent = EventManager.Register<UIElement, TouchMoveEventArgs>(nameof(TouchMoveEvent), RoutingStrategy.Tunnel);

        public static readonly RoutedEvent<TouchEventArgs> TouchDownEvent = EventManager.Register<UIElement, TouchEventArgs>(nameof(TouchDownEvent), RoutingStrategy.Direct);

        public static readonly RoutedEvent<TouchEventArgs> TouchUpEvent = EventManager.Register<UIElement, TouchEventArgs>(nameof(TouchUpEvent), RoutingStrategy.Direct);

        static UIElement()
        {
            TextInputEvent.AddClassHandler<UIElement>((x, args) => x?.OnTextInput(args));
            KeyUpEvent.AddClassHandler<UIElement>((x, args) => x?.OnKeyUp(args));
            KeyDownEvent.AddClassHandler<UIElement>((x, args) => x?.OnKeyDown(args));
            MouseUpEvent.AddClassHandler<UIElement>((x, args) => x?.OnMouseUp(args));
            MouseDownEvent.AddClassHandler<UIElement>(HandleMouseDown);
            MouseDoubleClickEvent.AddClassHandler<UIElement>((x, args) => x?.OnDoubleClick(args));
            MouseEnterEvent.AddClassHandler<UIElement>(HandleMouseEnter);
            MouseLeaveEvent.AddClassHandler<UIElement>(HandleMouseLeave);
            MouseMoveEvent.AddClassHandler<UIElement>((x, args) => x?.OnMouseMove(args));
            MouseWheelEvent.AddClassHandler<UIElement>((x, args) => x?.OnMouseWheel(args));
            FocusGrainedEvent.AddClassHandler<UIElement>((x, args) => x?.OnGotFocus(args));
            FocusLostEvent.AddClassHandler<UIElement>((x, args) => x?.OnLostFocus(args));
        }

        public event RoutedEventHandler<TextInputEventArgs> TextInput
        {
            add => AddHandler(TextInputEvent, value);
            remove => RemoveHandler(TextInputEvent, value);
        }

        public event RoutedEventHandler<KeyboardEventArgs> KeyUp
        {
            add => AddHandler(KeyUpEvent, value);
            remove => RemoveHandler(KeyUpEvent, value);
        }

        public event RoutedEventHandler<KeyboardEventArgs> KeyDown
        {
            add => AddHandler(KeyDownEvent, value);
            remove => RemoveHandler(KeyDownEvent, value);
        }

        public event RoutedEventHandler<MouseButtonEventArgs> MouseUp
        {
            add => AddHandler(MouseUpEvent, value);
            remove => RemoveHandler(MouseUpEvent, value);
        }

        public event RoutedEventHandler<MouseButtonEventArgs> MouseDown
        {
            add => AddHandler(MouseDownEvent, value);
            remove => RemoveHandler(MouseDownEvent, value);
        }

        public event RoutedEventHandler<MouseButtonEventArgs> DoubleClick
        {
            add => AddHandler(MouseDoubleClickEvent, value);
            remove => RemoveHandler(MouseDoubleClickEvent, value);
        }

        public event RoutedEventHandler<MouseEventArgs> MouseLeave
        {
            add => AddHandler(MouseLeaveEvent, value);
            remove => RemoveHandler(MouseLeaveEvent, value);
        }

        public event RoutedEventHandler<MouseEventArgs> MouseEnter
        {
            add => AddHandler(MouseEnterEvent, value);
            remove => RemoveHandler(MouseEnterEvent, value);
        }

        public event RoutedEventHandler<MouseMoveEventArgs> MouseMove
        {
            add => AddHandler(MouseMoveEvent, value);
            remove => RemoveHandler(MouseMoveEvent, value);
        }

        public event RoutedEventHandler<MouseWheelEventArgs> MouseWheel
        {
            add => AddHandler(MouseWheelEvent, value);
            remove => RemoveHandler(MouseWheelEvent, value);
        }

        public event RoutedEventHandler<FocusGainedEventArgs> GotFocus
        {
            add => AddHandler(FocusGrainedEvent, value);
            remove => RemoveHandler(FocusGrainedEvent, value);
        }

        public event RoutedEventHandler<FocusLostEventArgs> LostFocus
        {
            add => AddHandler(FocusLostEvent, value);
            remove => RemoveHandler(FocusLostEvent, value);
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

        private static void HandleMouseDown(UIElement? sender, MouseButtonEventArgs e)
        {
            if (sender == null) return;

            if (e.Button == MouseButton.Left)
            {
                sender.Focus();
            }

            sender.OnMouseDown(e);
        }

        private static void HandleMouseEnter(UIElement? sender, MouseEventArgs e)
        {
            if (sender == null) return;

            if (sender.isMouseOver) return;

            sender.isMouseOver = true;
            sender.OnMouseEnter(e);
        }

        private static void HandleMouseLeave(UIElement? sender, MouseEventArgs e)
        {
            if (sender == null) return;

            if (!sender.isMouseOver) return;

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

        protected virtual void OnDoubleClick(MouseButtonEventArgs args)
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

        protected virtual void RouteDoubleClickEvent(MouseButtonEventArgs args)
        {
        }

        public void Focus()
        {
            if (Focusable)
            {
                SetFocus(this);
            }
        }

        internal static void SetFocus(IInputElement? element)
        {
            if (element == Focused)
                return;

            Focused?.RouteEvent(new FocusLostEventArgs());
            Focused = element;
            Focused?.RouteEvent(new FocusGainedEventArgs());
        }
    }
}