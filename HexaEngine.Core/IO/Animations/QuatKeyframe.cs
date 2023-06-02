namespace HexaEngine.Core.IO.Animations
{
    using HexaEngine.Mathematics;
    using System;
    using System.Numerics;

    public struct QuatKeyframe
    {
        public Quaternion Value;
        public double Time;

        public void Write(Stream stream, Endianness endianness)
        {
            stream.WriteQuaternion(Value, endianness);
            stream.WriteDouble(Time, endianness);
        }

        public void Read(Stream stream, Endianness endianness)
        {
            Value = stream.ReadQuaternion(endianness);
            Time = stream.ReadDouble(endianness);
        }

        public static QuatKeyframe ReadFrom(Stream stream, Endianness endianness)
        {
            QuatKeyframe keyframe = default;
            keyframe.Read(stream, endianness);
            return keyframe;
        }
    }
}