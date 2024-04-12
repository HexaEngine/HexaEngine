namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;
    using System;

    public interface ICombinedTex2D : IDeviceChild
    {
        Viewport Viewport { get; }

        bool IsUAV { get; }

        bool IsRTV { get; }

        bool IsSRV { get; }

        bool IsDSV { get; }
    }

    public struct CombinedTex2DDesc : IEquatable<CombinedTex2DDesc>
    {
        public Format Format;
        public int Width;
        public int Height;
        public int ArraySize;
        public int MipLevels;
        public CpuAccessFlags CpuAccessFlags;
        public GpuAccessFlags GpuAccessFlags;
        public ResourceMiscFlag MiscFlag;
        public SampleDescription SampleDescription;

        public CombinedTex2DDesc(Format format, int width, int height, int arraySize, int mipLevels, CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags, ResourceMiscFlag miscFlag, SampleDescription sampleDescription)
        {
            Format = format;
            Width = width;
            Height = height;
            ArraySize = arraySize;
            MipLevels = mipLevels;
            CpuAccessFlags = cpuAccessFlags;
            GpuAccessFlags = gpuAccessFlags;
            MiscFlag = miscFlag;
            SampleDescription = sampleDescription;
        }

        public CombinedTex2DDesc(Format format, int width, int height, int arraySize, int mipLevels, CpuAccessFlags cpuAccessFlags, GpuAccessFlags gpuAccessFlags, ResourceMiscFlag miscFlag = ResourceMiscFlag.None)
        {
            Format = format;
            Width = width;
            Height = height;
            ArraySize = arraySize;
            MipLevels = mipLevels;
            CpuAccessFlags = cpuAccessFlags;
            GpuAccessFlags = gpuAccessFlags;
            MiscFlag = miscFlag;
            SampleDescription = SampleDescription.Default;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is CombinedTex2DDesc desc && Equals(desc);
        }

        public readonly bool Equals(CombinedTex2DDesc other)
        {
            return Format == other.Format &&
                   Width == other.Width &&
                   Height == other.Height &&
                   ArraySize == other.ArraySize &&
                   MipLevels == other.MipLevels &&
                   CpuAccessFlags == other.CpuAccessFlags &&
                   GpuAccessFlags == other.GpuAccessFlags &&
                   MiscFlag == other.MiscFlag &&
                   SampleDescription.Equals(other.SampleDescription);
        }

        public override readonly int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Format);
            hash.Add(Width);
            hash.Add(Height);
            hash.Add(ArraySize);
            hash.Add(MipLevels);
            hash.Add(CpuAccessFlags);
            hash.Add(GpuAccessFlags);
            hash.Add(MiscFlag);
            hash.Add(SampleDescription);
            return hash.ToHashCode();
        }

        public static bool operator ==(CombinedTex2DDesc left, CombinedTex2DDesc right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CombinedTex2DDesc left, CombinedTex2DDesc right)
        {
            return !(left == right);
        }
    }
}