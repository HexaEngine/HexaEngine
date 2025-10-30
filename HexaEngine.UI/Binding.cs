namespace HexaEngine.UI
{
    using HexaEngine.Core.Windows.Events;

    public class Binding : BindingBase
    {
        public static readonly RoutedEvent<DataTransferEventArgs> SourceUpdatedEvent = EventManager.Register<Binding, DataTransferEventArgs>(nameof(SourceUpdatedEvent), RoutingStrategy.Direct);

        public static readonly RoutedEvent<DataTransferEventArgs> TargetUpdatedEvent = EventManager.Register<Binding, DataTransferEventArgs>(nameof(TargetUpdatedEvent), RoutingStrategy.Direct);

        public BindingMode Mode { get; set; }

        public UpdateSourceTrigger UpdateSourceTrigger { get; set; }
    }
}