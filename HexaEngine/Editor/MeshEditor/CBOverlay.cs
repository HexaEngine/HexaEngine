namespace HexaEngine.Editor.MeshEditor
{
    using System.Numerics;
    using System.Runtime.InteropServices;

    public struct CBOverlay
    {
        public int ShowWeights;
        public int WeightMask;
        public Vector2 Padd1;

        public CBOverlay(int showWeights, int weightMask)
        {
            ShowWeights = showWeights;
            WeightMask = weightMask;
            Padd1 = default;
        }
    }
}