namespace HexaEngine.UI
{
    using HexaEngine.Mathematics;
    using System.Numerics;

    public partial class UIElement
    {
        private Vector2 desiredSize;

        public static readonly DependencyProperty<HorizontalAlignment> HorizontalAlignmentProperty = DependencyProperty.Register<UIElement, HorizontalAlignment>(nameof(HorizontalAlignment), false, new FrameworkMetadata(HorizontalAlignment.Left) { AffectsArrange = true });

        public HorizontalAlignment HorizontalAlignment
        {
            get => GetValue(HorizontalAlignmentProperty);
            set => SetValue(HorizontalAlignmentProperty, value);
        }

        public static readonly DependencyProperty<VerticalAlignment> VerticalAlignmentProperty = DependencyProperty.Register<UIElement, VerticalAlignment>(nameof(VerticalAlignment), false, new FrameworkMetadata(VerticalAlignment.Top) { AffectsArrange = true });

        public VerticalAlignment VerticalAlignment
        {
            get => GetValue(VerticalAlignmentProperty);
            set => SetValue(VerticalAlignmentProperty, value);
        }

        public static readonly DependencyProperty<Thickness> PaddingProperty = DependencyProperty.Register<UIElement, Thickness>(nameof(Padding), false, new FrameworkMetadata(Thickness.Zero) { AffectsArrange = true, AffectsMeasure = true });

        public Thickness Padding
        {
            get => GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        public static readonly DependencyProperty<Thickness> MarginProperty = DependencyProperty.Register<UIElement, Thickness>(nameof(Margin), false, new FrameworkMetadata(Thickness.Zero) { AffectsArrange = true, AffectsMeasure = true });

        public Thickness Margin
        {
            get => GetValue(MarginProperty);
            set => SetValue(MarginProperty, value);
        }

        public static readonly DependencyProperty<float> WidthProperty = DependencyProperty.Register<UIElement, float>(nameof(Width), false, new FrameworkMetadata(float.NaN) { AffectsMeasure = true, AffectsArrange = true });

        public float Width
        {
            get => GetValue(WidthProperty);
            set => SetValue(WidthProperty, value);
        }

        public static readonly DependencyProperty<float> HeightProperty = DependencyProperty.Register<UIElement, float>(nameof(Height), false, new FrameworkMetadata(float.NaN) { AffectsMeasure = true, AffectsArrange = true });

        public float Height
        {
            get => GetValue(HeightProperty);
            set => SetValue(HeightProperty, value);
        }

        public static readonly DependencyProperty<float> MinWidthProperty = DependencyProperty.Register<UIElement, float>(nameof(MinWidth), false, new FrameworkMetadata(0f) { AffectsMeasure = true, AffectsArrange = true });

        public float MinWidth
        {
            get => GetValue(MinWidthProperty);
            set
            {
                SetValue(MinWidthProperty, value);
                InvalidateArrange();
            }
        }

        public static readonly DependencyProperty<float> MinHeightProperty = DependencyProperty.Register<UIElement, float>(nameof(MinHeight), false, new FrameworkMetadata(0f) { AffectsMeasure = true, AffectsArrange = true });

        public float MinHeight
        {
            get => GetValue(MinHeightProperty);
            set => SetValue(MinHeightProperty, value);
        }

        public static readonly DependencyProperty<float> MaxWidthProperty = DependencyProperty.Register<UIElement, float>(nameof(MaxWidth), false, new FrameworkMetadata(0f) { AffectsMeasure = true, AffectsArrange = true });

        public float MaxWidth
        {
            get => GetValue(MaxWidthProperty);
            set => SetValue(MaxWidthProperty, value);
        }

        public static readonly DependencyProperty<float> MaxHeightProperty = DependencyProperty.Register<UIElement, float>(nameof(MaxHeight), false, new FrameworkMetadata(0f) { AffectsMeasure = true, AffectsArrange = true });

        public float MaxHeight
        {
            get => GetValue(MaxHeightProperty);
            set => SetValue(MaxHeightProperty, value);
        }

        public Vector2 DesiredSize => desiredSize;

        public virtual void InvalidateArrange()
        {
            if (!IsInitialized)
            {
                return;
            }

            ResolveObject<UIWindow>()?.InvalidateArrange();
        }

        public virtual void InvalidateMeasure()
        {
            if (!IsInitialized)
            {
                return;
            }

            ResolveObject<UIWindow>()?.InvalidateMeasure();
        }

        public virtual void InvalidateVisual()
        {
            if (!IsInitialized)
            {
                return;
            }

            ResolveObject<UIWindow>()?.InvalidateVisual();
        }

        public void Measure(Vector2 availableSize)
        {
            if (Visibility == Visibility.Collapsed)
            {
                desiredSize = default;
                return;
            }

            desiredSize = MeasureCore(availableSize);
        }

        protected virtual Vector2 MeasureCore(Vector2 availableSize)
        {
            return default;
        }

        public void Arrange(RectangleF finalRect)
        {
            if (Visibility == Visibility.Collapsed)
            {
                return;
            }

            ArrangeCore(finalRect);
        }

        protected virtual void ArrangeCore(RectangleF finalRect)
        {
        }
    }
}