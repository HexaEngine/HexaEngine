namespace HexaEngine.UI.Controls.Primitives
{
    using System.Diagnostics.CodeAnalysis;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public class RangeBase : Control
    {
        public static readonly DependencyProperty<float> LargeChangeProperty = DependencyProperty.Register<RangeBase, float>(nameof(LargeChange), false, new PropertyMetadata(1f));

        public float LargeChange { get => GetValue(LargeChangeProperty); set => SetValue(LargeChangeProperty, value); }

        public static readonly DependencyProperty<float> MaximumProperty = DependencyProperty.Register<RangeBase, float>(nameof(Maximum), false, new PropertyMetadata(0f));

        public float Maximum { get => GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }

        public static readonly DependencyProperty<float> MinimumProperty = DependencyProperty.Register<RangeBase, float>(nameof(Minimum), false, new PropertyMetadata(0f));

        public float Minimum { get => GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }

        public static readonly DependencyProperty<float> SmallChangeProperty = DependencyProperty.Register<RangeBase, float>(nameof(SmallChange), false, new PropertyMetadata(0.1f));

        public float SmallChange { get => GetValue(SmallChangeProperty); set => SetValue(SmallChangeProperty, value); }

        public static readonly DependencyProperty<float> ValueProperty = DependencyProperty.Register<RangeBase, float>(nameof(Value), false, new PropertyMetadata(1f));

        public float Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    }
}