namespace HexaEngine.UI
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Core.Windows.Events;
    using System.Diagnostics;
    using System.Numerics;

    public class InputElement : DependencyElement, IInputElement
    {
        private List<InputElement> hovering = [];

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

        public bool IsMouseOver { get; private set; }

        public bool IsEnabled { get; set; }

        public bool Focusable { get; set; }

        protected virtual void OnGotFocus(FocusGainedEventArgs args)
        {
            GotFocus?.Invoke(this, args);
        }

        protected virtual void OnLostFocus(FocusLostEventArgs args)
        {
            LostFocus?.Invoke(this, args);
        }

        internal virtual void OnChar(KeyboardCharEventArgs args)
        {
            TextInput?.Invoke(this, args);
        }

        internal virtual void OnKeyDown(KeyboardEventArgs args)
        {
            KeyDown?.Invoke(this, args);
        }

        internal virtual void OnKeyUp(KeyboardEventArgs args)
        {
            KeyUp?.Invoke(this, args);
        }

        internal virtual void OnMouseDown(MouseButtonEventArgs args)
        {
            if (args.Button == MouseButton.Left)
                Focus();
            MouseDown?.Invoke(this, args);
            RouteMouseDownEvent(args);
        }

        internal virtual void OnMouseUp(MouseButtonEventArgs args)
        {
            MouseUp?.Invoke(this, args);
            RouteMouseUpEvent(args);
        }

        internal virtual void OnDoubleClick(MouseButtonEventArgs args)
        {
            DoubleClick?.Invoke(this, args);
            RouteDoubleClickEvent(args);
        }

        internal virtual void OnMouseEnter(MouseEventArgs args)
        {
            IsMouseOver = true;
            Trace.WriteLine(GetType().Name + " Enter");
            MouseEnter?.Invoke(this, args);
            RouteMouseEnterEvent(args);
        }

        internal virtual void OnMouseLeave(MouseEventArgs args)
        {
            IsMouseOver = false;
            MouseLeave?.Invoke(this, args);
            RouteMouseLeaveEvent(args);
            Trace.WriteLine(GetType().Name + " Leave");
        }

        internal virtual void OnMouseMove(MouseMotionEventArgs args)
        {
            MouseMove?.Invoke(this, args);
            RouteMouseMoveEvent(args);
        }

        internal virtual void OnMouseWheel(MouseWheelEventArgs args)
        {
            MouseWheel?.Invoke(this, args);
            RouteMouseWheelEvent(args);
        }

        internal virtual void RouteMouseWheelEvent(MouseWheelEventArgs args)
        {
            hovering.ForEach(child => { child.OnMouseWheel(args); if (args.Handled) return; });
        }

        internal virtual void RouteDoubleClickEvent(MouseButtonEventArgs args)
        {
            hovering.ForEach(child => { child.OnDoubleClick(args); if (args.Handled) return; });
        }

        internal virtual void RouteMouseLeaveEvent(MouseEventArgs args)
        {
            if (args.Handled) return;
            hovering.ForEach(child => child.OnMouseLeave(args));
            hovering.Clear();
        }

        internal virtual void RouteMouseEnterEvent(MouseEventArgs args)
        {
            if (args.Handled) return;
        }

        internal virtual void RouteMouseUpEvent(MouseButtonEventArgs args)
        {
            if (args.Handled) return;
            hovering.ForEach(child => { child.OnMouseUp(args); if (args.Handled) return; });
        }

        internal void RouteMouseDownEvent(MouseButtonEventArgs args)
        {
            if (args.Handled) return;
            hovering.ForEach(child => { child.OnMouseDown(args); if (args.Handled) return; });
        }

        internal virtual void RouteMouseMoveEvent(MouseMotionEventArgs args)
        {
            if (args.Handled) return;
            var newHover = new List<InputElement>();
            if (this is IChildContainer container)
            {
                container.Children.ForEach(child =>
                {
                    var aabb = child.BoundingBox;
                    if (aabb.Contains(new Vector2(args.X, args.Y)))
                    {
                        child.OnMouseMove(args);
                        newHover.Add(child);
                    }
                });
            }

            hovering.Except(newHover).ToList().ForEach(child => child.OnMouseLeave(args));
            newHover.Except(hovering).ToList().ForEach(child => child.OnMouseEnter(args));
            hovering = newHover;
        }

        public virtual void RaiseEvent(RoutedEventArgs e)
        {
        }

        public void Focus()
        {
            if (Focusable)
            {
                SetFocus(this, true);
            }
        }

        internal static void SetFocus(IInputElement element, bool state)
        {
            if (state)
            {
                if (element == Focused)
                    return;
                Focused?.RaiseEvent(new FocusLostEventArgs());
                Focused = element;
                Focused.RaiseEvent(new FocusGainedEventArgs());
            }
            else
            {
                if (element != Focused)
                    return;
                Focused?.RaiseEvent(new FocusLostEventArgs());
                Focused = null;
            }
        }
    }
}