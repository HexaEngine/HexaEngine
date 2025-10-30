namespace HexaEngine.UI.Controls
{
    public class ColumnDefinition
    {
        public ColumnDefinition(GridLength width, float minWidth = 0, float maxWidth = 0)
        {
            MinWidth = minWidth;
            MaxWidth = maxWidth;
            Width = width;
        }

        public ColumnDefinition()
        {
            MinWidth = 0;
            MaxWidth = 0;
            Width = new(1, GridUnitType.Star);
        }

        public float MinWidth { get; set; }

        public float MaxWidth { get; set; }

        public GridLength Width { get; set; }

        public float ActualWidth { get; set; }

        public float NormalizeWidth(float width)
        {
            if (MinWidth != 0)
            {
                width = MathF.Max(width, MinWidth);
            }

            if (MaxWidth != 0)
            {
                width = MathF.Min(width, MaxWidth);
            }

            return width;
        }
    }
}