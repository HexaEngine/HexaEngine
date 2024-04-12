namespace HexaEngine.Core.Graphics
{
    using System;

    /// <summary>
    /// Represents a shader resource view for a multisampled 2D texture.
    /// </summary>
    public struct Texture2DMultisampledShaderResourceView : IEquatable<Texture2DMultisampledShaderResourceView>
    {
        /// <summary>
        /// Unused field with nothing to define.
        /// </summary>
        public int UnusedFieldNothingToDefine;

        public override readonly bool Equals(object? obj)
        {
            return obj is Texture2DMultisampledShaderResourceView view && Equals(view);
        }

        public readonly bool Equals(Texture2DMultisampledShaderResourceView other)
        {
            return UnusedFieldNothingToDefine == other.UnusedFieldNothingToDefine;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(UnusedFieldNothingToDefine);
        }

        public static bool operator ==(Texture2DMultisampledShaderResourceView left, Texture2DMultisampledShaderResourceView right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Texture2DMultisampledShaderResourceView left, Texture2DMultisampledShaderResourceView right)
        {
            return !(left == right);
        }
    }
}