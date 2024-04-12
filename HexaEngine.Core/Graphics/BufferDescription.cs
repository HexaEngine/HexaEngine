namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Xml.Serialization;

    /// <summary>
    /// Describes a buffer resource, which represents a contiguous block of memory.
    /// </summary>
    public struct BufferDescription : IEquatable<BufferDescription>
    {
        /// <summary>
        /// Gets or sets the size of the buffer in bytes.
        /// </summary>
        [XmlAttribute]
        public int ByteWidth;

        /// <summary>
        /// Gets or sets the intended usage of the buffer.
        /// </summary>
        [XmlAttribute]
        public Usage Usage;

        /// <summary>
        /// Gets or sets the bind flags that specify how the buffer is bound to the pipeline stages.
        /// </summary>
        [XmlAttribute]
        public BindFlags BindFlags;

        /// <summary>
        /// Gets or sets the CPU access flags that determine how the CPU can access the buffer data.
        /// </summary>
        [XmlAttribute]
        public CpuAccessFlags CPUAccessFlags;

        /// <summary>
        /// Gets or sets optional flags that can be used to specify additional resource options.
        /// </summary>
        [XmlAttribute]
        public ResourceMiscFlag MiscFlags;

        /// <summary>
        /// Gets or sets the stride between elements (in bytes) when using structured buffers.
        /// </summary>
        [XmlAttribute]
        public int StructureByteStride;

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferDescription"/> struct with the specified parameters.
        /// </summary>
        /// <param name="byteWidth">The size of the buffer in bytes.</param>
        /// <param name="bindFlags">The bind flags that specify how the buffer is bound to the pipeline stages.</param>
        /// <param name="usage">The intended usage of the buffer (default is <see cref="Usage.Default"/>).</param>
        /// <param name="cpuAccessFlags">The CPU access flags that determine how the CPU can access the buffer data (default is <see cref="CpuAccessFlags.None"/>).</param>
        /// <param name="miscFlags">Optional flags that can be used to specify additional resource options (default is <see cref="ResourceMiscFlag.None"/>).</param>
        /// <param name="structureByteStride">The stride between elements (in bytes) when using structured buffers (default is 0).</param>
        public BufferDescription(int byteWidth,
            BindFlags bindFlags,
            Usage usage = Usage.Default,
            CpuAccessFlags cpuAccessFlags = CpuAccessFlags.None,
            ResourceMiscFlag miscFlags = ResourceMiscFlag.None,
            int structureByteStride = 0)
        {
            ByteWidth = byteWidth;
            BindFlags = bindFlags;
            Usage = usage;
            CPUAccessFlags = cpuAccessFlags;
            MiscFlags = miscFlags;
            StructureByteStride = structureByteStride;
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is BufferDescription description && Equals(description);
        }

        public readonly bool Equals(BufferDescription other)
        {
            return ByteWidth == other.ByteWidth &&
                   Usage == other.Usage &&
                   BindFlags == other.BindFlags &&
                   CPUAccessFlags == other.CPUAccessFlags &&
                   MiscFlags == other.MiscFlags &&
                   StructureByteStride == other.StructureByteStride;
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(ByteWidth, Usage, BindFlags, CPUAccessFlags, MiscFlags, StructureByteStride);
        }

        public static bool operator ==(BufferDescription left, BufferDescription right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BufferDescription left, BufferDescription right)
        {
            return !(left == right);
        }
    }
}