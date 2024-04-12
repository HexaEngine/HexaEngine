namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Describes a swap chain.
    /// </summary>
    public struct SwapChainDescription : IEquatable<SwapChainDescription>
    {
        /// <summary>
        /// The width of the swap chain.
        /// </summary>
        public uint Width;

        /// <summary>
        /// The height of the swap chain.
        /// </summary>
        public uint Height;

        /// <summary>
        /// The display format.
        /// </summary>
        public Format Format;

        /// <summary>
        /// Indicates whether the swap chain is stereo.
        /// </summary>
        public bool Stereo;

        /// <summary>
        /// The sample description.
        /// </summary>
        public SampleDescription SampleDesc;

        /// <summary>
        /// The buffer usage flags.
        /// </summary>
        public uint BufferUsage;

        /// <summary>
        /// The number of back buffers in the swap chain.
        /// </summary>
        public uint BufferCount;

        /// <summary>
        /// The scaling mode of the swap chain.
        /// </summary>
        public Scaling Scaling;

        /// <summary>
        /// The swap effect.
        /// </summary>
        public SwapEffect SwapEffect;

        /// <summary>
        /// The alpha mode of the swap chain.
        /// </summary>
        public SwapChainAlphaMode AlphaMode;

        /// <summary>
        /// Flags for the swap chain.
        /// </summary>
        public SwapChainFlags Flags;

        public override readonly bool Equals(object? obj)
        {
            return obj is SwapChainDescription description && Equals(description);
        }

        public readonly bool Equals(SwapChainDescription other)
        {
            return Width == other.Width &&
                   Height == other.Height &&
                   Format == other.Format &&
                   Stereo == other.Stereo &&
                   SampleDesc.Equals(other.SampleDesc) &&
                   BufferUsage == other.BufferUsage &&
                   BufferCount == other.BufferCount &&
                   Scaling == other.Scaling &&
                   SwapEffect == other.SwapEffect &&
                   AlphaMode == other.AlphaMode &&
                   Flags == other.Flags;
        }

        public override readonly int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(Width);
            hash.Add(Height);
            hash.Add(Format);
            hash.Add(Stereo);
            hash.Add(SampleDesc);
            hash.Add(BufferUsage);
            hash.Add(BufferCount);
            hash.Add(Scaling);
            hash.Add(SwapEffect);
            hash.Add(AlphaMode);
            hash.Add(Flags);
            return hash.ToHashCode();
        }

        public static bool operator ==(SwapChainDescription left, SwapChainDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SwapChainDescription left, SwapChainDescription right)
        {
            return !(left == right);
        }
    }
}