namespace HexaEngine.Editor.TerrainEditor
{
    using HexaEngine.Meshes;
    using System.Numerics;

    public struct CBColorMask
    {
        public Vector4 Mask;

        public CBColorMask(ChannelMask mask)
        {
            Mask = new(-1);
            if ((mask & ChannelMask.R) != 0)
            {
                Mask.X = 1;
            }
            if ((mask & ChannelMask.G) != 0)
            {
                Mask.Y = 1;
            }
            if ((mask & ChannelMask.B) != 0)
            {
                Mask.Z = 1;
            }
            if ((mask & ChannelMask.A) != 0)
            {
                Mask.W = 1;
            }
        }
    }
}