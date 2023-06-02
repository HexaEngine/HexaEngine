namespace HexaEngine.Core.IO.Animations
{
    using HexaEngine.Mathematics;

    public struct MeshMorphKeyframe
    {
        public double Time;
        public uint[] Values;
        public double[] Weights;

        public void Write(Stream stream, Endianness endianness)
        {
            stream.WriteDouble(Time, endianness);
            stream.WriteInt(Values.Length, endianness);
            for (int i = 0; i < Values.Length; i++)
            {
                stream.WriteUInt(Values[i], endianness);
            }
            stream.WriteInt(Weights.Length, endianness);
            for (int i = 0; i < Weights.Length; i++)
            {
                stream.WriteDouble(Weights[i], endianness);
            }
        }

        public void Read(Stream stream, Endianness endianness)
        {
            Time = stream.ReadDouble(endianness);
            int valueCount = stream.ReadInt(endianness);
            Values = new uint[valueCount];
            for (int i = 0; i < valueCount; i++)
            {
                Values[i] = stream.ReadUInt(endianness);
            }
            int weightCount = stream.ReadInt(endianness);
            Weights = new double[weightCount];
            for (int i = 0; i < weightCount; i++)
            {
                Weights[i] = stream.ReadDouble(endianness);
            }
        }

        public static MeshMorphKeyframe ReadFrom(Stream stream, Endianness endianness)
        {
            MeshMorphKeyframe keyframe = default;
            keyframe.Read(stream, endianness);
            return keyframe;
        }
    }
}