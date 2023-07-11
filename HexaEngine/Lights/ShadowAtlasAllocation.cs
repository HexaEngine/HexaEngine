namespace HexaEngine.Lights
{
    using HexaEngine.Mathematics;
    using System.Numerics;

    public struct ShadowAtlasAllocation
    {
        public nint BlockHandle;
        public nint LayerHandle;
        public Vector2 Size;
        public Vector2 Offset;
        public int LayerIndex;

        public readonly bool IsValid => BlockHandle != 0;

        public readonly Viewport GetViewport()
        {
            return new(Offset * Size, Size);
        }
    }
}