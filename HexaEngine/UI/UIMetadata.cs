namespace HexaEngine.UI
{
    public class UIMetadata : PropertyMetadata
    {
        public UIMetadata()
        {
        }

        public UIMetadata(object? defaultValue) : base(defaultValue)
        {
        }

        public UIMetadata(PropertyChangedCallback? propertyChangedCallback) : base(propertyChangedCallback)
        {
        }

        public UIMetadata(object? defaultValue, PropertyChangedCallback? propertyChangedCallback) : base(defaultValue, propertyChangedCallback)
        {
        }

        public UIMetadata(object? defaultValue, PropertyChangedCallback? propertyChangedCallback, CoerceValueCallback? coerceValueCallback) : base(defaultValue, propertyChangedCallback, coerceValueCallback)
        {
        }

        public UIMetadata(object? defaultValue, PropertyChangedCallback? propertyChangedCallback, CoerceValueCallback? coerceValueCallback, bool isAnimationProhibited) : base(defaultValue, propertyChangedCallback, coerceValueCallback)
        {
            IsAnimationProhibited = isAnimationProhibited;
        }

        public bool IsAnimationProhibited { get; set; }
    }
}