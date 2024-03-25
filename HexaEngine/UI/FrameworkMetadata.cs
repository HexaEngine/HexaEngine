namespace HexaEngine.UI
{
    public class FrameworkMetadata : UIMetadata
    {
        private FrameworkPropertyMetadataOptions options;

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

        public FrameworkMetadata(object? defaultValue, FrameworkPropertyMetadataOptions options) : base(defaultValue)
        {
            this.options = options;
        }

        public FrameworkMetadata(object? defaultValue, FrameworkPropertyMetadataOptions options, PropertyChangedCallback? propertyChangedCallback) : base(defaultValue, propertyChangedCallback)
        {
            this.options = options;
        }

        public FrameworkMetadata(object? defaultValue, FrameworkPropertyMetadataOptions options, PropertyChangedCallback? propertyChangedCallback, CoerceValueCallback? coerceValueCallback) : base(defaultValue, propertyChangedCallback, coerceValueCallback)
        {
            this.options = options;
        }

        public FrameworkMetadata(object? defaultValue, FrameworkPropertyMetadataOptions options, PropertyChangedCallback? propertyChangedCallback, CoerceValueCallback? coerceValueCallback, bool isAnimationProhibited) : base(defaultValue, propertyChangedCallback, coerceValueCallback, isAnimationProhibited)
        {
            this.options = options;
        }

        public bool AffectsMeasure
        {
            get => (options & FrameworkPropertyMetadataOptions.AffectsMeasure) != 0;
            set
            {
                if (value)
                {
                    options |= FrameworkPropertyMetadataOptions.AffectsMeasure;
                }
                else
                {
                    options &= ~FrameworkPropertyMetadataOptions.AffectsMeasure;
                }
            }
        }

        public bool AffectsArrange
        {
            get => (options & FrameworkPropertyMetadataOptions.AffectsArrange) != 0;
            set
            {
                if (value)
                {
                    options |= FrameworkPropertyMetadataOptions.AffectsArrange;
                }
                else
                {
                    options &= ~FrameworkPropertyMetadataOptions.AffectsArrange;
                }
            }
        }

        public bool AffectsParentMeasure
        {
            get => (options & FrameworkPropertyMetadataOptions.AffectsParentMeasure) != 0;
            set
            {
                if (value)
                {
                    options |= FrameworkPropertyMetadataOptions.AffectsParentMeasure;
                }
                else
                {
                    options &= ~FrameworkPropertyMetadataOptions.AffectsParentMeasure;
                }
            }
        }

        public bool AffectsParentArrange
        {
            get => (options & FrameworkPropertyMetadataOptions.AffectsParentArrange) != 0;
            set
            {
                if (value)
                {
                    options |= FrameworkPropertyMetadataOptions.AffectsParentArrange;
                }
                else
                {
                    options &= ~FrameworkPropertyMetadataOptions.AffectsParentArrange;
                }
            }
        }

        public bool AffectsRender
        {
            get => (options & FrameworkPropertyMetadataOptions.AffectsRender) != 0;
            set
            {
                if (value)
                {
                    options |= FrameworkPropertyMetadataOptions.AffectsRender;
                }
                else
                {
                    options &= ~FrameworkPropertyMetadataOptions.AffectsRender;
                }
            }
        }

        public bool BindsTwoWayByDefault
        {
            get => (options & FrameworkPropertyMetadataOptions.BindsTwoWayByDefault) != 0;
            set
            {
                if (value)
                {
                    options |= FrameworkPropertyMetadataOptions.BindsTwoWayByDefault;
                }
                else
                {
                    options &= ~FrameworkPropertyMetadataOptions.BindsTwoWayByDefault;
                }
            }
        }

        public bool Inherits
        {
            get => (options & FrameworkPropertyMetadataOptions.Inherits) != 0;
            set
            {
                if (value)
                {
                    options |= FrameworkPropertyMetadataOptions.Inherits;
                }
                else
                {
                    options &= ~FrameworkPropertyMetadataOptions.Inherits;
                }
            }
        }

        public bool IsDataBindingAllowed
        {
            get => (options & FrameworkPropertyMetadataOptions.NotDataBindable) == 0;
            set
            {
                if (value)
                {
                    options &= ~FrameworkPropertyMetadataOptions.NotDataBindable;
                }
                else
                {
                    options |= FrameworkPropertyMetadataOptions.NotDataBindable;
                }
            }
        }

        public bool IsNotDataBindable
        {
            get => (options & FrameworkPropertyMetadataOptions.NotDataBindable) != 0;
            set
            {
                if (value)
                {
                    options |= FrameworkPropertyMetadataOptions.NotDataBindable;
                }
                else
                {
                    options &= ~FrameworkPropertyMetadataOptions.NotDataBindable;
                }
            }
        }

        public bool Journal
        {
            get => (options & FrameworkPropertyMetadataOptions.Journal) != 0;
            set
            {
                if (value)
                {
                    options |= FrameworkPropertyMetadataOptions.Journal;
                }
                else
                {
                    options &= ~FrameworkPropertyMetadataOptions.Journal;
                }
            }
        }

        public bool OverridesInheritanceBehavior
        {
            get => (options & FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior) != 0;
            set
            {
                if (value)
                {
                    options |= FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior;
                }
                else
                {
                    options &= ~FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior;
                }
            }
        }

        public bool SubPropertiesDoNotAffectRender
        {
            get => (options & FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender) != 0;
            set
            {
                if (value)
                {
                    options |= FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender;
                }
                else
                {
                    options &= ~FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender;
                }
            }
        }

        public UpdateSourceTrigger DefaultUpdateSourceTrigger { get; set; }
    }
}