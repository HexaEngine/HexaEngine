namespace HexaEngine.Editor.TerrainEditor
{
    using HexaEngine.Core.IO;
    using HexaEngine.Meshes;

    public abstract class TerrainToolShape
    {
        public abstract bool TestCell(TerrainToolContext context, TerrainTool tool, TerrainCell cell);

        public abstract bool TestVertex(TerrainToolContext context, TerrainTool tool, TerrainVertex vertex, out float distance);
    }
}