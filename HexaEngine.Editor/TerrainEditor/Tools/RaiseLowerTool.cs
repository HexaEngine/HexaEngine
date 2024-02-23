namespace HexaEngine.Editor.TerrainEditor.Tools
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core;
    using HexaEngine.Editor.TerrainEditor;

    public class RaiseLowerTool : TerrainTool
    {
        private bool raise = true;

        public bool Raise { get => raise; set => raise = value; }

        public override string Name { get; } = "Raise/Lower";

        public override void DrawSettings()
        {
            ImGui.Checkbox("Raise", ref raise);
        }

        public override bool Modify(TerrainToolContext context)
        {
            bool hasAffected = false;
            for (int j = 0; j < context.VertexCount; j++)
            {
                if (!context.TestVertex(this, j, out var vertex, out var distance))
                {
                    continue;
                }

                float edgeFade = ComputeEdgeFade(distance);
                float value = Strength * edgeFade * Time.Delta;

                uint index = context.GetHeightMapIndex(vertex, out var heightMap);

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