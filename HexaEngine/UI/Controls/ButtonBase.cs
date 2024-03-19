namespace HexaEngine.UI.Controls
{
    using HexaEngine.Core.Input;
    using HexaEngine.Core.Input.Events;
    using HexaEngine.Core.Windows.Events;

    public class ButtonBase : ContentControl
    {
        public static readonly RoutedEvent<RoutedEventArgs> ClickEvent = EventManager.Register<Button, RoutedEventArgs>(nameof(ClickEvent), RoutingStrategy.Direct);
        private ClickMode clickMode;

        public event RoutedEventHandler<RoutedEventArgs> Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        public ClickMode ClickMode { get => clickMode; set => clickMode = value; }

        protected override void OnMouseUp(MouseButtonEventArgs args)
        {
            if (clickMode != ClickMode.Release || args.Button != MouseButton.Left)
            {
                return;
            }

            args.Handled = true;
            RouteEvent(new RoutedEventArgs(ClickEvent));
        }

        protected override void OnMouseDown(MouseButtonEventArgs args)
        {
            if (clickMode != ClickMode.Press || args.Button != MouseButton.Left)
            {
                return;
            }

            args.Handled = true;
            RouteEvent(new RoutedEventArgs(ClickEvent));
        }

        protected override void OnMouseMove(MouseMoveEventArgs args)
        {
            if (clickMode != ClickMode.Hover)
            {
                return;
            }

            args.Handled = true;
            RouteEvent(new RoutedEventArgs(ClickEvent));
        }
    }
}