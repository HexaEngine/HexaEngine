namespace HexaEngine.DirectXTex.Tests
{
    using System.Numerics;

    public unsafe class MiscImageOperations
    {
        [Fact]
        public void CopyRectangle()
        {
            TexMetadata metadataSrc = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.Texture2D,
                Format = Format.FormatR8G8B8A8Unorm,
                Height = 64,
                Width = 64,
                MipLevels = 1,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };
            ScratchImage srcImage = new();
            srcImage.Initialize(metadataSrc, CPFlags.None);

            TexMetadata metadataDest = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.Texture2D,
                Format = Format.FormatR8G8B8A8Unorm,
                Height = 512,
                Width = 512,
                MipLevels = 1,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };
            ScratchImage dstImage = new();
            dstImage.Initialize(metadataDest, CPFlags.None);

            Rect rect = new() { X = 0, Y = 0, W = 64, H = 64 };
            DirectXTex.CopyRectangle(srcImage.GetImage(0, 0, 0), &rect, dstImage.GetImage(0, 0, 0), TexFilterFlags.DEFAULT, 100, 50);

            srcImage.Release();
            dstImage.Release();
        }

        [Fact]
        public void ComputeMSE()
        {
            TexMetadata metadataSrc = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.Texture2D,
                Format = Format.FormatR8G8B8A8Unorm,
                Height = 64,
                Width = 64,
                MipLevels = 1,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };
            ScratchImage srcImage = new();
            srcImage.Initialize(metadataSrc, CPFlags.None);

            ScratchImage dstImage = new();
            dstImage.Initialize(metadataSrc, CPFlags.None);
            Vector4 mseV;
            float mse;

            DirectXTex.ComputeMSE(srcImage.GetImage(0, 0, 0), dstImage.GetImage(0, 0, 0), &mse, (float*)&mseV);

            srcImage.Release();
            dstImage.Release();
        }

        [Fact]
        public void EvaluateImage()
        {
            Vector4 maxLum = Vector4.Zero;
            void func(Vector4* pixels, ulong width, ulong y)
            {
                for (ulong j = 0; j < width; ++j)
                {
                    Vector4 s_luminance = new(0.3f, 0.59f, 0.11f, 0.0f);

                    Vector4 v = *pixels++;

                    v = new(Vector4.Dot(v, s_luminance));

                    maxLum = Vector4.Max(v, maxLum);
                }
            }

            TexMetadata metadataSrc = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.Texture2D,
                Format = Format.FormatR8G8B8A8Unorm,
                Height = 64,
                Width = 64,
                MipLevels = 1,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };
            ScratchImage srcImage = new();
            srcImage.Initialize(metadataSrc, CPFlags.None);

            DirectXTex.EvaluateImage(srcImage.GetImage(0, 0, 0), func);
        }

        public static bool NearEqual(Vector4 V1, Vector4 V2, Vector4 Epsilon)
        {
            return ((Math.Abs(V1.X - V2.X) <= Epsilon.X) &&
                     (Math.Abs(V1.Y - V2.Y) <= Epsilon.Y) &&
                     (Math.Abs(V1.Z - V2.Z) <= Epsilon.Z));
        }

        [Fact]
        public void TransformImage()
        {
            static void func(Vector4* outPixels, Vector4* inPixels, ulong width, ulong y)
            {
                Vector4 s_chromaKey = new(0.0f, 1.0f, 0.0f, 0.0f);
                Vector4 s_tolerance = new(0.2f, 0.2f, 0.2f, 0.0f);

                for (ulong j = 0; j < width; ++j)
                {
                    Vector4 value = inPixels[j];

                    if (NearEqual(value, s_chromaKey, s_tolerance))
                    {
                        value = Vector4.Zero;
                    }

                    outPixels[j] = value;
                }
            };

            TexMetadata metadataSrc = new()
            {
                ArraySize = 1,
                Depth = 1,
                Dimension = TexDimension.Texture2D,
                Format = Format.FormatR8G8B8A8Unorm,
                Height = 64,
                Width = 64,
                MipLevels = 1,
                MiscFlags = 0,
                MiscFlags2 = 0,
            };
            ScratchImage srcImage = new();
            srcImage.Initialize(metadataSrc, CPFlags.None);

            ScratchImage dstImage = new();
            dstImage.Initialize(metadataSrc, CPFlags.None);

            DirectXTex.TransformImage(srcImage.GetImage(0, 0, 0), func, &dstImage);
        }
    }
}