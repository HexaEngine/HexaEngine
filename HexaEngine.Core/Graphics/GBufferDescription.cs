namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Describes the configuration for a G-Buffer.
    /// </summary>
    public struct GBufferDescription : IEquatable<GBufferDescription>
    {
        /// <summary>
        /// The width of the G-Buffer textures.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the G-Buffer textures.
        /// </summary>
        public int Height;

        /// <summary>
        /// The number of textures in the G-Buffer.
        /// </summary>
        public int Count;

        /// <summary>
        /// An array of texture formats for the G-Buffer.
        /// </summary>
        public Format[] Formats;

        /// <summary>
        /// Initializes a new instance of the <see cref="GBufferDescription"/> struct with the specified parameters.
        /// </summary>
        /// <param name="width">The width of the G-Buffer textures.</param>
        /// <param name="height">The height of the G-Buffer textures.</param>
        /// <param name="count">The number of textures in the G-Buffer.</param>
        /// <param name="formats">An array of texture formats for the G-Buffer.</param>
        public GBufferDescription(int width, int height, int count, params Format[] formats)
        {
            Width = width;
            Height = height;
            Count = count;
            Formats = formats;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is GBufferDescription description && Equals(description);
        }

        public readonly bool Equals(GBufferDescription other)
        {
            return Width == other.Width &&
                   Height == other.Height &&
                   Count == other.Count &&
                   EqualityComparer<Format[]>.Default.Equals(Formats, other.Formats);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Width, Height, Count, Formats);
        }

        public static bool operator ==(GBufferDescription left, GBufferDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GBufferDescription left, GBufferDescription right)
        {
            return !(left == right);
        }
    }
}