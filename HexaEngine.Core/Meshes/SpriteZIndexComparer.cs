namespace HexaEngine.Rendering
{
    using System.Collections.Generic;

    public class SpriteZIndexComparer : IComparer<Sprite>
    {
        public int Compare(Sprite? x, Sprite? y)
        {
            if (x == null || y == null)
                return 0;
            return x.ZIndex.CompareTo(y.ZIndex);
        }
    }
}