namespace HexaEngine.UI.Controls
{
    using Hexa.NET.Mathematics;
    using System.Numerics;

    public class StackPanel : Panel, IChildContainer
    {
        public StackPanelDirection Direction { get; set; }

        public override void InitializeComponent()
        {
            Children.ForEach(child => child.InitializeComponent());
            Children.ElementAdded += OnChildAdded;
            Children.ElementRemoved += OnChildRemoved;
        }

        protected virtual void OnChildAdded(object? sender, UIElement e)
        {
            InvalidateArrange();
        }

        protected virtual void OnChildRemoved(object? sender, UIElement e)
        {
            InvalidateArrange();
        }

        public override void Initialize()
        {
            base.Initialize();
            Children.ForEach(child => child.Initialize());
        }

        internal override void Uninitialize()
        {
            Children.ElementAdded -= OnChildAdded;
            Children.ElementRemoved -= OnChildRemoved;
            Children.ForEach(child => child.Uninitialize());
            base.Uninitialize();
        }

        protected override Vector2 ArrangeOverwrite(Vector2 size)
        {
            Vector2 origin = ContentOffset.Translation;

            float pen = 0;
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                Vector2 desiredSize = Vector2.Min(child.DesiredSize, size);

                Vector2 childOrigin;
                if (Direction == StackPanelDirection.Vertical)
                {
                    childOrigin = new(0, pen);
                    pen += desiredSize.Y;
                }
                else
                {
                    childOrigin = new(pen, 0);
                    pen += desiredSize.X;
                }

                RectangleF childRect = new(origin + childOrigin, desiredSize);
                child.Arrange(childRect);
            }

            return size;
        }

        protected override Vector2 MeasureOverwrite(Vector2 availableSize)
        {
            Vector2 size = default;
            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                child.Measure(availableSize);
                Vector2 desiredSize = child.DesiredSize;

                if (Direction == StackPanelDirection.Vertical)
                {
                    size.Y += desiredSize.Y;
                    size.X = MathF.Max(size.X, desiredSize.X);
                }
                else
                {
                    size.X += desiredSize.X;
                    size.Y = MathF.Max(size.Y, desiredSize.Y);
                }
            }

            size = Vector2.Min(size, availableSize);

            return size;
        }
    }
}