namespace HexaEngine.UI
{
    using HexaEngine.Core.Windows.Events;

    public class DataTransferEventArgs : RoutedEventArgs
    {
        public DataTransferEventArgs(DependencyProperty property, DependencyObject targetObject)
        {
            Property = property;
            TargetObject = targetObject;
        }

        public DependencyProperty Property { get; }

        public DependencyObject TargetObject { get; }
    }
}