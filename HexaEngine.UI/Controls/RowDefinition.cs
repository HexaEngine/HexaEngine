namespace HexaEngine.UI.Controls
{
    public class RowDefinition
    {
        public RowDefinition(GridLength height, float minHeight = 0, float maxHeight = 0)
        {
            MinHeight = minHeight;
            MaxHeight = maxHeight;
            Height = height;
        }

        public RowDefinition()
        {
            MinHeight = 0;
            MaxHeight = 0;
            Height = new(1, GridUnitType.Star);
        }

        public float MinHeight { get; set; }

        public float MaxHeight { get; set; }

        public GridLength Height { get; set; }

        public float ActualHeight { get; set; }

        public float NormalizeHeight(float height)
        {
            if (MinHeight != 0)
            {
                height = MathF.Max(height, MinHeight);
            }

            if (MaxHeight != 0)
            {
                height = MathF.Min(height, MaxHeight);
            }

            return height;
        }
    }
}