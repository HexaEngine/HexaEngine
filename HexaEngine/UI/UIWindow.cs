namespace HexaEngine.UI
{
    using HexaEngine.Core;
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Mathematics;
    using HexaEngine.UI.Controls;

    public class UIWindow : ContentControl, IChildContainer, IDisposable
    {
        private bool disposedValue;
        private bool shown;

        public Color BackgroundColor { get; set; } = Colors.White;

        public string Title { get; set; }

        public UIElementCollection Children { get; }

        public float X { get; set; }

        public float Y { get; set; }

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

            Keyboard.KeyDown += KeyboardKeyDown;
            Keyboard.KeyUp += KeyboardKeyUp;
            Keyboard.TextInput += KeyboardTextInput;

            Application.MainWindow.Enter += MainWindowEnter;
            Application.MainWindow.Leave += MainWindowLeave;

            base.Initialize();
            InvalidateArrange();
        }

        private void KeyboardTextInput(object? sender, TextInputEventArgs e)
        {
            e.RoutedEvent = TextInputEvent;
            Focused?.RouteEvent(e);
        }

        private void KeyboardKeyUp(object? sender, KeyboardEventArgs e)
        {
            e.RoutedEvent = KeyUpEvent;
            Focused?.RouteEvent(e);
        }

        private void KeyboardKeyDown(object? sender, KeyboardEventArgs e)
        {
            e.RoutedEvent = KeyDownEvent;
            Focused?.RouteEvent(e);
        }

        private UIElement? mouseFocused;

        private void MouseButtonUp(object? sender, MouseButtonEventArgs e)
        {
            e.RoutedEvent = MouseUpEvent;
            mouseFocused?.RouteEvent(e);
        }

        private void MouseButtonDown(object? sender, MouseButtonEventArgs e)
        {
            e.RoutedEvent = MouseDownEvent;
            mouseFocused?.RouteEvent(e);
        }

        private void MainWindowLeave(object? sender, Core.Windows.Events.LeaveEventArgs e)
        {
            mouseLeaveEventArgs.Handled = false;
            mouseFocused?.RouteEvent(mouseLeaveEventArgs);
            mouseFocused = null;
        }

        private void MainWindowEnter(object? sender, Core.Windows.Events.EnterEventArgs e)
        {
            var position = Application.MainWindow.MousePosition;
            var result = VisualTreeHelper.HitTest(this, position);
            if (result.VisualHit == null)
            {
                return;
            }
            var first = result.VisualHit.FindFirstObject<UIElement>();
            if (first != null)
            {
                UpdateMouseFocus(first);
            }
        }

        private void MouseMoved(object? sender, MouseMoveEventArgs e)
        {
            var position = Application.MainWindow.MousePosition;
            var result = VisualTreeHelper.HitTest(this, position);
            if (result.VisualHit == null)
            {
                return;
            }
            var first = result.VisualHit.FindFirstObject<UIElement>();

            if (first != null)
            {
                UpdateMouseFocus(first);

                e.RoutedEvent = MouseMoveEvent;
                first.RouteEvent(e);
            }
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
                newElement.RouteEvent(mouseEnterEventArgs);
            }

            mouseFocused = newElement;
        }

        public override void InvalidateArrange()
        {
            Measure(new(Width - X, Height - Y));
            Arrange(new(X, Y, Width, Height));
            InvalidateVisual();
        }

        public override void InvalidateVisual()
        {
            OnInvalidateVisual?.Invoke(this, EventArgs.Empty);
        }

        ~UIWindow()
        {
            Dispose(disposing: false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Uninitialize();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}