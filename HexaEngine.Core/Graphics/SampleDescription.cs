namespace HexaEngine.Core.Graphics
{
    using System;
    using System.ComponentModel;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Describes the number of multisamples and the quality of the samples.
    /// </summary>
    public struct SampleDescription : IEquatable<SampleDescription>
    {
        /// <summary>
        /// Gets or sets the number of samples per pixel.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(1)]
        public int Count;

        /// <summary>
        /// Gets or sets the image quality level. The higher the quality, the lower the performance.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(0)]
        public int Quality;

        /// <summary>
        /// A <see cref="SampleDescription"/> with Count=1 and Quality=0.
        /// </summary>
        public static readonly SampleDescription Default = new(1, 0);

        /// <summary>
        /// Create new instance of <see cref="SampleDescription"/> struct.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="quality"></param>
        public SampleDescription(int count, int quality)
        {
            Count = count;
            Quality = quality;
        }

        /// <inheritdoc/>
        public override readonly string ToString() => $"Count: {Count}, Quality: {Quality}";

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is SampleDescription description && Equals(description);
        }

        /// <inheritdoc/>
        public readonly bool Equals(SampleDescription other)
        {
            return Count == other.Count &&
                   Quality == other.Quality;
        }

        /// <inheritdoc/>
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(Count, Quality);
        }

        /// <summary>
        /// Determines whether two <see cref="SampleDescription"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="SampleDescription"/> to compare.</param>
        /// <param name="right">The second <see cref="SampleDescription"/> to compare.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="SampleDescription"/> instances are equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(SampleDescription left, SampleDescription right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="SampleDescription"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="SampleDescription"/> to compare.</param>
        /// <param name="right">The second <see cref="SampleDescription"/> to compare.</param>
        /// <returns>
        /// <see langword="true"/> if the specified <see cref="SampleDescription"/> instances are not equal; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(SampleDescription left, SampleDescription right)
        {
            return !(left == right);
        }
    }
}