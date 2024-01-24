namespace HexaEngine.Core.Graphics
{
    using HexaEngine.Mathematics;

    public interface ICombinedTex2D : IDeviceChild
    {
        Viewport Viewport { get; }

        bool IsUAV { get; }

        bool IsRTV { get; }

        bool IsSRV { get; }

        bool IsDSV { get; }
    }

    public struct CombinedTex2DDesc
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
    }
}