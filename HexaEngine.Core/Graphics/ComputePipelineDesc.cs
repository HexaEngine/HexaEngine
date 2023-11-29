namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// Describes a compute pipeline.
    /// </summary>
    public struct ComputePipelineDesc : IEquatable<ComputePipelineDesc>
    {
        /// <summary>
        /// Gets or sets the path to the compute shader.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public string? Path;

        /// <summary>
        /// Gets or sets the entry point name for the compute shader.
        /// </summary>
        [XmlAttribute]
        [DefaultValue("main")]
        public string Entry = "main";

        /// <summary>
        /// Gets or sets the shader macros for the compute shader.
        /// </summary>
        public ShaderMacro[]? Macros;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputePipelineDesc"/> struct with default values.
        /// </summary>
        public ComputePipelineDesc()
        {
            Path = null;
            Entry = "main";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputePipelineDesc"/> struct with a specified shader path.
        /// </summary>
        /// <param name="path">The path to the compute shader.</param>
        public ComputePipelineDesc(string? path) : this()
        {
            Path = path;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputePipelineDesc"/> struct with specified shader path and entry point.
        /// </summary>
        /// <param name="path">The path to the compute shader.</param>
        /// <param name="entry">The entry point name for the compute shader.</param>
        public ComputePipelineDesc(string? path, string entry)
        {
            Path = path;
            Entry = entry;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is ComputePipelineDesc desc && Equals(desc);
        }

        /// <inheritdoc/>
        public readonly bool Equals(ComputePipelineDesc other)
        {
            return Path == other.Path &&
                   Entry == other.Entry &&
                   EqualityComparer<ShaderMacro[]?>.Default.Equals(Macros, other.Macros);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Path, Entry, Macros);
        }

        /// <summary>
        /// Determines whether two <see cref="ComputePipelineDesc"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="ComputePipelineDesc"/> to compare.</param>
        /// <param name="right">The second <see cref="ComputePipelineDesc"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="ComputePipelineDesc"/> instances are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(ComputePipelineDesc left, ComputePipelineDesc right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="ComputePipelineDesc"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="ComputePipelineDesc"/> to compare.</param>
        /// <param name="right">The second <see cref="ComputePipelineDesc"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="ComputePipelineDesc"/> instances are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(ComputePipelineDesc left, ComputePipelineDesc right)
        {
            return !(left == right);
        }
    }
}