namespace HexaEngine.Core.Graphics
{
    using System;
    using System.ComponentModel;
    using System.Xml;
    using System.Xml.Serialization;

    public struct SampleDescription : IEquatable<SampleDescription>
    {
        [XmlAttribute]
        [DefaultValue(1)]
        public int Count;

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

        public override string ToString() => $"Count: {Count}, Quality: {Quality}";

        public override bool Equals(object? obj)
        {
            return obj is SampleDescription description && Equals(description);
        }

        public bool Equals(SampleDescription other)
        {
            return Count == other.Count &&
                   Quality == other.Quality;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Count, Quality);
        }

        public static bool operator ==(SampleDescription left, SampleDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SampleDescription left, SampleDescription right)
        {
            return !(left == right);
        }
    }
}