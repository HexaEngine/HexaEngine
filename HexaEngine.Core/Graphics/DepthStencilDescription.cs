namespace HexaEngine.Core.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Xml.Serialization;

    /// <summary>
    /// Represents a description of a depth stencil state.
    /// </summary>
    public struct DepthStencilDescription : IEquatable<DepthStencilDescription>
    {
        /// <summary>
        /// The default stencil read mask value.
        /// </summary>
        public const int DefaultStencilReadMask = unchecked(255);

        /// <summary>
        /// The default stencil write mask value.
        /// </summary>
        public const int DefaultStencilWriteMask = unchecked(255);

        /// <summary>
        /// Gets or sets a value indicating whether depth testing is enabled.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(true)]
        public bool DepthEnable;

        /// <summary>
        /// Gets or sets the depth write mask, indicating which parts of the depth-stencil buffer can be modified by depth data.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(DepthWriteMask.All)]
        public DepthWriteMask DepthWriteMask;

        /// <summary>
        /// Gets or sets the comparison function used for depth testing.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(ComparisonFunction.LessEqual)]
        public ComparisonFunction DepthFunc;

        /// <summary>
        /// Gets or sets a value indicating whether stencil testing is enabled.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(false)]
        public bool StencilEnable;

        /// <summary>
        /// Gets or sets the stencil read mask, which identifies a portion of the depth-stencil buffer for reading stencil data.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(DefaultStencilReadMask)]
        public byte StencilReadMask;

        /// <summary>
        /// Gets or sets the stencil write mask, which identifies a portion of the depth-stencil buffer for writing stencil data.
        /// </summary>
        [XmlAttribute]
        [DefaultValue(DefaultStencilWriteMask)]
        public byte StencilWriteMask;

        /// <summary>
        /// Gets or sets the depth and stencil operation description for the front face.
        /// </summary>
        public DepthStencilOperationDescription FrontFace;

        /// <summary>
        /// Gets or sets the depth and stencil operation description for the back face.
        /// </summary>
        public DepthStencilOperationDescription BackFace;

        /// <summary>
        /// A built-in description with settings for not using a depth stencil buffer.
        /// </summary>
        public static readonly DepthStencilDescription None = new(false, DepthWriteMask.Zero);

        /// <summary>
        /// A built-in description with default settings for using a depth stencil buffer.
        /// </summary>
        public static readonly DepthStencilDescription Default = new(true, DepthWriteMask.All);

        /// <summary>
        /// A built-in description with default settings for using a depth stencil buffer. ComparisonFunction.Less
        /// </summary>
        public static readonly DepthStencilDescription DefaultLess = new(true, DepthWriteMask.All, ComparisonFunction.Less);

        /// <summary>
        /// A built-in description with default settings for using a depth stencil buffer.
        /// </summary>
        public static readonly DepthStencilDescription DefaultStencil = new(true, true, DepthWriteMask.All);

        /// <summary>
        /// A built-in description with settings for enabling a read-only depth stencil buffer.
        /// </summary>
        public static readonly DepthStencilDescription DepthRead = new(true, DepthWriteMask.Zero);

        /// <summary>
        /// A built-in description with settings for enabling a read-only depth stencil buffer. ComparisonFunction.Equal
        /// </summary>
        public static readonly DepthStencilDescription DepthReadEquals = new(true, DepthWriteMask.Zero, ComparisonFunction.Equal);

        /// <summary>
        /// A built-in description with settings for using a reverse depth stencil buffer.
        /// </summary>
        public static readonly DepthStencilDescription DepthReverseZ = new(true, DepthWriteMask.All, ComparisonFunction.GreaterEqual);

        /// <summary>
        /// A built-in description with settings for enabling a read-only reverse depth stencil buffer.
        /// </summary>
        public static readonly DepthStencilDescription DepthReadReverseZ = new(true, DepthWriteMask.Zero, ComparisonFunction.GreaterEqual);

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencilDescription"/> struct.
        /// </summary>
        /// <param name="depthEnable">Enable depth testing.</param>
        /// <param name="depthWriteMask">Identify a portion of the depth-stencil buffer that can be modified by depth data.</param>
        /// <param name="depthFunc">A function that compares depth data against existing depth data. </param>
        public DepthStencilDescription(bool depthEnable, DepthWriteMask depthWriteMask, ComparisonFunction depthFunc = ComparisonFunction.LessEqual)
        {
            DepthEnable = depthEnable;
            DepthWriteMask = depthWriteMask;
            DepthFunc = depthFunc;
            StencilEnable = false;
            StencilReadMask = DefaultStencilReadMask;
            StencilWriteMask = DefaultStencilWriteMask;
            FrontFace = DepthStencilOperationDescription.Default;
            BackFace = DepthStencilOperationDescription.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencilDescription"/> struct.
        /// </summary>
        /// <param name="depthEnable">Enable depth testing.</param>
        /// <param name="stencilEnable">Specifies whether to enable stencil testing. Set this member to <b>true</b> to enable stencil testing.</param>
        /// <param name="depthWriteMask">Identify a portion of the depth-stencil buffer that can be modified by depth data.</param>
        /// <param name="depthFunc">A function that compares depth data against existing depth data. </param>
        public DepthStencilDescription(bool depthEnable, bool stencilEnable, DepthWriteMask depthWriteMask, ComparisonFunction depthFunc = ComparisonFunction.LessEqual)
        {
            DepthEnable = depthEnable;
            DepthWriteMask = depthWriteMask;
            DepthFunc = depthFunc;
            StencilEnable = stencilEnable;
            StencilReadMask = DefaultStencilReadMask;
            StencilWriteMask = DefaultStencilWriteMask;
            FrontFace = DepthStencilOperationDescription.DefaultFront;
            BackFace = DepthStencilOperationDescription.DefaultBack;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthStencilDescription"/> struct.
        /// </summary>
        /// <param name="depthEnable">Specifies whether to enable depth testing. Set this member to <b>true</b> to enable depth testing.</param>
        /// <param name="depthWriteEnable">Specifies a _value that identifies a portion of the depth-stencil buffer that can be modified by depth data.</param>
        /// <param name="depthFunc">A <see cref="ComparisonFunction"/> _value that identifies a function that compares depth data against existing depth data.</param>
        /// <param name="stencilEnable">Specifies whether to enable stencil testing. Set this member to <b>true</b> to enable stencil testing.</param>
        /// <param name="stencilReadMask">Identify a portion of the depth-stencil buffer for reading stencil data.</param>
        /// <param name="stencilWriteMask">Identify a portion of the depth-stencil buffer for writing stencil data.</param>
        /// <param name="frontStencilFailOp"></param>
        /// <param name="frontStencilDepthFailOp"></param>
        /// <param name="frontStencilPassOp"></param>
        /// <param name="frontStencilFunc"></param>
        /// <param name="backStencilFailOp"></param>
        /// <param name="backStencilDepthFailOp"></param>
        /// <param name="backStencilPassOp"></param>
        /// <param name="backStencilFunc"></param>
        public DepthStencilDescription(
            bool depthEnable,
            bool depthWriteEnable,
            ComparisonFunction depthFunc,
            bool stencilEnable,
            byte stencilReadMask,
            byte stencilWriteMask,
            StencilOperation frontStencilFailOp,
            StencilOperation frontStencilDepthFailOp,
            StencilOperation frontStencilPassOp,
            ComparisonFunction frontStencilFunc,
            StencilOperation backStencilFailOp,
            StencilOperation backStencilDepthFailOp,
            StencilOperation backStencilPassOp,
            ComparisonFunction backStencilFunc)
        {
            DepthEnable = depthEnable;
            DepthWriteMask = depthWriteEnable ? DepthWriteMask.All : DepthWriteMask.Zero;
            DepthFunc = depthFunc;
            StencilEnable = stencilEnable;
            StencilReadMask = stencilReadMask;
            StencilWriteMask = stencilWriteMask;
            FrontFace.StencilFailOp = frontStencilFailOp;
            FrontFace.StencilDepthFailOp = frontStencilDepthFailOp;
            FrontFace.StencilPassOp = frontStencilPassOp;
            FrontFace.StencilFunc = frontStencilFunc;
            BackFace.StencilFailOp = backStencilFailOp;
            BackFace.StencilDepthFailOp = backStencilDepthFailOp;
            BackFace.StencilPassOp = backStencilPassOp;
            BackFace.StencilFunc = backStencilFunc;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj)
        {
            return obj is DepthStencilDescription description && Equals(description);
        }

        /// <inheritdoc/>
        public readonly bool Equals(DepthStencilDescription other)
        {
            return DepthEnable == other.DepthEnable &&
                   DepthWriteMask == other.DepthWriteMask &&
                   DepthFunc == other.DepthFunc &&
                   StencilEnable == other.StencilEnable &&
                   StencilReadMask == other.StencilReadMask &&
                   StencilWriteMask == other.StencilWriteMask &&
                   EqualityComparer<DepthStencilOperationDescription>.Default.Equals(FrontFace, other.FrontFace) &&
                   EqualityComparer<DepthStencilOperationDescription>.Default.Equals(BackFace, other.BackFace);
        }

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(DepthEnable, DepthWriteMask, DepthFunc, StencilEnable, StencilReadMask, StencilWriteMask, FrontFace, BackFace);
        }

        /// <summary>
        /// Determines whether two <see cref="DepthStencilDescription"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="DepthStencilDescription"/> to compare.</param>
        /// <param name="right">The second <see cref="DepthStencilDescription"/> to compare.</param>
        /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(DepthStencilDescription left, DepthStencilDescription right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="DepthStencilDescription"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="DepthStencilDescription"/> to compare.</param>
        /// <param name="right">The second <see cref="DepthStencilDescription"/> to compare.</param>
        /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(DepthStencilDescription left, DepthStencilDescription right)
        {
            return !(left == right);
        }
    }
}