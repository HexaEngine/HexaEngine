namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Buffers.Binary;
    using System.Numerics;
    using System.Text;

    public unsafe struct MaterialProperty
    {
        public string Name;
        public MaterialPropertyType Type;
        public MaterialValueType ValueType;
        public Endianness Endianness;
        public int Length;
        public byte[] Data;

        public MaterialProperty(string name, MaterialPropertyType type, MaterialValueType valueType, Endianness endianness, int length, byte[] data)
        {
            Name = name;
            Type = type;
            Data = data;
            Length = length;
            ValueType = valueType;
            Endianness = endianness;
        }

        public MaterialProperty(string name, MaterialPropertyType type, Endianness endianness, Vector4 value)
        {
            Name = name;
            Type = type;
            Length = sizeof(Vector4);
            Data = new byte[Length];
            ValueType = MaterialValueType.Float4;
            Endianness = endianness;
            SetFloat4(value);
        }

        public static int GetByteCount(MaterialValueType type)
        {
            return type switch
            {
                MaterialValueType.Float => 4,
                MaterialValueType.Float2 => 8,
                MaterialValueType.Float3 => 12,
                MaterialValueType.Float4 => 16,
                MaterialValueType.Bool => 1,
                MaterialValueType.UInt8 => 1,
                MaterialValueType.UInt16 => 2,
                MaterialValueType.UInt32 => 4,
                MaterialValueType.UInt64 => 8,
                MaterialValueType.Int8 => 1,
                MaterialValueType.Int16 => 2,
                MaterialValueType.Int32 => 4,
                MaterialValueType.Int64 => 8,
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }

        public void Read(Stream src, Encoding encoding, Endianness endianness)
        {
            Name = src.ReadString(encoding, endianness) ?? string.Empty;
            Type = (MaterialPropertyType)src.ReadInt(endianness);
            ValueType = (MaterialValueType)src.ReadInt(endianness);
            Length = src.ReadInt(endianness);
            Data = src.ReadBytes(Length);
            Endianness = endianness;
        }

        public void Write(Stream dst, Encoding encoding, Endianness endianness)
        {
            dst.WriteString(Name, encoding, endianness);
            dst.WriteInt((int)Type, endianness);
            dst.WriteInt((int)ValueType, endianness);
            dst.WriteInt(Length, endianness);
            dst.Write(Data);
        }

        public float AsFloat()
        {
            if (Endianness == Endianness.LittleEndian)
            {
                return BinaryPrimitives.ReadSingleLittleEndian(Data);
            }
            else
            {
                return BinaryPrimitives.ReadSingleBigEndian(Data);
            }
        }

        public void SetFloat(float value)
        {
            if (Endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(Data, value);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(Data, value);
            }
        }

        public Vector2 AsFloat2()
        {
            if (Endianness == Endianness.LittleEndian)
            {
                var x = BinaryPrimitives.ReadSingleLittleEndian(Data);
                var y = BinaryPrimitives.ReadSingleLittleEndian(Data.AsSpan(4, 4));
                return new(x, y);
            }
            else
            {
                var x = BinaryPrimitives.ReadSingleBigEndian(Data);
                var y = BinaryPrimitives.ReadSingleBigEndian(Data.AsSpan(4, 4));
                return new(x, y);
            }
        }

        public void SetFloat2(Vector2 value)
        {
            if (Endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(Data, value.X);
                BinaryPrimitives.WriteSingleLittleEndian(Data.AsSpan(4, 4), value.Y);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(Data, value.X);
                BinaryPrimitives.WriteSingleBigEndian(Data.AsSpan(4, 4), value.Y);
            }
        }

        public Vector3 AsFloat3()
        {
            if (Endianness == Endianness.LittleEndian)
            {
                var x = BinaryPrimitives.ReadSingleLittleEndian(Data);
                var y = BinaryPrimitives.ReadSingleLittleEndian(Data.AsSpan(4, 4));
                var z = BinaryPrimitives.ReadSingleLittleEndian(Data.AsSpan(8, 4));
                return new(x, y, z);
            }
            else
            {
                var x = BinaryPrimitives.ReadSingleBigEndian(Data);
                var y = BinaryPrimitives.ReadSingleBigEndian(Data.AsSpan(4, 4));
                var z = BinaryPrimitives.ReadSingleBigEndian(Data.AsSpan(8, 4));
                return new(x, y, z);
            }
        }

        public void SetFloat3(Vector3 value)
        {
            if (Endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(Data, value.X);
                BinaryPrimitives.WriteSingleLittleEndian(Data.AsSpan(4, 4), value.Y);
                BinaryPrimitives.WriteSingleLittleEndian(Data.AsSpan(8, 4), value.Z);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(Data, value.X);
                BinaryPrimitives.WriteSingleBigEndian(Data.AsSpan(4, 4), value.Y);
                BinaryPrimitives.WriteSingleBigEndian(Data.AsSpan(8, 4), value.Z);
            }
        }

        public Vector4 AsFloat4()
        {
            if (Endianness == Endianness.LittleEndian)
            {
                var x = BinaryPrimitives.ReadSingleLittleEndian(Data);
                var y = BinaryPrimitives.ReadSingleLittleEndian(Data.AsSpan(4, 4));
                var z = BinaryPrimitives.ReadSingleLittleEndian(Data.AsSpan(8, 4));
                var w = BinaryPrimitives.ReadSingleLittleEndian(Data.AsSpan(12, 4));
                return new(x, y, z, w);
            }
            else
            {
                var x = BinaryPrimitives.ReadSingleBigEndian(Data);
                var y = BinaryPrimitives.ReadSingleBigEndian(Data.AsSpan(4, 4));
                var z = BinaryPrimitives.ReadSingleBigEndian(Data.AsSpan(8, 4));
                var w = BinaryPrimitives.ReadSingleBigEndian(Data.AsSpan(12, 4));
                return new(x, y, z, w);
            }
        }

        public void SetFloat4(Vector4 value)
        {
            if (Endianness == Endianness.LittleEndian)
            {
                BinaryPrimitives.WriteSingleLittleEndian(Data, value.X);
                BinaryPrimitives.WriteSingleLittleEndian(Data.AsSpan(4, 4), value.Y);
                BinaryPrimitives.WriteSingleLittleEndian(Data.AsSpan(8, 4), value.Z);
                BinaryPrimitives.WriteSingleLittleEndian(Data.AsSpan(12, 4), value.W);
            }
            else
            {
                BinaryPrimitives.WriteSingleBigEndian(Data, value.X);
                BinaryPrimitives.WriteSingleBigEndian(Data.AsSpan(4, 4), value.Y);
                BinaryPrimitives.WriteSingleBigEndian(Data.AsSpan(8, 4), value.Z);
                BinaryPrimitives.WriteSingleBigEndian(Data.AsSpan(12, 4), value.W);
            }
        }

        public bool AsBool()
        {
            return Data[0] == 1;
        }

        public void SetBool(bool value)
        {
            Data[0] = (byte)(value ? 1 : 0);
        }

        public ShaderMacro AsShaderMacro()
        {
            string definition = string.Empty;
            switch (ValueType)
            {
                case MaterialValueType.Float:
                    definition = AsFloat().ToHLSL();
                    break;

                case MaterialValueType.Float2:
                    definition = AsFloat2().ToHLSL();
                    break;

                case MaterialValueType.Float3:
                    definition = AsFloat3().ToHLSL();
                    break;

                case MaterialValueType.Float4:
                    definition = AsFloat4().ToHLSL();
                    break;

                case MaterialValueType.Bool:
                    definition = AsBool().ToHLSL();
                    break;
            }

            return new ShaderMacro(Type.ToString(), definition);
        }
    }
}