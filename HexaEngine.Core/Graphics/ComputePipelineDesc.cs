namespace HexaEngine.Core.Graphics
{
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// Describes a compute pipeline.
    /// </summary>
    public struct ComputePipelineDesc
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
    }
}