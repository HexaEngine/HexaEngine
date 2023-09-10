namespace HexaEngine.Core.IO.Terrains
{
    public class TerrainFile
    {
        public TerrainHeader Header;
        public HeightMap HeightMap;
        public TerrainCellData[] Cells;
    }
}