namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a depth stencil view for a multisampled 2D texture.
    /// </summary>
    public struct Texture2DMultisampledDepthStencilView : IEquatable<Texture2DMultisampledDepthStencilView>
    {
        /// <summary>
        /// Unused field with nothing to define.
        /// </summary>
        public int UnusedFieldNothingToDefine;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture2DMultisampledDepthStencilView view && Equals(view);
        }

        public readonly bool Equals(Texture2DMultisampledDepthStencilView other)
        {
            return UnusedFieldNothingToDefine == other.UnusedFieldNothingToDefine;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(UnusedFieldNothingToDefine);
        }

        public static bool operator ==(Texture2DMultisampledDepthStencilView left, Texture2DMultisampledDepthStencilView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture2DMultisampledDepthStencilView left, Texture2DMultisampledDepthStencilView right)
        {
            return !(left == right);
        }
    }
}