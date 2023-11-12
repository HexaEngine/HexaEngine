namespace HexaEngine.Core.IO.Materials
{
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Mathematics;
    using System.Buffers.Binary;
    using System.Numerics;
    using System.Text;

    /// <summary>
    /// Structure representing a material property.
    /// </summary>
    public unsafe struct MaterialProperty
    {
        /// <summary>
        /// The name of the material property.
        /// </summary>
        public string Name;

        /// <summary>
        /// The type of the material property.
        /// </summary>
        public MaterialPropertyType Type;

        /// <summary>
        /// The value type of the material property.
        /// </summary>
        public MaterialValueType ValueType;

        /// <summary>
        /// The endianness of the material property data.
        /// </summary>
        public Endianness Endianness;

        /// <summary>
        /// The length of the material property data.
        /// </summary>
        public int Length;

        /// <summary>
        /// The raw data of the material property.
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialProperty"/> struct.
        /// </summary>
        /// <param name="name">The name of the material property.</param>
        /// <param name="type">The type of the material property.</param>
        /// <param name="valueType">The value type of the material property.</param>
        /// <param name="endianness">The endianness of the material property data.</param>
        /// <param name="length">The length of the material property data.</param>
        /// <param name="data">The raw data of the material property.</param>
        public MaterialProperty(string name, MaterialPropertyType type, MaterialValueType valueType, Endianness endianness, int length, byte[] data)
        {
            Name = name;
            Type = type;
            Data = data;
            Length = length;
            ValueType = valueType;
            Endianness = endianness;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialProperty"/> struct with a Vector4 value.
        /// </summary>
        /// <param name="name">The name of the material property.</param>
        /// <param name="type">The type of the material property.</param>
        /// <param name="endianness">The endianness of the material property data.</param>
        /// <param name="value">The Vector4 value to set for the material property.</param>
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

        /// <summary>
        /// Gets the byte count based on the specified <see cref="MaterialValueType"/>.
        /// </summary>
        /// <param name="type">The <see cref="MaterialValueType"/> to get the byte count for.</param>
        /// <returns>The byte count for the specified value type.</returns>
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

        /// <summary>
        /// Reads the material property from the specified stream using the provided encoding and endianness.
        /// </summary>
        /// <param name="src">The stream to read from.</param>
        /// <param name="encoding">The encoding used for reading strings.</param>
        /// <param name="endianness">The endianness of the data in the stream.</param>
        public void ReadFrom(Stream src, Encoding encoding, Endianness endianness)
        {
            Name = src.ReadString(encoding, endianness) ?? string.Empty;
            Type = (MaterialPropertyType)src.ReadInt32(endianness);
            ValueType = (MaterialValueType)src.ReadInt32(endianness);
            Length = src.ReadInt32(endianness);
            Data = src.ReadBytes(Length);
            Endianness = endianness;
        }

        /// <summary>
        /// Reads a <see cref="MaterialProperty"/> instance from the specified stream using the specified encoding and endianness.
        /// </summary>
        /// <param name="src">The stream to read the material property from.</param>
        /// <param name="encoding">The encoding used for reading string data.</param>
        /// <param name="endianness">The endianness of the binary data in the stream.</param>
        /// <returns>A new instance of <see cref="MaterialProperty"/> read from the stream.</returns>
        public static MaterialProperty Read(Stream src, Encoding encoding, Endianness endianness)
        {
            MaterialProperty materialProperty = default;
            materialProperty.ReadFrom(src, encoding, endianness);
            return materialProperty;
        }

        /// <summary>
        /// Writes the material property to the specified stream using the provided encoding and endianness.
        /// </summary>
        /// <param name="dst">The stream to write to.</param>
        /// <param name="encoding">The encoding used for writing strings.</param>
        /// <param name="endianness">The endianness to use for writing.</param>
        public void Write(Stream dst, Encoding encoding, Endianness endianness)
        {
            dst.WriteString(Name, encoding, endianness);
            dst.WriteInt32((int)Type, endianness);
            dst.WriteInt32((int)ValueType, endianness);
            dst.WriteInt32(Length, endianness);
            dst.Write(Data);
        }

        /// <summary>
        /// Gets the float value of the material property.
        /// </summary>
        /// <returns>The float value of the material property.</returns>
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

        /// <summary>
        /// Sets the float value of the material property.
        /// </summary>
        /// <param name="value">The float value to set for the material property.</param>
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

        /// <summary>
        /// Gets the Vector2 value of the material property.
        /// </summary>
        /// <returns>The Vector2 value of the material property.</returns>
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

        /// <summary>
        /// Sets the Vector2 value of the material property.
        /// </summary>
        /// <param name="value">The Vector2 value to set for the material property.</param>
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

        /// <summary>
        /// Gets the Vector3 value of the material property.
        /// </summary>
        /// <returns>The Vector3 value of the material property.</returns>
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

        /// <summary>
        /// Sets the Vector3 value of the material property.
        /// </summary>
        /// <param name="value">The Vector3 value to set for the material property.</param>
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

        /// <summary>
        /// Gets the Vector4 value of the material property.
        /// </summary>
        /// <returns>The Vector4 value of the material property.</returns>
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

        /// <summary>
        /// Sets the Vector4 value of the material property.
        /// </summary>
        /// <param name="value">The Vector4 value to set for the material property.</param>
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

        /// <summary>
        /// Gets the boolean value of the material property.
        /// </summary>
        /// <returns>The boolean value of the material property.</returns>
        public bool AsBool()
        {
            return Data[0] == 1;
        }

        /// <summary>
        /// Sets the boolean value of the material property.
        /// </summary>
        /// <param name="value">The boolean value to set for the material property.</param>
        public void SetBool(bool value)
        {
            Data[0] = (byte)(value ? 1 : 0);
        }

        /// <summary>
        /// Converts the material property to a <see cref="ShaderMacro"/>.
        /// </summary>
        /// <returns>The <see cref="ShaderMacro"/> representation of the material property.</returns>
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