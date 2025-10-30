namespace HexaEngine.UI.Controls.Primitives
{
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Core.Windows.Events;
    using System.Diagnostics.CodeAnalysis;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public class Thumb : Control
    {
        private float horizontalOffset;
        private float verticalOffset;
        private float horizontalOffsetNew;
        private float verticalOffsetNew;

        public static readonly DependencyProperty<bool> IsDraggingProperty = DependencyProperty.Register<Thumb, bool>(nameof(IsDragging), false, new PropertyMetadata(false));

        public bool IsDragging { get => GetValue(IsDraggingProperty); private set => SetValue(IsDraggingProperty, value); }

        public static readonly RoutedEvent<DragCompletedEventArgs> DragCompletedEvent = EventManager.Register<Thumb, DragCompletedEventArgs>(nameof(DragCompleted), RoutingStrategy.Bubble);

        public event RoutedEventHandler<DragCompletedEventArgs> DragCompleted { add => AddHandler(DragCompletedEvent, value); remove => RemoveHandler(DragCompletedEvent, value); }

        public static readonly RoutedEvent<DragDeltaEventArgs> DragDeltaEvent = EventManager.Register<Thumb, DragDeltaEventArgs>(nameof(DragDelta), RoutingStrategy.Bubble);

        public event RoutedEventHandler<DragDeltaEventArgs> DragDelta { add => AddHandler(DragDeltaEvent, value); remove => RemoveHandler(DragDeltaEvent, value); }

        public static readonly RoutedEvent<DragStartedEventArgs> DragStartedEvent = EventManager.Register<Thumb, DragStartedEventArgs>(nameof(DragStarted), RoutingStrategy.Bubble);

        public event RoutedEventHandler<DragStartedEventArgs> DragStarted { add => AddHandler(DragStartedEvent, value); remove => RemoveHandler(DragStartedEvent, value); }

        private readonly DragCompletedEventArgs dragCompletedEventArgs = new(DragCompletedEvent);

        public void CancelDrag()
        {
            IsDragging = false;
            dragCompletedEventArgs.HorizontalChange = horizontalOffsetNew - horizontalOffset;
            dragCompletedEventArgs.VerticalChange = verticalOffsetNew - verticalOffset;
            dragCompletedEventArgs.Canceled = true;
            dragCompletedEventArgs.Handled = false;
            RaiseEvent(dragCompletedEventArgs);
            horizontalOffset = horizontalOffsetNew;
            verticalOffset = verticalOffsetNew;
        }

        private readonly DragStartedEventArgs dragStartedEventArgs = new(DragStartedEvent);

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs args)
        {
            IsDragging = true;
            dragStartedEventArgs.HorizontalOffset = horizontalOffset;
            dragStartedEventArgs.VerticalOffset = verticalOffset;
            dragStartedEventArgs.Handled = false;
            RaiseEvent(dragStartedEventArgs);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs args)
        {
            if (IsDragging)
            {
                dragCompletedEventArgs.HorizontalChange = horizontalOffsetNew - horizontalOffset;
                dragCompletedEventArgs.VerticalChange = verticalOffsetNew - verticalOffset;
                dragCompletedEventArgs.Canceled = false;
                dragCompletedEventArgs.Handled = false;
                RaiseEvent(dragCompletedEventArgs);
                horizontalOffset = horizontalOffsetNew;
                verticalOffset = verticalOffsetNew;
                IsDragging = false;
            }
        }

        private readonly DragDeltaEventArgs dragDeltaArgs = new(DragDeltaEvent);

        protected override void OnMouseMove(MouseMoveEventArgs args)
        {
            if (IsDragging)
            {
                var horizontalOffsetNext = horizontalOffsetNew + args.RelX;
                var verticalOffsetNext = verticalOffsetNew + args.RelY;
                dragDeltaArgs.HorizontalChange = horizontalOffsetNext - horizontalOffsetNew;
                dragDeltaArgs.VerticalChange = verticalOffsetNext - verticalOffsetNew;
                dragDeltaArgs.Handled = false;
                RaiseEvent(dragDeltaArgs);
                horizontalOffsetNew = horizontalOffsetNext;
                verticalOffsetNew = verticalOffsetNext;
            }
        }
    }
}