namespace HexaEngine.Graphics.Overlays
{
    public class OverlayComparer : IComparer<IOverlay>
    {
        public static readonly OverlayComparer Instance = new();

        public int Compare(IOverlay? x, IOverlay? y)
        {
            if (x == null || y == null) return 0;

            return x.ZIndex.CompareTo(y.ZIndex);
        }
    }
}
