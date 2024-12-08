namespace HexaEngine.UI.Animation
{
    public abstract class Timeline : Animatable
    {
        public static readonly DependencyProperty<double> AccelerationRatioProperty = DependencyProperty.Register<Timeline, double>(nameof(AccelerationRatio), false, new FrameworkMetadata(null));

        public double AccelerationRatio { get => GetValue(AccelerationRatioProperty); set => SetValue(AccelerationRatioProperty, value); }

        public static readonly DependencyProperty<bool> AutoReverseProperty = DependencyProperty.Register<Timeline, bool>(nameof(AutoReverse), false, new FrameworkMetadata(null));

        public bool AutoReverse { get => GetValue(AutoReverseProperty); set => SetValue(AutoReverseProperty, value); }

        public static readonly DependencyProperty<TimeSpan> BeginTimeProperty = DependencyProperty.Register<Timeline, TimeSpan>(nameof(BeginTime), false, new FrameworkMetadata(null));

        public TimeSpan BeginTime { get => GetValue(BeginTimeProperty); set => SetValue(BeginTimeProperty, value); }

        public static readonly DependencyProperty<double> DecelerationRatioProperty = DependencyProperty.Register<Timeline, double>(nameof(DecelerationRatio), false, new FrameworkMetadata(null));

        public double DecelerationRatio { get => GetValue(DecelerationRatioProperty); set => SetValue(DecelerationRatioProperty, value); }

        public static readonly DependencyProperty<Duration> DurationProperty = DependencyProperty.Register<Timeline, Duration>(nameof(Duration), false, new FrameworkMetadata(null));

        public Duration Duration { get => GetValue(DurationProperty); set => SetValue(DurationProperty, value); }
    }
}