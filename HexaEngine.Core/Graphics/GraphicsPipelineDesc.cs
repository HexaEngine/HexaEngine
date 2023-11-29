namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// Represents a description of a graphics pipeline.
    /// </summary>
    public struct GraphicsPipelineDesc : IEquatable<GraphicsPipelineDesc>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsPipelineDesc"/> struct.
        /// </summary>
        public GraphicsPipelineDesc()
        {
        }

        /// <summary>
        /// Gets or sets the path to the vertex shader file.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public string? VertexShader = null;

        /// <summary>
        /// Gets or sets the entry point for the vertex shader.
        /// </summary>
        [XmlAttribute]
        [DefaultValue("main")]
        public string VertexShaderEntrypoint = "main";

        /// <summary>
        /// Gets or sets the path to the hull shader file.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public string? HullShader = null;

        /// <summary>
        /// Gets or sets the entry point for the hull shader.
        /// </summary>
        [XmlAttribute]
        [DefaultValue("main")]
        public string HullShaderEntrypoint = "main";

        /// <summary>
        /// Gets or sets the path to the domain shader file.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public string? DomainShader = null;

        /// <summary>
        /// Gets or sets the entry point for the domain shader.
        /// </summary>
        [XmlAttribute]
        [DefaultValue("main")]
        public string DomainShaderEntrypoint = "main";

        /// <summary>
        /// Gets or sets the path to the geometry shader file.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public string? GeometryShader = null;

        /// <summary>
        /// Gets or sets the entry point for the geometry shader.
        /// </summary>
        [XmlAttribute]
        [DefaultValue("main")]
        public string GeometryShaderEntrypoint = "main";

        /// <summary>
        /// Gets or sets the path to the pixel shader file.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public string? PixelShader = null;

        /// <summary>
        /// Gets or sets the entry point for the pixel shader.
        /// </summary>
        [XmlAttribute]
        [DefaultValue("main")]
        public string PixelShaderEntrypoint = "main";

        /// <summary>
        /// Gets or sets the graphics pipeline state.
        /// </summary>
        public GraphicsPipelineState State;

        /// <summary>
        /// Gets or sets the macros of the graphics pipeline.
        /// </summary>
        public ShaderMacro[]? Macros;

        /// <summary>
        /// Gets or sets the input elements of the graphics pipeline.
        /// </summary>
        public InputElementDescription[]? InputElements;

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is GraphicsPipelineDesc desc && Equals(desc);
        }

        /// <inheritdoc/>
        public readonly bool Equals(GraphicsPipelineDesc other)
        {
            return VertexShader == other.VertexShader &&
                   VertexShaderEntrypoint == other.VertexShaderEntrypoint &&
                   HullShader == other.HullShader &&
                   HullShaderEntrypoint == other.HullShaderEntrypoint &&
                   DomainShader == other.DomainShader &&
                   DomainShaderEntrypoint == other.DomainShaderEntrypoint &&
                   GeometryShader == other.GeometryShader &&
                   GeometryShaderEntrypoint == other.GeometryShaderEntrypoint &&
                   PixelShader == other.PixelShader &&
                   PixelShaderEntrypoint == other.PixelShaderEntrypoint &&
                   State.Equals(other.State) &&
                   EqualityComparer<ShaderMacro[]?>.Default.Equals(Macros, other.Macros) &&
                   EqualityComparer<InputElementDescription[]?>.Default.Equals(InputElements, other.InputElements);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(VertexShader);
            hash.Add(VertexShaderEntrypoint);
            hash.Add(HullShader);
            hash.Add(HullShaderEntrypoint);
            hash.Add(DomainShader);
            hash.Add(DomainShaderEntrypoint);
            hash.Add(GeometryShader);
            hash.Add(GeometryShaderEntrypoint);
            hash.Add(PixelShader);
            hash.Add(PixelShaderEntrypoint);
            hash.Add(State);
            hash.Add(Macros);
            hash.Add(InputElements);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Determines whether two <see cref="GraphicsPipelineDesc"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="GraphicsPipelineDesc"/> to compare.</param>
        /// <param name="right">The second <see cref="GraphicsPipelineDesc"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="GraphicsPipelineDesc"/> instances are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(GraphicsPipelineDesc left, GraphicsPipelineDesc right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="GraphicsPipelineDesc"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="GraphicsPipelineDesc"/> to compare.</param>
        /// <param name="right">The second <see cref="GraphicsPipelineDesc"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="GraphicsPipelineDesc"/> instances are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(GraphicsPipelineDesc left, GraphicsPipelineDesc right)
        {
            return !(left == right);
        }
    }
}