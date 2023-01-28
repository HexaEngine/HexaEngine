namespace HexaEngine.Core.Graphics
{
    using System.ComponentModel;

    public struct RasterizerDescription
    {
        public const int DefaultDepthBias = unchecked(0);
        public const float DefaultDepthBiasClamp = unchecked((float)0.0F);
        public const float DefaultSlopeScaledDepthBias = unchecked((float)0.0F);

        [DefaultValue(FillMode.Solid)]
        public FillMode FillMode;

        [DefaultValue(CullMode.Back)]
        public CullMode CullMode;

        [DefaultValue(false)]
        public bool FrontCounterClockwise;

        [DefaultValue(DefaultDepthBias)]
        public int DepthBias;

        [DefaultValue(DefaultDepthBiasClamp)]
        public float DepthBiasClamp;

        [DefaultValue(DefaultSlopeScaledDepthBias)]
        public float SlopeScaledDepthBias;

        [DefaultValue(true)]
        public bool DepthClipEnable;

        [DefaultValue(false)]
        public bool ScissorEnable;

        [DefaultValue(true)]
        public bool MultisampleEnable;

        [DefaultValue(false)]
        public bool AntialiasedLineEnable;

        /// <summary>
        /// A built-in description with settings with settings for not culling any primitives.
        /// </summary>
        public static readonly RasterizerDescription CullNone = new(CullMode.None, FillMode.Solid);

        /// <summary>
        /// A built-in description with settings for culling primitives with clockwise winding order.
        /// </summary>
        public static readonly RasterizerDescription CullFront = new(CullMode.Front, FillMode.Solid);

        /// <summary>
        /// A built-in description with settings for culling primitives with counter-clockwise winding order.
        /// </summary>
        public static readonly RasterizerDescription CullBack = new(CullMode.Back, FillMode.Solid);

        /// <summary>
        /// A built-in description with settings for not culling any primitives and wireframe fill mode.
        /// </summary>
        public static readonly RasterizerDescription Wireframe = new(CullMode.None, FillMode.Wireframe);

        /// <summary>
        /// Initializes a new instance of the <see cref="RasterizerDescription"/> class.
        /// </summary>
        /// <param name="cullMode">A <see cref="CullMode"/> value that specifies that triangles facing the specified direction are not drawn..</param>
        /// <param name="fillMode">A <see cref="FillMode"/> value that specifies the fill mode to use when rendering.</param>
        public RasterizerDescription(CullMode cullMode, FillMode fillMode)
        {
            CullMode = cullMode;
            FillMode = fillMode;
            FrontCounterClockwise = false;
            DepthBias = DefaultDepthBias;
            DepthBiasClamp = DefaultDepthBiasClamp;
            SlopeScaledDepthBias = DefaultSlopeScaledDepthBias;
            DepthClipEnable = true;
            ScissorEnable = false;
            MultisampleEnable = true;
            AntialiasedLineEnable = false;
        }
    }
}