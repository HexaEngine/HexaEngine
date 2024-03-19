namespace HexaEngine.UI
{
    public class FrameworkMetadata : UIMetadata
    {
        public FrameworkMetadata()
        {
        }

        public FrameworkMetadata(object? defaultValue) : base(defaultValue)
        {
        }

        public FrameworkMetadata(PropertyChangedCallback? propertyChangedCallback) : base(propertyChangedCallback)
        {
        }

        public FrameworkMetadata(object? defaultValue, PropertyChangedCallback? propertyChangedCallback) : base(defaultValue, propertyChangedCallback)
        {
        }

        public FrameworkMetadata(object? defaultValue, PropertyChangedCallback? propertyChangedCallback, CoerceValueCallback? coerceValueCallback) : base(defaultValue, propertyChangedCallback, coerceValueCallback)
        {
        }

        public bool AffectsArrange { get; set; }

        public bool AffectsMeasure { get; set; }

        public bool AffectsParentArrange { get; set; }

        public bool AffectsParentMeasure { get; set; }

        public bool AffectsRender { get; set; }

        public bool BindsTwoWayByDefault { get; set; }

        public UpdateSourceTrigger DefaultUpdateSourceTrigger { get; set; }

        public bool Inherits { get; set; } = true;

        public bool IsDataBindingAllowed { get; set; }

        public bool IsNotDataBindable { get; set; }

        public bool Journal { get; set; }

        public bool OverridesInheritanceBehavior { get; set; }

        public bool SubPropertiesDoNotAffectRender { get; set; }
    }
}