namespace HexaEngine.Lights
{
    using HexaEngine.Mathematics;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public struct TextureCoordsMapping
    {
        public Matrix4x4 ClipToTexture;
        public Matrix4x4 ClipToNdc;

        public TextureCoordsMapping(Matrix4x4 clipToTexture, Matrix4x4 clipToNdc)
        {
            ClipToTexture = clipToTexture;
            ClipToNdc = clipToNdc;
        }
    }

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

        public static TextureCoordsMapping GetTextureCoordsMapping(int atlasSize, Viewport viewport)
        {
            Matrix4x4 mt = new(
                    0.5f, 0.0f, 0.0f, 0.5f,
                    0.0f, 0.5f, 0.0f, 0.5f,
                    0.0f, 0.0f, -0.5f, 0.5f,
                    0.0f, 0.0f, 0.0f, 1.0f
                );

            Vector2 o = new Vector2(viewport.X, viewport.Y) / atlasSize;
            Vector2 s = new Vector2(viewport.Width, viewport.Height) / atlasSize;

            Matrix4x4 mv = new(
                    s.X, 0.0f, 0.0f, o.X,
                    0.0f, s.Y, 0.0f, o.Y,
                    0.0f, 0.0f, 1.0f, 0.0f,
                    0.0f, 0.0f, 0.0f, 1.0f
                );

            Matrix4x4 mf = new(
                    1.0f, 0.0f, 0.0f, 0.0f,
                    0.0f, -1.0f, 0.0f, 1.0f,
                    0.0f, 0.0f, 1.0f, 0.0f,
                    0.0f, 0.0f, 0.0f, 1.0f
                );

            Matrix4x4.Invert(mt, out var inverse);

            return new(mt * mv * mf, mt * mv * inverse);
        }
    }
}