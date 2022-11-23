﻿namespace HexaEngine.Plugins2
{
    using HexaEngine.Core.Unsafes;
    using System.Buffers.Binary;
    using System.Numerics;

    public unsafe struct Transform
    {
        public Vector3 Position;
        public Quaternion Orientation;
        public Vector3 Scale;

        public (Vector3, Quaternion, Vector3) POS { get => (Position, Orientation, Scale); set => (Position, Orientation, Scale) = value; }

        public int Write(Span<byte> data)
        {
            int idx = 0;

            BinaryPrimitives.WriteSingleLittleEndian(data[idx..], Position.X);
            idx += 4;
            BinaryPrimitives.WriteSingleLittleEndian(data[idx..], Position.Y);
            idx += 4;
            BinaryPrimitives.WriteSingleLittleEndian(data[idx..], Position.Z);
            idx += 4;

            BinaryPrimitives.WriteSingleLittleEndian(data[idx..], Orientation.X);
            idx += 4;
            BinaryPrimitives.WriteSingleLittleEndian(data[idx..], Orientation.Y);
            idx += 4;
            BinaryPrimitives.WriteSingleLittleEndian(data[idx..], Orientation.Z);
            idx += 4;
            BinaryPrimitives.WriteSingleLittleEndian(data[idx..], Orientation.W);
            idx += 4;

            BinaryPrimitives.WriteSingleLittleEndian(data[idx..], Scale.X);
            idx += 4;
            BinaryPrimitives.WriteSingleLittleEndian(data[idx..], Scale.Y);
            idx += 4;
            BinaryPrimitives.WriteSingleLittleEndian(data[idx..], Scale.Z);
            idx += 4;
            return idx;
        }

        public int Read(ReadOnlySpan<byte> data)
        {
            int idx = 0;

            Position.X = BinaryPrimitives.ReadSingleLittleEndian(data[idx..]);
            idx += 4;
            Position.Y = BinaryPrimitives.ReadSingleLittleEndian(data[idx..]);
            idx += 4;
            Position.Z = BinaryPrimitives.ReadSingleLittleEndian(data[idx..]);
            idx += 4;

            Orientation.X = BinaryPrimitives.ReadSingleLittleEndian(data[idx..]);
            idx += 4;
            Orientation.Y = BinaryPrimitives.ReadSingleLittleEndian(data[idx..]);
            idx += 4;
            Orientation.Z = BinaryPrimitives.ReadSingleLittleEndian(data[idx..]);
            idx += 4;
            Orientation.W = BinaryPrimitives.ReadSingleLittleEndian(data[idx..]);
            idx += 4;

            Scale.X = BinaryPrimitives.ReadSingleLittleEndian(data[idx..]);
            idx += 4;
            Scale.Y = BinaryPrimitives.ReadSingleLittleEndian(data[idx..]);
            idx += 4;
            Scale.Z = BinaryPrimitives.ReadSingleLittleEndian(data[idx..]);
            idx += 4;
            return idx;
        }
    }
}