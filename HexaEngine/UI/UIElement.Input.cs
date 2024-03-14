namespace HexaEngine.UI
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Core.Windows.Events;
    using System.Diagnostics;
    using System.Numerics;

    public partial class UIElement : IInputElement
    {
        private List<UIElement> hovering = [];

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

        protected virtual void OnChar(KeyboardCharEventArgs args)
        {
            TextInput?.Invoke(this, args);
        }

        protected virtual void OnKeyDown(KeyboardEventArgs args)
        {
            KeyDown?.Invoke(this, args);
        }

        protected virtual void OnKeyUp(KeyboardEventArgs args)
        {
            KeyUp?.Invoke(this, args);
        }

        protected virtual void OnMouseDown(MouseButtonEventArgs args)
        {
            if (args.Button == MouseButton.Left)
                Focus();
            MouseDown?.Invoke(this, args);
            RouteMouseDownEvent(args);
        }

        protected virtual void OnMouseUp(MouseButtonEventArgs args)
        {
            MouseUp?.Invoke(this, args);
            RouteMouseUpEvent(args);
        }

        protected virtual void OnDoubleClick(MouseButtonEventArgs args)
        {
            DoubleClick?.Invoke(this, args);
            RouteDoubleClickEvent(args);
        }

        protected virtual void OnMouseEnter(MouseEventArgs args)
        {
            IsMouseOver = true;
            Trace.WriteLine(GetType().Name + " Enter");
            MouseEnter?.Invoke(this, args);
            RouteMouseEnterEvent(args);
        }

        protected virtual void OnMouseLeave(MouseEventArgs args)
        {
            IsMouseOver = false;
            MouseLeave?.Invoke(this, args);
            RouteMouseLeaveEvent(args);
            Trace.WriteLine(GetType().Name + " Leave");
        }

        protected virtual void OnMouseMove(MouseMotionEventArgs args)
        {
            MouseMove?.Invoke(this, args);
            RouteMouseMoveEvent(args);
        }

        protected virtual void OnMouseWheel(MouseWheelEventArgs args)
        {
            MouseWheel?.Invoke(this, args);
            RouteMouseWheelEvent(args);
        }

        protected virtual void RouteMouseWheelEvent(MouseWheelEventArgs args)
        {
            if (args.Handled)
                return;
            for (int i = 0; i < hovering.Count; i++)
            {
                hovering[i].OnMouseWheel(args);
                if (args.Handled)
                    return;
            }
        }

        protected virtual void RouteDoubleClickEvent(MouseButtonEventArgs args)
        {
            if (args.Handled)
                return;
            for (int i = 0; i < hovering.Count; i++)
            {
                hovering[i].OnDoubleClick(args);
                if (args.Handled)
                    return;
            }
        }

        protected virtual void RouteMouseLeaveEvent(MouseEventArgs args)
        {
            if (args.Handled)
                return;
            for (int i = 0; i < hovering.Count; i++)
            {
                hovering[i].OnMouseLeave(args);
            }

            hovering.Clear();
        }

        protected virtual void RouteMouseEnterEvent(MouseEventArgs args)
        {
            if (args.Handled)
                return;
        }

        protected virtual void RouteMouseUpEvent(MouseButtonEventArgs args)
        {
            if (args.Handled)
                return;
            for (int i = 0; i < hovering.Count; i++)
            {
                hovering[i].OnMouseUp(args);
                if (args.Handled)
                    return;
            }
        }

        protected void RouteMouseDownEvent(MouseButtonEventArgs args)
        {
            if (args.Handled)
                return;
            for (int i = 0; i < hovering.Count; i++)
            {
                hovering[i].OnMouseDown(args);
                if (args.Handled)
                    return;
            }
        }

        protected virtual void RouteMouseMoveEvent(MouseMotionEventArgs args)
        {
            // TODO: Optimize code, less gc pressure.
            if (args.Handled) return;
            var newHover = new List<UIElement>();
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