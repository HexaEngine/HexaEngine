namespace HexaEngine.UI
{
    public delegate object CoerceValueCallback(DependencyObject d, object? baseValue);

    public delegate void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e);

    public class PropertyMetadata
    {
        public PropertyMetadata()
        {
        }

        public PropertyMetadata(object? defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public PropertyMetadata(PropertyChangedCallback? propertyChangedCallback)
        {
            PropertyChangedCallback = propertyChangedCallback;
        }

        public PropertyMetadata(object? defaultValue, PropertyChangedCallback? propertyChangedCallback)
        {
            DefaultValue = defaultValue;
            PropertyChangedCallback = propertyChangedCallback;
        }

        public PropertyMetadata(object? defaultValue, PropertyChangedCallback? propertyChangedCallback, CoerceValueCallback? coerceValueCallback)
        {
            DefaultValue = defaultValue;
            PropertyChangedCallback = propertyChangedCallback;
            CoerceValueCallback = coerceValueCallback;
        }

        public object? DefaultValue { get; set; }

        protected bool IsSealed { get; }

        public PropertyChangedCallback? PropertyChangedCallback { get; set; }

        public CoerceValueCallback? CoerceValueCallback { get; set; }

        protected virtual void Merge(PropertyMetadata baseMetadata, DependencyProperty dp)
        {
        }
    }
}