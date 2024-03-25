namespace HexaEngine.UI.Controls
{
    using HexaEngine.Core;
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Core.Windows.Events;
    using HexaEngine.UI.Graphics;
    using System.Diagnostics.CodeAnalysis;
    using System.Numerics;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public class ScrollViewer : ContentControl
    {
        public static readonly DependencyProperty<bool> CanContentScrollProperty = DependencyProperty.Register<ScrollViewer, bool>(nameof(CanContentScroll), false, new PropertyMetadata(false));

        public bool CanContentScroll { get => GetValue(CanContentScrollProperty); set => SetValue(CanContentScrollProperty, value); }

        public static readonly DependencyProperty<Visibility> ComputedHorizontalScrollBarVisibilityProperty = DependencyProperty.Register<ScrollViewer, Visibility>(nameof(ComputedHorizontalScrollBarVisibility), false, new PropertyMetadata(Visibility.Hidden));

        public Visibility ComputedHorizontalScrollBarVisibility { get => GetValue(ComputedHorizontalScrollBarVisibilityProperty); private set => SetValue(ComputedHorizontalScrollBarVisibilityProperty, value); }

        public static readonly DependencyProperty<Visibility> ComputedVerticalScrollBarVisibilityProperty = DependencyProperty.Register<ScrollViewer, Visibility>(nameof(ComputedVerticalScrollBarVisibility), false, new PropertyMetadata(Visibility.Hidden));

        public Visibility ComputedVerticalScrollBarVisibility { get => GetValue(ComputedVerticalScrollBarVisibilityProperty); private set => SetValue(ComputedVerticalScrollBarVisibilityProperty, value); }

        public static readonly DependencyProperty<float> ContentHorizontalOffsetProperty = DependencyProperty.Register<ScrollViewer, float>(nameof(ContentHorizontalOffset), false, new PropertyMetadata(0));

        public float ContentHorizontalOffset { get => GetValue(ContentHorizontalOffsetProperty); private set => SetValue(ContentHorizontalOffsetProperty, value); }

        public static readonly DependencyProperty<float> ContentVerticalOffsetProperty = DependencyProperty.Register<ScrollViewer, float>(nameof(ContentVerticalOffset), false, new PropertyMetadata(0));

        public float ContentVerticalOffset { get => GetValue(ContentVerticalOffsetProperty); private set => SetValue(ContentVerticalOffsetProperty, value); }

        public static readonly DependencyProperty<float> ExtentHeightProperty = DependencyProperty.Register<ScrollViewer, float>(nameof(ExtentHeight), false, new PropertyMetadata(0));

        public float ExtentHeight { get => GetValue(ExtentHeightProperty); private set => SetValue(ExtentHeightProperty, value); }

        public static readonly DependencyProperty<float> ExtentWidthProperty = DependencyProperty.Register<ScrollViewer, float>(nameof(ExtentWidth), false, new PropertyMetadata(0));

        public float ExtentWidth { get => GetValue(ExtentWidthProperty); private set => SetValue(ExtentWidthProperty, value); }

        public static readonly DependencyProperty<float> HorizontalOffsetProperty = DependencyProperty.Register<ScrollViewer, float>(nameof(HorizontalOffset), false, new PropertyMetadata(0));

        public float HorizontalOffset { get => GetValue(HorizontalOffsetProperty); private set => SetValue(HorizontalOffsetProperty, value); }

        public static readonly DependencyProperty<float> VerticalOffsetProperty = DependencyProperty.Register<ScrollViewer, float>(nameof(VerticalOffset), false, new PropertyMetadata(0));

        public float VerticalOffset { get => GetValue(VerticalOffsetProperty); private set => SetValue(VerticalOffsetProperty, value); }

        public static readonly DependencyProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty = DependencyProperty.Register<ScrollViewer, ScrollBarVisibility>(nameof(HorizontalScrollBarVisibility), false, new PropertyMetadata(ScrollBarVisibility.Hidden));

        public ScrollBarVisibility HorizontalScrollBarVisibility { get => GetValue(HorizontalScrollBarVisibilityProperty); set => SetValue(HorizontalScrollBarVisibilityProperty, value); }

        public static readonly DependencyProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty = DependencyProperty.Register<ScrollViewer, ScrollBarVisibility>(nameof(VerticalScrollBarVisibility), false, new PropertyMetadata(ScrollBarVisibility.Hidden));

        public ScrollBarVisibility VerticalScrollBarVisibility { get => GetValue(VerticalScrollBarVisibilityProperty); set => SetValue(VerticalScrollBarVisibilityProperty, value); }

        public static readonly DependencyProperty<bool> IsDeferredScrollingEnabledProperty = DependencyProperty.Register<ScrollViewer, bool>(nameof(IsDeferredScrollingEnabled), false, new PropertyMetadata(false));

        public bool IsDeferredScrollingEnabled { get => GetValue(IsDeferredScrollingEnabledProperty); set => SetValue(IsDeferredScrollingEnabledProperty, value); }

        public static readonly DependencyProperty<bool> IsSmoothScrollingEnabledProperty = DependencyProperty.Register<ScrollViewer, bool>(nameof(IsSmoothScrollingEnabled), false, new PropertyMetadata(false));

        public bool IsSmoothScrollingEnabled { get => GetValue(IsSmoothScrollingEnabledProperty); set => SetValue(IsSmoothScrollingEnabledProperty, value); }

        public static readonly DependencyProperty<float> PanningDecelerationProperty = DependencyProperty.Register<ScrollViewer, float>(nameof(PanningDeceleration), false, new PropertyMetadata(1));

        public float PanningDeceleration { get => GetValue(PanningDecelerationProperty); set => SetValue(PanningDecelerationProperty, value); }

        public static readonly DependencyProperty<PanningMode> PanningModeProperty = DependencyProperty.Register<ScrollViewer, PanningMode>(nameof(PanningMode), false, new PropertyMetadata(PanningMode.None));

        public PanningMode PanningMode { get => GetValue(PanningModeProperty); set => SetValue(PanningModeProperty, value); }

        public static readonly DependencyProperty<float> PanningRatioProperty = DependencyProperty.Register<ScrollViewer, float>(nameof(PanningRatio), false, new PropertyMetadata(1));

        public float PanningRatio { get => GetValue(PanningRatioProperty); set => SetValue(PanningRatioProperty, value); }

        public static readonly DependencyProperty<float> ScrollableHeightProperty = DependencyProperty.Register<ScrollViewer, float>(nameof(ScrollableHeight), false, new PropertyMetadata(0));

        public float ScrollableHeight { get => GetValue(ScrollableHeightProperty); private set => SetValue(ScrollableHeightProperty, value); }

        public static readonly DependencyProperty<float> ScrollableWidthProperty = DependencyProperty.Register<ScrollViewer, float>(nameof(ScrollableWidth), false, new PropertyMetadata(0));

        public float ScrollableWidth { get => GetValue(ScrollableWidthProperty); private set => SetValue(ScrollableWidthProperty, value); }

        public static readonly DependencyProperty<float> ViewportHeightProperty = DependencyProperty.Register<ScrollViewer, float>(nameof(ViewportHeight), false, new PropertyMetadata(0));

        public float ViewportHeight { get => GetValue(ViewportHeightProperty); private set => SetValue(ViewportHeightProperty, value); }

        public static readonly DependencyProperty<float> ViewportWidthProperty = DependencyProperty.Register<ScrollViewer, float>(nameof(ViewportWidth), false, new PropertyMetadata(0));

        public float ViewportWidth { get => GetValue(ViewportWidthProperty); private set => SetValue(ViewportWidthProperty, value); }

        public static readonly RoutedEvent<ScrollChangedEventArgs> ScrollChangedEvent = EventManager.Register<ScrollViewer, ScrollChangedEventArgs>(nameof(ScrollChanged), RoutingStrategy.Bubble);

        public event RoutedEventHandler<ScrollChangedEventArgs> ScrollChanged { add => AddHandler(ScrollChangedEvent, value); remove => RemoveHandler(ScrollChangedEvent, value); }

        protected override Vector2 MeasureOverwrite(Vector2 availableSize)
        {
            return base.MeasureOverwrite(availableSize);
        }

        protected override Vector2 ArrangeOverwrite(Vector2 size)
        {
            return base.ArrangeOverwrite(size);
        }

        private float thumbHeight = 50;
        private float thumbPosition = 0;
        private float scrollableHeight = 1000;
        private float visibleHeight = 200;

        protected override void OnMouseDown(MouseButtonEventArgs args)
        {
            base.OnMouseDown(args);
        }

        protected override void OnMouseUp(MouseButtonEventArgs args)
        {
            base.OnMouseUp(args);
        }

        protected override void OnMouseMove(MouseMoveEventArgs args)
        {
            base.OnMouseMove(args);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs args)
        {
            thumbPosition += args.Wheel.Y * -1;
        }

        public void IsMouseOverThumb()
        {
        }

        protected override void OnRender(UICommandList commandList)
        {
            base.OnRender(commandList);

            float actualWidth = ActualWidth;
            float actualHeight = ActualHeight;

            // Zeichnen der Scrollleiste hier implementieren
            float scrollbarWidth = 20;
            float scrollbarHeight = actualHeight;
            float thumbWidth = scrollbarWidth;
            float thumbY = thumbPosition;

            float posX = actualWidth - scrollbarWidth;

            // Zeichnen der Scrollleisten-Spur
            commandList.FillRectangle(new Vector2(posX, 0), new Vector2(scrollbarWidth, scrollbarHeight), Brushes.LightGray);

            // Zeichnen des Scrollleisten-Daumens
            commandList.FillRectangle(new Vector2(posX, thumbY), new Vector2(thumbWidth, thumbHeight), Brushes.DarkGray);
        }
    }
}