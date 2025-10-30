namespace HexaEngine.UI.Controls
{
    public struct GridLength
    {
        public float Value;
        public GridUnitType GridUnitType;

        public GridLength(float value = 1, GridUnitType gridUnitType = GridUnitType.Star)
        {
            Value = value;
            GridUnitType = gridUnitType;
        }
    }
}