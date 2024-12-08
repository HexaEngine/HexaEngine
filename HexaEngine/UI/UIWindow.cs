namespace HexaEngine.UI
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using HexaEngine.UI.Controls;
    using System.Numerics;

    public class UIWindow : ContentControl, IChildContainer, IDisposable
    {
        private UIElement? mouseFocused;
        private bool disposedValue;
        private bool shown;
        private bool hovered;
        private Matrix3x2 inputTransform = Matrix3x2.Identity;

        public Color BackgroundColor { get; set; } = Colors.White;

        public string? Title { get; set; }

        public UIElementCollection Children { get; } = null!;

        public float X { get; set; }

        public float Y { get; set; }

        public bool Hovered { get => hovered; }

        public event EventHandler? OnInvalidateVisual;

        public UIWindow()
        {
        }

        public UIWindow(string title, float width, float height)
        {
            Title = title;
            Children = new(this);

            Width = width;
            Height = height;
        }

        public void Show()
        {
            Initialize();
            shown = true;
        }

        public new void Initialize()
        {
            Mouse.Moved += MouseMoved;
            Mouse.ButtonDown += MouseButtonDown;
            Mouse.ButtonUp += MouseButtonUp;
            Mouse.Wheel += MouseWheel;

            Keyboard.KeyDown += KeyboardKeyDown;
            Keyboard.KeyUp += KeyboardKeyUp;
            Keyboard.TextInput += KeyboardTextInput;

            base.Initialize();
            InvalidateArrange();
        }

        private new void MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            e.RoutedEvent = MouseWheelEvent;
            mouseFocused?.RaiseEvent(e);
        }

        private void KeyboardTextInput(object? sender, TextInputEventArgs e)
        {
            e.RoutedEvent = TextInputEvent;
            Focused?.RaiseEvent(e);
        }

        private void KeyboardKeyUp(object? sender, KeyboardEventArgs e)
        {
            e.RoutedEvent = KeyUpEvent;
            Focused?.RaiseEvent(e);
        }

        private void KeyboardKeyDown(object? sender, KeyboardEventArgs e)
        {
            e.RoutedEvent = KeyDownEvent;
            Focused?.RaiseEvent(e);
        }

        private void MouseButtonUp(object? sender, MouseButtonEventArgs e)
        {
            var position = e.Position;
            Vector2 adjusted = Vector2.Transform(Mouse.Global, inputTransform);
            e.Position = adjusted;
            e.RoutedEvent = MouseUpEvent;
            mouseFocused?.RaiseEvent(e);
            e.Position = position;
        }

        private void MouseButtonDown(object? sender, MouseButtonEventArgs e)
        {
            var position = Mouse.Global;
            Vector2 adjusted = Vector2.Transform(Mouse.Global, inputTransform);
            e.Position = adjusted;
            e.RoutedEvent = MouseDownEvent;
            mouseFocused?.RaiseEvent(e);
            e.Position = position;
        }

        private void MouseMoved(object? sender, MouseMoveEventArgs e)
        {
            var x = e.X;
            var y = e.Y;
            var position = new Vector2(x, y);
            Vector2 adjusted = Vector2.Transform(position, inputTransform);

            if (adjusted.X < X || adjusted.Y < Y || adjusted.X > X + Width || adjusted.Y > Y + Height)
            {
                if (hovered)
                {
                    hovered = false;
                    OnMouseLeave();
                }
                return;
            }

            if (!hovered)
            {
                hovered = true;
                OnMouseEnter();
            }

            var result = VisualTreeHelper.HitTest(this, adjusted);
            if (result.VisualHit == null)
            {
                return;
            }

            e.X = adjusted.X;
            e.Y = adjusted.Y;

            var first = result.VisualHit.FindFirstObject<UIElement>();

            if (first != null)
            {
                UpdateMouseFocus(first);

                e.RoutedEvent = MouseMoveEvent;
                first.RaiseEvent(e);
            }

            e.X = x;
            e.Y = y;
        }

        protected virtual void OnMouseLeave()
        {
            mouseLeaveEventArgs.Handled = false;
            mouseFocused?.RaiseEvent(mouseLeaveEventArgs);
            mouseFocused = null;
        }

        protected virtual void OnMouseEnter()
        {
            mouseEnterEventArgs.Handled = false;
            RaiseEvent(mouseEnterEventArgs);
            mouseFocused = this;
        }

        private readonly MouseEventArgs mouseEnterEventArgs = new(MouseEnterEvent);
        private readonly MouseEventArgs mouseLeaveEventArgs = new(MouseLeaveEvent);

        private void UpdateMouseFocus(UIElement newElement)
        {
            if (mouseFocused == newElement)
            {
                return;
            }

            mouseEnterEventArgs.Handled = false;
            mouseLeaveEventArgs.Handled = false;

            if (mouseFocused != null)
            {
                if (mouseFocused.IsAncestorOf(newElement))
                {
                    newElement.RouteEvent(mouseEnterEventArgs, mouseFocused);
                }
                else if (mouseFocused.IsDescendantOf(newElement))
                {
                    mouseFocused.RouteEvent(mouseLeaveEventArgs, newElement);
                }
                else
                {
                    var commonAncestor = mouseFocused.FindCommonVisualAncestor(newElement);
                    mouseFocused.RouteEvent(mouseLeaveEventArgs, commonAncestor);
                    newElement.RouteEvent(mouseEnterEventArgs, commonAncestor);
                }
            }
            else
            {
                newElement.RaiseEvent(mouseEnterEventArgs);
            }

            mouseFocused = newElement;
        }

        public override sealed void InvalidateMeasure()
        {
            Measure(new(Width - X, Height - Y));
            InvalidateArrange();
        }

        public override sealed void InvalidateArrange()
        {
            Arrange(new(X, Y, Width, Height));
            InvalidateVisual();
        }

        public override void InvalidateVisual()
        {
            OnInvalidateVisual?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Mouse.Moved -= MouseMoved;
                Mouse.ButtonDown -= MouseButtonDown;
                Mouse.ButtonUp -= MouseButtonUp;
                Mouse.Wheel -= MouseWheel;

                Keyboard.KeyDown -= KeyboardKeyDown;
                Keyboard.KeyUp -= KeyboardKeyUp;
                Keyboard.TextInput -= KeyboardTextInput;

                Uninitialize();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void SetInputTransform(Matrix3x2 inputTransform)
        {
            this.inputTransform = inputTransform;
        }
    }
}