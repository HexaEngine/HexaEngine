namespace HexaEngine.Lights
{
    public struct ClusterSizes
    {
        public uint GridSizeX;
        public uint GridSizeY;
        public uint GridSizeZ;
        public uint SizeX;

        public ClusterSizes(uint gridSizeX, uint gridSizeY, uint gridSizeZ, uint sizeX)
        {
            GridSizeX = gridSizeX;
            GridSizeY = gridSizeY;
            GridSizeZ = gridSizeZ;
            SizeX = sizeX;
        }
    }
}