namespace HexaEngine.Core.IO.Animations
{
    using HexaEngine.Mathematics;
    using System.Numerics;

    public struct VectorKeyframe
    {
        public Vector3 Value;
        public double Time;

        public static VectorKeyframe ReadFrom(Stream stream, Endianness endianness)
        {
            VectorKeyframe keyframe = default;
            keyframe.Read(stream, endianness);
            return keyframe;
        }

        public void Write(Stream stream, Endianness endianness)
        {
            stream.WriteVector3(Value, endianness);
            stream.WriteDouble(Time, endianness);
        }

        public void Read(Stream stream, Endianness endianness)
        {
            Value = stream.ReadVector3(endianness);
            Time = stream.ReadDouble(endianness);
        }
    }
}