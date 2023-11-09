namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies the primitive topology for rendering.
    /// </summary>
    public enum PrimitiveTopology : int
    {
        /// <summary>
        /// The primitive topology is undefined.
        /// </summary>
        Undefined = unchecked(0),

        /// <summary>
        /// A list of individual points.
        /// </summary>
        PointList = unchecked(1),

        /// <summary>
        /// A list of individual lines.
        /// </summary>
        LineList = unchecked(2),

        /// <summary>
        /// A strip of connected lines.
        /// </summary>
        LineStrip = unchecked(3),

        /// <summary>
        /// A list of individual triangles.
        /// </summary>
        TriangleList = unchecked(4),

        /// <summary>
        /// A strip of connected triangles.
        /// </summary>
        TriangleStrip = unchecked(5),

        /// <summary>
        /// A list of individual lines with adjacency information.
        /// </summary>
        LineListAdjacency = unchecked(10),

        /// <summary>
        /// A strip of connected lines with adjacency information.
        /// </summary>
        LineStripAdjacency = unchecked(11),

        /// <summary>
        /// A list of individual triangles with adjacency information.
        /// </summary>
        TriangleListAdjacency = unchecked(12),

        /// <summary>
        /// A strip of connected triangles with adjacency information.
        /// </summary>
        TriangleStripAdjacency = unchecked(13),

        /// <summary>
        /// A list of patches with 1 control point.
        /// </summary>
        PatchListWith1ControlPoints = unchecked(33),

        /// <summary>
        /// A list of patches with 2 control points.
        /// </summary>
        PatchListWith2ControlPoints = unchecked(34),

        /// <summary>
        /// A list of patches with 3 control points.
        /// </summary>
        PatchListWith3ControlPoints = unchecked(35),

        /// <summary>
        /// A list of patches with 4 control points.
        /// </summary>
        PatchListWith4ControlPoints = unchecked(36),

        /// <summary>
        /// A list of patches with 5 control points.
        /// </summary>
        PatchListWith5ControlPoints = unchecked(37),

        /// <summary>
        /// A list of patches with 6 control points.
        /// </summary>
        PatchListWith6ControlPoints = unchecked(38),

        /// <summary>
        /// A list of patches with 7 control points.
        /// </summary>
        PatchListWith7ControlPoints = unchecked(39),

        /// <summary>
        /// A list of patches with 8 control points.
        /// </summary>
        PatchListWith8ControlPoints = unchecked(40),

        /// <summary>
        /// A list of patches with 9 control points.
        /// </summary>
        PatchListWith9ControlPoints = unchecked(41),

        /// <summary>
        /// A list of patches with 10 control points.
        /// </summary>
        PatchListWith10ControlPoints = unchecked(42),

        /// <summary>
        /// A list of patches with 11 control points.
        /// </summary>
        PatchListWith11ControlPoints = unchecked(43),

        /// <summary>
        /// A list of patches with 12 control points.
        /// </summary>
        PatchListWith12ControlPoints = unchecked(44),

        /// <summary>
        /// A list of patches with 13 control points.
        /// </summary>
        PatchListWith13ControlPoints = unchecked(45),

        /// <summary>
        /// A list of patches with 14 control points.
        /// </summary>
        PatchListWith14ControlPoints = unchecked(46),

        /// <summary>
        /// A list of patches with 15 control points.
        /// </summary>
        PatchListWith15ControlPoints = unchecked(47),

        /// <summary>
        /// A list of patches with 16 control points.
        /// </summary>
        PatchListWith16ControlPoints = unchecked(48),

        /// <summary>
        /// A list of patches with 17 control points.
        /// </summary>
        PatchListWith17ControlPoints = unchecked(49),

        /// <summary>
        /// A list of patches with 18 control points.
        /// </summary>
        PatchListWith18ControlPoints = unchecked(50),

        /// <summary>
        /// A list of patches with 19 control points.
        /// </summary>
        PatchListWith19ControlPoints = unchecked(51),

        /// <summary>
        /// A list of patches with 20 control points.
        /// </summary>
        PatchListWith20ControlPoints = unchecked(52),

        /// <summary>
        /// A list of patches with 21 control points.
        /// </summary>
        PatchListWith21ControlPoints = unchecked(53),

        /// <summary>
        /// A list of patches with 22 control points.
        /// </summary>
        PatchListWith22ControlPoints = unchecked(54),

        /// <summary>
        /// A list of patches with 23 control points.
        /// </summary>
        PatchListWith23ControlPoints = unchecked(55),

        /// <summary>
        /// A list of patches with 24 control points.
        /// </summary>
        PatchListWith24ControlPoints = unchecked(56),

        /// <summary>
        /// A list of patches with 25 control points.
        /// </summary>
        PatchListWith25ControlPoints = unchecked(57),

        /// <summary>
        /// A list of patches with 26 control points.
        /// </summary>
        PatchListWith26ControlPoints = unchecked(58),

        /// <summary>
        /// A list of patches with 27 control points.
        /// </summary>
        PatchListWith27ControlPoints = unchecked(59),

        /// <summary>
        /// A list of patches with 28 control points.
        /// </summary>
        PatchListWith28ControlPoints = unchecked(60),

        /// <summary>
        /// A list of patches with 29 control points.
        /// </summary>
        PatchListWith29ControlPoints = unchecked(61),

        /// <summary>
        /// A list of patches with 30 control points.
        /// </summary>
        PatchListWith30ControlPoints = unchecked(62),

        /// <summary>
        /// A list of patches with 31 control points.
        /// </summary>
        PatchListWith31ControlPoints = unchecked(63),

        /// <summary>
        /// A list of patches with 32 control points.
        /// </summary>
        PatchListWith32ControlPoints = unchecked(64)
    }
}