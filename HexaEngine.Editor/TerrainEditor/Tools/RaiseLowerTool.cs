namespace HexaEngine.Editor.TerrainEditor.Tools
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.TerrainEditor;

    public class RaiseLowerTool : TerrainTool
    {
        private bool raise = true;

        public bool Raise { get => raise; set => raise = value; }

        public override string Name { get; } = "Raise/Lower";

        public override bool DrawSettings(TerrainToolContext toolContext)
        {
            ImGui.Checkbox("Raise", ref raise);
            return false;
        }

        public override bool Modify(IGraphicsContext context, TerrainToolContext toolContext)
        {
            bool hasAffected = false;
            for (int j = 0; j < toolContext.VertexCount; j++)
            {
                if (!toolContext.TestVertex(this, j, out var vertex, out var distance))
                {
                    continue;
                }

                float edgeFade = ComputeEdgeFade(distance);
                float value = Strength * edgeFade * Time.Delta;

                uint index = toolContext.GetHeightMapIndex(vertex, out var heightMap);

                if (raise)
                {
                    heightMap[index] += value;
                }
                else
                {
                    heightMap[index] -= value;
                }

                hasAffected = true;
            }

            return hasAffected;
        }
    }
}