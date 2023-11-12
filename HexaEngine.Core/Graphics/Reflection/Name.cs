namespace HexaEngine.Core.Graphics.Reflection
{
    /// <summary>
    /// Represents symbolic names for input and output semantic indices in HLSL.
    /// </summary>
    public enum Name
    {
        /// <summary>
        /// Undefined semantic.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Position semantic.
        /// </summary>
        Position = 1,

        /// <summary>
        /// Clip Distance semantic.
        /// </summary>
        ClipDistance = 2,

        /// <summary>
        /// Cull Distance semantic.
        /// </summary>
        CullDistance = 3,

        /// <summary>
        /// Render Target Array Index semantic.
        /// </summary>
        RenderTargetArrayIndex = 4,

        /// <summary>
        /// Viewport Array Index semantic.
        /// </summary>
        ViewportArrayIndex = 5,

        /// <summary>
        /// Vertex ID semantic.
        /// </summary>
        VertexID = 6,

        /// <summary>
        /// Primitive ID semantic.
        /// </summary>
        PrimitiveID = 7,

        /// <summary>
        /// Instance ID semantic.
        /// </summary>
        InstanceID = 8,

        /// <summary>
        /// Is Front Face semantic.
        /// </summary>
        IsFrontFace = 9,

        /// <summary>
        /// Sample Index semantic.
        /// </summary>
        SampleIndex = 10,

        /// <summary>
        /// Final Quad Edge Tessellation Factor semantic.
        /// </summary>
        FinalQuadEdgeTessfactor = 11,

        /// <summary>
        /// Final Quad Inside Tessellation Factor semantic.
        /// </summary>
        FinalQuadInsideTessfactor = 12,

        /// <summary>
        /// Final Tri Edge Tessellation Factor semantic.
        /// </summary>
        FinalTriEdgeTessfactor = 13,

        /// <summary>
        /// Final Tri Inside Tessellation Factor semantic.
        /// </summary>
        FinalTriInsideTessfactor = 14,

        /// <summary>
        /// Final Line Detail Tessellation Factor semantic.
        /// </summary>
        FinalLineDetailTessfactor = 0xF,

        /// <summary>
        /// Final Line Density Tessellation Factor semantic.
        /// </summary>
        FinalLineDensityTessfactor = 0x10,

        /// <summary>
        /// Barycentrics semantic.
        /// </summary>
        Barycentrics = 23,

        /// <summary>
        /// Shading Rate semantic.
        /// </summary>
        Shadingrate = 24,

        /// <summary>
        /// Cull Primitive semantic.
        /// </summary>
        Cullprimitive = 25,

        /// <summary>
        /// Target semantic.
        /// </summary>
        Target = 0x40,

        /// <summary>
        /// Depth semantic.
        /// </summary>
        Depth = 65,

        /// <summary>
        /// Coverage semantic.
        /// </summary>
        Coverage = 66,

        /// <summary>
        /// Depth Greater Equal semantic.
        /// </summary>
        DepthGreaterEqual = 67,

        /// <summary>
        /// Depth Less Equal semantic.
        /// </summary>
        DepthLessEqual = 68,

        /// <summary>
        /// Stencil Reference semantic.
        /// </summary>
        StencilRef = 69,

        /// <summary>
        /// Inner Coverage semantic.
        /// </summary>
        InnerCoverage = 70,
    }
}