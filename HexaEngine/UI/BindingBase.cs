namespace HexaEngine.UI
{
    public abstract class BindingBase
    {
        public string? BindingGroupName { get; set; }

        public int Delay { get; set; }

        public object? FallbackValue { get; set; }

        public string? StringFormat { get; set; }

        public object? TargetNullValue { get; set; }
    }
}