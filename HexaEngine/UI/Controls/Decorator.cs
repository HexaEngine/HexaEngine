namespace HexaEngine.UI.Controls
{
    using Hexa.NET.Mathematics;
    using HexaEngine.UI.Markup;
    using System.Numerics;

    public static class MathHelper
    {
        public static Vector2 Deflate(this Vector2 size, Thickness thickness)
        {
            return new Vector2(size.X - thickness.Left - thickness.Right, size.Y - thickness.Top - thickness.Bottom);
        }

        public static Vector2 Inflate(this Vector2 size, Thickness thickness)
        {
            return new Vector2(size.X + thickness.Left + thickness.Right, size.Y + thickness.Top + thickness.Bottom);
        }

        public static RectangleF Deflate(this RectangleF rect, Thickness thickness)
        {
            return new RectangleF(rect.Left + thickness.Left, rect.Top + thickness.Top, rect.Right - thickness.Right, rect.Bottom - thickness.Bottom);
        }

        public static RectangleF Inflate(this RectangleF rect, Thickness thickness)
        {
            return new RectangleF(rect.Left - thickness.Left, rect.Top - thickness.Top, rect.Right + thickness.Right, rect.Bottom + thickness.Bottom);
        }
    }

    [ContentProperty("Child")]
    public class Decorator : FrameworkElement, IAddChild
    {
        static Decorator()
        {
        }

        public static readonly DependencyProperty<UIElement?> ChildProperty = DependencyProperty.Register<Decorator, UIElement?>(nameof(Child), false, new FrameworkMetadata(null) { AffectsMeasure = true, PropertyChangedCallback = ChildChanged });

        public virtual UIElement? Child { get => GetValue(ChildProperty); set => SetValue(ChildProperty, value); }

        public static readonly DependencyProperty<Thickness> PaddingProperty = DependencyProperty.Register<Decorator, Thickness>(nameof(Padding), false, new FrameworkMetadata(Thickness.Zero) { AffectsArrange = true, AffectsMeasure = true });

        public Thickness Padding { get => GetValue(PaddingProperty); set => SetValue(PaddingProperty, value); }

        public void AddChild(object value)
        {
            if (value is UIElement element)
            {
                Child = element;
            }
        }

        public void AddText(string text)
        {
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            if (Child != null)
            {
                Child.Measure(availableSize.Deflate(Padding));
                return Child.DesiredSize;
            }
            return default;
        }

        protected override Vector2 ArrangeOverwrite(Vector2 size)
        {
            Child?.Arrange(new RectangleF(default, size).Deflate(Padding));
            return size;
        }

        private static void ChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not Decorator decorator) return;
            var oldChild = (UIElement?)e.OldValue;
            var newChild = (UIElement?)e.NewValue;

            if (oldChild != null)
            {
                oldChild.Parent = null;
                //decorator.LogicalChildren.Clear();
                decorator.VisualChildren.Remove(oldChild);
            }

            if (newChild != null)
            {
                (newChild).Parent = (decorator);
                decorator.VisualChildren.Add(newChild);
                //decorator.LogicalChildren.Add(newChild);
            }
        }
    }
}