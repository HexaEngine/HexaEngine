namespace HexaEngine.UI
{
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
            base.Initialize();
            InvalidateLayout();
        }

        public override void Draw(UICommandList commandList)
        {
            if (!shown)
            {
                return;
            }
            DrawContent(commandList);
        }

        public override void DrawContent(UICommandList commandList)
        {
            commandList.PushClipRect(new ClipRectangle(Vector2.Zero, new(Width, Height)));
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Draw(commandList);
            }
            commandList.PopClipRect();
        }

        public override Vector2 GetContentSize(UIElement? ancestor)
        {
            return new Vector2(Width, Height);
        }

        public override void InvalidateLayout()
        {
            CalculateBounds();
        }

        public override void CalculateBounds()
        {
            BoundingBox = new(0, 0, Width, Height);
            ActualWidth = Width;
            ActualHeight = Height;
            UpdateBounds(null);
            Vector2 center = new Vector2(Width, Height) / 2f;
            UpdateLayout(null, center);

            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].CalculateBounds();
            }
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