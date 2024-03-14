namespace HexaEngine.UI
{
    using HexaEngine.Core.Input;
    using HexaEngine.Mathematics;
    using HexaEngine.UI.Graphics;
    using System.Numerics;

    public class UIWindow : UIElement, IChildContainer, IDisposable
    {
        private bool disposedValue;
        private bool shown;

        public Color BackgroundColor { get; set; } = Colors.White;

        public string Title { get; set; }

        public UIElementCollection Children { get; }

        public float X { get; set; }

        public float Y { get; set; }

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

            base.Initialize();
            Children.ForEach(child => child.InitializeComponent());
            InvalidateArrange();
        }

        private void MouseButtonUp(object? sender, Core.Input.Events.MouseButtonEventArgs e)
        {
            OnMouseUp(e);
        }

        private void MouseButtonDown(object? sender, Core.Input.Events.MouseButtonEventArgs e)
        {
            OnMouseDown(e);
        }

        private void MouseMoved(object? sender, Core.Input.Events.MouseMotionEventArgs e)
        {
            OnMouseMove(e);
        }

        public override void Draw(UICommandList commandList)
        {
            if (!shown)
            {
                return;
            }
            OnRender(commandList);
        }

        public override void OnRender(UICommandList commandList)
        {
            commandList.PushClipRect(new ClipRectangle(Vector2.Zero, new(Width, Height)));
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Draw(commandList);
            }
            commandList.PopClipRect();
        }

        public override void InvalidateArrange()
        {
            Measure(new(Width, Height));
            Arrange(new(0, 0, Width, Height));
            InvalidateVisual();
        }

        public override void InvalidateVisual()
        {
        }

        protected override void ArrangeCore(RectangleF finalRect)
        {
            base.ArrangeCore(finalRect);
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Arrange(finalRect);
            }
        }

        protected override Vector2 MeasureCore(Vector2 availableSize)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Measure(availableSize);
            }
            return availableSize;
        }

        ~UIWindow()
        {
            Dispose(disposing: false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Children.ForEach(child => child.UninitializeComponent());

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