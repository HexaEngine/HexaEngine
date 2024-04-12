namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// Describes the bytecode for a compute pipeline.
    /// </summary>
    public unsafe struct ComputePipelineBytecodeDesc : IEquatable<ComputePipelineBytecodeDesc>
    {
        /// <summary>
        /// Gets or sets the pointer to the shader bytecode.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public Shader* Shader = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputePipelineBytecodeDesc"/> struct.
        /// </summary>
        public ComputePipelineBytecodeDesc()
        {
        }

        public override readonly bool Equals(object? obj)
        {
            return obj is ComputePipelineBytecodeDesc desc && Equals(desc);
        }

        public readonly bool Equals(ComputePipelineBytecodeDesc other)
        {
            return (Shader == other.Shader);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine((nint)Shader);
        }

        public static bool operator ==(ComputePipelineBytecodeDesc left, ComputePipelineBytecodeDesc right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ComputePipelineBytecodeDesc left, ComputePipelineBytecodeDesc right)
        {
            return !(left == right);
        }
    }
}