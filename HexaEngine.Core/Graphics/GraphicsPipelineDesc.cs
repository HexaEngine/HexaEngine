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
        /// Gets or sets the macros of the graphics pipeline.
        /// </summary>
        public ShaderMacro[]? Macros;

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
                   EqualityComparer<ShaderMacro[]?>.Default.Equals(Macros, other.Macros);
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
            hash.Add(Macros);
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

        public static implicit operator GraphicsPipelineDescEx(GraphicsPipelineDesc desc)
        {
            GraphicsPipelineDescEx result;
            result.VertexShader = desc.VertexShader != null ? ShaderSource.FromFile(desc.VertexShader) : null;
            result.HullShader = desc.HullShader != null ? ShaderSource.FromFile(desc.HullShader) : null;
            result.DomainShader = desc.DomainShader != null ? ShaderSource.FromFile(desc.DomainShader) : null;
            result.GeometryShader = desc.GeometryShader != null ? ShaderSource.FromFile(desc.GeometryShader) : null;
            result.PixelShader = desc.PixelShader != null ? ShaderSource.FromFile(desc.PixelShader) : null;
            result.VertexShaderEntrypoint = desc.VertexShaderEntrypoint;
            result.HullShaderEntrypoint = desc.HullShaderEntrypoint;
            result.DomainShaderEntrypoint = desc.DomainShaderEntrypoint;
            result.GeometryShaderEntrypoint = desc.GeometryShaderEntrypoint;
            result.PixelShaderEntrypoint = desc.PixelShaderEntrypoint;
            result.Macros = desc.Macros;
            return result;
        }
    }

    /// <summary>
    /// Represents a description of a graphics pipeline.
    /// </summary>
    public struct GraphicsPipelineDescEx : IEquatable<GraphicsPipelineDescEx>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsPipelineDescEx"/> struct.
        /// </summary>
        public GraphicsPipelineDescEx()
        {
        }

        /// <summary>
        /// Gets or sets the path to the vertex shader file.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(null)]
        public ShaderSource? VertexShader = null;

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
        public ShaderSource? HullShader = null;

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
        public ShaderSource? DomainShader = null;

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
        public ShaderSource? GeometryShader = null;

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
        public ShaderSource? PixelShader = null;

        /// <summary>
        /// Gets or sets the entry point for the pixel shader.
        /// </summary>
        [XmlAttribute]
        [DefaultValue("main")]
        public string PixelShaderEntrypoint = "main";

        /// <summary>
        /// Gets or sets the macros of the graphics pipeline.
        /// </summary>
        public ShaderMacro[]? Macros;

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is GraphicsPipelineDescEx desc && Equals(desc);
        }

        /// <inheritdoc/>
        public readonly bool Equals(GraphicsPipelineDescEx other)
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
                   EqualityComparer<ShaderMacro[]?>.Default.Equals(Macros, other.Macros);
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
            hash.Add(Macros);
            return hash.ToHashCode();
        }

        /// <summary>
        /// Determines whether two <see cref="GraphicsPipelineDescEx"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="GraphicsPipelineDescEx"/> to compare.</param>
        /// <param name="right">The second <see cref="GraphicsPipelineDescEx"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="GraphicsPipelineDescEx"/> instances are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(GraphicsPipelineDescEx left, GraphicsPipelineDescEx right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="GraphicsPipelineDescEx"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="GraphicsPipelineDescEx"/> to compare.</param>
        /// <param name="right">The second <see cref="GraphicsPipelineDescEx"/> to compare.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="GraphicsPipelineDescEx"/> instances are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(GraphicsPipelineDescEx left, GraphicsPipelineDescEx right)
        {
            return !(left == right);
        }
    }
}