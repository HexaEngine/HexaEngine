namespace HexaEngine.Editor.TerrainEditor.Shapes
{
    using HexaEngine.Core.IO;
    using HexaEngine.Editor.TerrainEditor;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using System.Numerics;

    public class SquareToolShape : TerrainToolShape
    {
        public override bool TestCell(TerrainToolContext context, TerrainTool tool, TerrainCell cell)
        {
            BoundingBox global = BoundingBox.Transform(cell.BoundingBox, cell.Transform);
            // we ignore the y axis.
            global.Max.Y = float.MaxValue;
            global.Min.Y = float.MinValue;

            Vector3 sizeVec = new(tool.Size);
            BoundingBox box = new(context.Position - sizeVec, context.Position + sizeVec);
            return global.Intersects(box);
        }

        public override bool TestVertex(TerrainToolContext context, TerrainTool tool, TerrainVertex vertex, out float distance)
        {
            Vector3 vertexWS = Vector3.Transform(vertex.Position, context.Transform);

            float deltaX = Math.Abs(vertexWS.X - context.Position.X);
            float deltaZ = Math.Abs(vertexWS.Z - context.Position.Z);

            distance = Math.Max(deltaX, deltaZ);

            return distance <= tool.Size;
        }
    }
}