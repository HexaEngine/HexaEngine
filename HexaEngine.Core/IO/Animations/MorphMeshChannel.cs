namespace HexaEngine.Core.IO.Animations
{
    using HexaEngine.Mathematics;
    using System.Text;

    public struct MorphMeshChannel
    {
        public string MeshName;
        public List<MeshMorphKeyframe> Keyframes = new();

        public MorphMeshChannel(string meshName) : this()
        {
            MeshName = meshName;
        }

        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(MeshName, encoding, endianness);
            stream.WriteInt32(Keyframes.Count, endianness);
            for (int i = 0; i < Keyframes.Count; i++)
            {
                Keyframes[i].Write(stream, endianness);
            }
        }

        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            MeshName = stream.ReadString(encoding, endianness) ?? string.Empty;
            var keyframeCount = stream.ReadInt32(endianness);
            Keyframes = new(keyframeCount);
            for (int i = 0; i < keyframeCount; i++)
            {
                Keyframes.Add(MeshMorphKeyframe.ReadFrom(stream, endianness));
            }
        }

        public static MorphMeshChannel ReadFrom(Stream stream, Encoding encoding, Endianness endianness)
        {
            MorphMeshChannel channel = default;
            channel.Read(stream, encoding, endianness);
            return channel;
        }
    }
}