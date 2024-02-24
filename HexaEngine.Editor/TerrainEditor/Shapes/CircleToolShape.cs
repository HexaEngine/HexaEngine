namespace HexaEngine.Editor.TerrainEditor.Shapes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Editor.TerrainEditor;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using System.Numerics;

    public class CircleToolShape : TerrainToolShape
    {
        public override bool TestCell(TerrainToolContext context, TerrainTool tool, TerrainCell cell)
        {
            BoundingBox global = BoundingBox.Transform(cell.BoundingBox, cell.Transform);
            // we ignore the y axis.
            global.Max.Y = float.MaxValue;
            global.Min.Y = float.MinValue;
            BoundingSphere sphere = new(context.Position, tool.Size);
            return global.Intersects(sphere);
        }

        public override bool TestVertex(TerrainToolContext context, TerrainTool tool, TerrainVertex vertex, out float distance)
        {
            var vertexWS = Vector3.Transform(vertex.Position, context.Transform);
            Vector3 p1 = new(vertexWS.X, 0, vertexWS.Z);
            Vector3 p2 = new(context.Position.X, 0, context.Position.Z);

            distance = Vector3.Distance(p2, p1);

            if (distance > tool.Size)
            {
                return false;
            }
            return true;
        }
    }
}