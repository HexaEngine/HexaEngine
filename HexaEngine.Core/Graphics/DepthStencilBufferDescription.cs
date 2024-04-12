namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Describes a depth-stencil buffer, including its format, size, array size, mip levels, binding options, usage, CPU access options, view options, sample description, and miscellaneous options.
    /// </summary>
    public struct DepthStencilBufferDescription : IEquatable<DepthStencilBufferDescription>
    {
        /// <summary>
        /// Gets or sets the data format of the depth-stencil buffer.
        /// </summary>
        public Format Format;

        /// <summary>
        /// Gets or sets the width of the depth-stencil buffer.
        /// </summary>
        public int Width;

        /// <summary>
        /// Gets or sets the height of the depth-stencil buffer.
        /// </summary>
        public int Height;

        /// <summary>
        /// Gets or sets the array size of the depth-stencil buffer.
        /// </summary>
        public int ArraySize;

        /// <summary>
        /// Gets or sets the number of mip levels in the depth-stencil buffer.
        /// </summary>
        public int MipLevels;

        /// <summary>
        /// Gets or sets the binding options for the depth-stencil buffer.
        /// </summary>
        public BindFlags BindFlags;

        /// <summary>
        /// Gets or sets the intended usage of the depth-stencil buffer.
        /// </summary>
        public Usage Usage;

        /// <summary>
        /// Gets or sets the CPU access options for the depth-stencil buffer.
        /// </summary>
        public CpuAccessFlags CPUAccessFlags;

        /// <summary>
        /// Gets or sets the depth-stencil view options for the buffer.
        /// </summary>
        public DepthStencilViewFlags ViewFlags;

        /// <summary>
        /// Gets or sets the description of multi-sampling for the depth-stencil buffer.
        /// </summary>
        public SampleDescription SampleDescription;

        /// <summary>
        /// Gets or sets the miscellaneous options for the depth-stencil buffer.
        /// </summary>
        public ResourceMiscFlag MiscFlag;

        /// <summary>
        /// Creates a depth-stencil buffer description for a single depth buffer with shader resource view (SRV) support.
        /// </summary>
        /// <param name="width">The width of the depth-stencil buffer.</param>
        /// <param name="height">The height of the depth-stencil buffer.</param>
        /// <returns>A <see cref="DepthStencilBufferDescription"/> for a single depth buffer with SRV support.</returns>
        public static DepthStencilBufferDescription CreateDepthBufferSRV(int width, int height) => new(width, height, 1, Format.D32Float, BindFlags.ShaderResource | BindFlags.DepthStencil, Usage.Default, CpuAccessFlags.None, DepthStencilViewFlags.None, SampleDescription.Default);

        /// <summary>
        /// Creates a depth-stencil buffer description for an array of depth buffers with shader resource view (SRV) support.
        /// </summary>
        /// <param name="width">The width of the depth-stencil buffer.</param>
        /// <param name="height">The height of the depth-stencil buffer.</param>
        /// <param name="arraySize">The array size of the depth-stencil buffer.</param>
        /// <returns>A <see cref="DepthStencilBufferDescription"/> for an array of depth buffers with SRV support.</returns>
        public static DepthStencilBufferDescription CreateDepthBufferSRV(int width, int height, int arraySize) => new(width, height, arraySize, Format.D32Float, BindFlags.ShaderResource | BindFlags.DepthStencil, Usage.Default, CpuAccessFlags.None, DepthStencilViewFlags.None, SampleDescription.Default);

        public override readonly bool Equals(object? obj)
        {
            return obj is DepthStencilBufferDescription description && Equals(description);
        }

        public readonly bool Equals(DepthStencilBufferDescription other)
        {
            return Format == other.Format &&
                   Width == other.Width &&
                   Height == other.Height &&
                   ArraySize == other.ArraySize &&
                   MipLevels == other.MipLevels &&
                   BindFlags == other.BindFlags &&
                   Usage == other.Usage &&
                   CPUAccessFlags == other.CPUAccessFlags &&
                   ViewFlags == other.ViewFlags &&
                   SampleDescription.Equals(other.SampleDescription) &&
                   MiscFlag == other.MiscFlag;
        }

        public override readonly int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Format);
            hash.Add(Width);
            hash.Add(Height);
            hash.Add(ArraySize);
            hash.Add(MipLevels);
            hash.Add(BindFlags);
            hash.Add(Usage);
            hash.Add(CPUAccessFlags);
            hash.Add(ViewFlags);
            hash.Add(SampleDescription);
            hash.Add(MiscFlag);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencilBufferDescription"/> struct with specified parameters.
        /// </summary>
        /// <param name="width">The width of the depth-stencil buffer.</param>
        /// <param name="height">The height of the depth-stencil buffer.</param>
        /// <param name="arraySize">The array size of the depth-stencil buffer.</param>
        /// <param name="format">The data format of the depth-stencil buffer.</param>
        /// <param name="bindFlags">The binding options for the depth-stencil buffer.</param>
        /// <param name="usage">The intended usage of the depth-stencil buffer.</param>
        /// <param name="cPUAccessFlags">The CPU access options for the depth-stencil buffer.</param>
        /// <param name="viewFlags">The depth-stencil view options for the buffer.</param>
        /// <param name="sampleDescription">The description of multi-sampling for the depth-stencil buffer.</param>
        public DepthStencilBufferDescription(int width, int height, int arraySize, Format format, BindFlags bindFlags, Usage usage, CpuAccessFlags cPUAccessFlags, DepthStencilViewFlags viewFlags, SampleDescription sampleDescription)
        {
            Width = width;
            Height = height;
            ArraySize = arraySize;
            MipLevels = 1;
            Format = format;
            BindFlags = bindFlags;
            Usage = usage;
            CPUAccessFlags = cPUAccessFlags;
            ViewFlags = viewFlags;
            SampleDescription = sampleDescription;
            MiscFlag = ResourceMiscFlag.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencilBufferDescription"/> struct with specified parameters.
        /// </summary>
        /// <param name="width">The width of the depth-stencil buffer.</param>
        /// <param name="height">The height of the depth-stencil buffer.</param>
        /// <param name="arraySize">The array size of the depth-stencil buffer.</param>
        /// <param name="format">The data format of the depth-stencil buffer.</param>
        /// <param name="bindFlags">The binding options for the depth-stencil buffer.</param>
        /// <param name="usage">The intended usage of the depth-stencil buffer.</param>
        /// <param name="cPUAccessFlags">The CPU access options for the depth-stencil buffer.</param>
        /// <param name="viewFlags">The depth-stencil view options for the buffer.</param>
        public DepthStencilBufferDescription(int width, int height, int arraySize, Format format, BindFlags bindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cPUAccessFlags = CpuAccessFlags.None, DepthStencilViewFlags viewFlags = DepthStencilViewFlags.None)
        {
            Width = width;
            Height = height;
            ArraySize = arraySize;
            MipLevels = 1;
            Format = format;
            BindFlags = bindFlags;
            Usage = usage;
            CPUAccessFlags = cPUAccessFlags;
            ViewFlags = viewFlags;
            SampleDescription = SampleDescription.Default;
            MiscFlag = ResourceMiscFlag.None;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencilBufferDescription"/> struct with specified parameters.
        /// </summary>
        /// <param name="width">The width of the depth-stencil buffer.</param>
        /// <param name="height">The height of the depth-stencil buffer.</param>
        /// <param name="arraySize">The array size of the depth-stencil buffer.</param>
        /// <param name="mipLevels">The number of mip levels in the depth-stencil buffer.</param>
        /// <param name="format">The data format of the depth-stencil buffer.</param>
        /// <param name="bindFlags">The binding options for the depth-stencil buffer.</param>
        /// <param name="usage">The intended usage of the depth-stencil buffer.</param>
        /// <param name="cPUAccessFlags">The CPU access options for the depth-stencil buffer.</param>
        /// <param name="viewFlags">The depth-stencil view options for the buffer.</param>
        /// <param name="miscFlag">The miscellaneous options for the depth-stencil buffer.</param>
        public DepthStencilBufferDescription(int width, int height, int arraySize, int mipLevels, Format format, BindFlags bindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource, Usage usage = Usage.Default, CpuAccessFlags cPUAccessFlags = CpuAccessFlags.None, DepthStencilViewFlags viewFlags = DepthStencilViewFlags.None, ResourceMiscFlag miscFlag = ResourceMiscFlag.None)
        {
            Width = width;
            Height = height;
            ArraySize = arraySize;
            MipLevels = mipLevels;
            Format = format;
            BindFlags = bindFlags;
            Usage = usage;
            CPUAccessFlags = cPUAccessFlags;
            ViewFlags = viewFlags;
            SampleDescription = SampleDescription.Default;
            MiscFlag = miscFlag;
        }

        public static bool operator ==(DepthStencilBufferDescription left, DepthStencilBufferDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DepthStencilBufferDescription left, DepthStencilBufferDescription right)
        {
            return !(left == right);
        }
    }
}