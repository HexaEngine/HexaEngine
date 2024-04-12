namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a render target view for a multisampled 2D texture.
    /// </summary>
    public struct Texture2DMultisampledRenderTargetView : IEquatable<Texture2DMultisampledRenderTargetView>
    {
        /// <summary>
        /// Unused field with nothing to define.
        /// </summary>
        public int UnusedFieldNothingToDefine;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture2DMultisampledRenderTargetView view && Equals(view);
        }

        public readonly bool Equals(Texture2DMultisampledRenderTargetView other)
        {
            return UnusedFieldNothingToDefine == other.UnusedFieldNothingToDefine;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(UnusedFieldNothingToDefine);
        }

        public static bool operator ==(Texture2DMultisampledRenderTargetView left, Texture2DMultisampledRenderTargetView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture2DMultisampledRenderTargetView left, Texture2DMultisampledRenderTargetView right)
        {
            return !(left == right);
        }
    }
}