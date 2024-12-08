namespace HexaEngine.Editor.TerrainEditor.Tools
{
    using Hexa.NET.Mathematics;
    using System.Numerics;

    public struct CBChannelRemap
    {
        public UPoint2 TextureSize;
        public uint SourceChannel;
        public uint DestinationChannel;
        public Vector4 Factor;

        public CBChannelRemap(UPoint2 textureSize, uint sourceChannel, uint destinationChannel, Vector4 factor)
        {
            TextureSize = textureSize;
            SourceChannel = sourceChannel;
            DestinationChannel = destinationChannel;
            Factor = factor;
        }
    }
}