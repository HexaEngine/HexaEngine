namespace HexaEngine.UI
{
    public readonly struct DependencyPropertyChangedEventArgs
    {
        public DependencyPropertyChangedEventArgs(DependencyProperty property, object? oldValue, object? newValue)
        {
            Property = property;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public DependencyProperty Property { get; }

        public object? OldValue { get; }

        public object? NewValue { get; }
    }
}