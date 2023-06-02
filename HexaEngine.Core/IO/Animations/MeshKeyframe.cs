namespace HexaEngine.Core.IO.Animations
{
    using HexaEngine.Mathematics;
    using System.Text;

    public struct MeshKeyframe
    {
        public string MeshName;
        public double Time;

        public void Write(Stream stream, Encoding encoding, Endianness endianness)
        {
            stream.WriteString(MeshName, encoding, endianness);
            stream.WriteDouble(Time, endianness);
        }

        public void Read(Stream stream, Encoding encoding, Endianness endianness)
        {
            MeshName = stream.ReadString(encoding, endianness) ?? string.Empty;
            Time = stream.ReadDouble(endianness);
        }

        public static MeshKeyframe ReadFrom(Stream stream, Encoding encoding, Endianness endianness)
        {
            MeshKeyframe keyframe = default;
            keyframe.Read(stream, encoding, endianness);
            return keyframe;
        }
    }
}