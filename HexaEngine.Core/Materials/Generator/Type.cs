namespace HexaEngine.Materials.Generator
{
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Generator.Structs;
    using HexaEngine.Materials.Pins;

    [Flags]
    public enum TypeFlags
    {
        None = 0,
        Scalar = 1 << 0,
        Vector = 1 << 1,
        Matrix = 1 << 2,
        Sampler = 1 << 3,
        Buffer = 1 << 4,
        Texture = 1 << 5,
        UavBuffer = 1 << 6,
        UavTexture = 1 << 7,
        ConstantBuffer = 1 << 8,
        Struct = 1 << 9,
        Unknown = 1 << 10,
        Void = 1 << 11,
    }

    public class UnknownSTypeException : Exception
    {
        public UnknownSTypeException()
        {
        }

        public UnknownSTypeException(string? message) : base(message)
        {
        }
    }

    public struct SType
    {
        private static readonly Dictionary<string, SType> map = new(StringComparer.InvariantCultureIgnoreCase);
        private readonly string? structName;

        static SType()
        {
            // pre-generate map for fast type parsing
            foreach (var val in Enum.GetValues<ScalarType>())
            {
                var name = val.ToString().ToLowerInvariant();
                if (name == "unknown")
                {
                    continue;
                }

                map.Add(name, new(val));
            }
            foreach (var val in Enum.GetValues<VectorType>())
            {
                var name = val.ToString().ToLowerInvariant();
                if (name == "unknown")
                {
                    continue;
                }

                map.Add(name, new(val));
            }
            foreach (var val in Enum.GetValues<MatrixType>())
            {
                var name = val.ToString().ToLowerInvariant();
                if (name == "unknown")
                {
                    continue;
                }

                map.Add(name, new(val));
            }
            foreach (var val in Enum.GetValues<SamplerType>())
            {
                var name = val.ToString();
                if (name == "Unknown")
                {
                    continue;
                }

                map.Add(name, new(val));
            }
            foreach (var val in Enum.GetValues<BufferType>())
            {
                var name = val.ToString();
                if (name == "Unknown")
                {
                    continue;
                }

                map.Add(name, new(val));
            }
            foreach (var val in Enum.GetValues<TextureType>())
            {
                var name = val.ToString();
                if (name == "Unknown")
                {
                    continue;
                }

                map.Add(name, new(val));
            }
            foreach (var val in Enum.GetValues<UavBufferType>())
            {
                var name = val.ToString();
                if (name == "Unknown")
                {
                    continue;
                }

                map.Add(name, new(val));
            }
            foreach (var val in Enum.GetValues<UavTextureType>())
            {
                var name = val.ToString();
                if (name == "Unknown")
                {
                    continue;
                }

                map.Add(name, new(val));
            }
        }

        public string Name { get => GetTypeName(); }

        public string? Semantic;
        public ScalarType ScalarType;
        public VectorType VectorType;
        public MatrixType MatrixType;
        public SamplerType SamplerType;
        public BufferType BufferType;
        public TextureType TextureType;
        public UavBufferType UavBufferType;
        public UavTextureType UavTextureType;
        private TypeFlags _flags;

        public bool IsScalar
        {
            readonly get => (_flags & TypeFlags.Scalar) != 0;
            set
            {
                if (value)
                    _flags |= TypeFlags.Scalar;
                else
                    _flags &= ~TypeFlags.Scalar;
            }
        }

        public bool IsVector
        {
            readonly get => (_flags & TypeFlags.Vector) != 0;
            set
            {
                if (value)
                    _flags |= TypeFlags.Vector;
                else
                    _flags &= ~TypeFlags.Vector;
            }
        }

        public bool IsMatrix
        {
            readonly get => (_flags & TypeFlags.Matrix) != 0;
            set
            {
                if (value)
                    _flags |= TypeFlags.Matrix;
                else
                    _flags &= ~TypeFlags.Matrix;
            }
        }

        public bool IsSampler
        {
            readonly get => (_flags & TypeFlags.Sampler) != 0;
            set
            {
                if (value)
                    _flags |= TypeFlags.Sampler;
                else
                    _flags &= ~TypeFlags.Sampler;
            }
        }

        public bool IsBuffer
        {
            readonly get => (_flags & TypeFlags.Buffer) != 0;
            set
            {
                if (value)
                    _flags |= TypeFlags.Buffer;
                else
                    _flags &= ~TypeFlags.Buffer;
            }
        }

        public bool IsTexture
        {
            readonly get => (_flags & TypeFlags.Texture) != 0;
            set
            {
                if (value)
                    _flags |= TypeFlags.Texture;
                else
                    _flags &= ~TypeFlags.Texture;
            }
        }

        public bool IsUavBuffer
        {
            readonly get => (_flags & TypeFlags.UavBuffer) != 0;
            set
            {
                if (value)
                    _flags |= TypeFlags.UavBuffer;
                else
                    _flags &= ~TypeFlags.UavBuffer;
            }
        }

        public bool IsUavTexture
        {
            readonly get => (_flags & TypeFlags.UavTexture) != 0;
            set
            {
                if (value)
                    _flags |= TypeFlags.UavTexture;
                else
                    _flags &= ~TypeFlags.UavTexture;
            }
        }

        public bool IsConstantBuffer
        {
            readonly get => (_flags & TypeFlags.ConstantBuffer) != 0;
            set
            {
                if (value)
                    _flags |= TypeFlags.ConstantBuffer;
                else
                    _flags &= ~TypeFlags.ConstantBuffer;
            }
        }

        public bool IsStruct
        {
            readonly get => (_flags & TypeFlags.Struct) != 0;
            set
            {
                if (value)
                    _flags |= TypeFlags.Struct;
                else
                    _flags &= ~TypeFlags.Struct;
            }
        }

        public bool IsUnknown
        {
            readonly get => (_flags & TypeFlags.Unknown) != 0;
            set
            {
                if (value)
                    _flags |= TypeFlags.Unknown;
                else
                    _flags &= ~TypeFlags.Unknown;
            }
        }

        public bool IsVoid
        {
            readonly get => (_flags & TypeFlags.Void) != 0;
            set
            {
                if (value)
                    _flags |= TypeFlags.Void;
                else
                    _flags &= ~TypeFlags.Void;
            }
        }

        public TypeFlags Flags
        {
            readonly get => _flags;
            set => _flags = value;
        }

        public static SType Void => new() { _flags = TypeFlags.Void };

        public static SType Unknown => new() { _flags = TypeFlags.Unknown };

        public List<Operator> Operators = [];

        public SType(string name)
        {
            IsStruct = true;
            structName = name;
        }

        public SType(ScalarType scalar, string semantic)
        {
            IsScalar = true;
            ScalarType = scalar;
            Semantic = semantic;
        }

        public SType(VectorType vector, string semantic)
        {
            IsVector = true;
            VectorType = vector;
            Semantic = semantic;
        }

        public SType(MatrixType matrix, string semantic)
        {
            IsMatrix = true;
            MatrixType = matrix;
            Semantic = semantic;
        }

        public SType(ScalarType scalar)
        {
            IsScalar = true;
            ScalarType = scalar;
        }

        public SType(VectorType vector)
        {
            IsVector = true;
            VectorType = vector;
        }

        public SType(MatrixType matrix)
        {
            IsMatrix = true;
            MatrixType = matrix;
        }

        public SType(SamplerType sampler)
        {
            IsSampler = true;
            SamplerType = sampler;
        }

        public SType(BufferType buffer)
        {
            IsBuffer = true;
            BufferType = buffer;
        }

        public SType(TextureType texture)
        {
            IsTexture = true;
            TextureType = texture;
        }

        public SType(UavBufferType uavBuffer)
        {
            IsUavBuffer = true;
            UavBufferType = uavBuffer;
        }

        public SType(UavTextureType uavTexture)
        {
            IsUavTexture = true;
            UavTextureType = uavTexture;
        }

        public readonly string GetTypeName()
        {
            if (IsScalar)
            {
                return ScalarType.ToString().ToLowerInvariant();
            }
            if (IsVector)
            {
                return VectorType.ToString().ToLowerInvariant();
            }
            if (IsMatrix)
            {
                return MatrixType.ToString().ToLowerInvariant();
            }
            if (IsSampler)
            {
                return SamplerType.ToString();
            }
            if (IsBuffer)
            {
                return BufferType.ToString();
            }
            if (IsTexture)
            {
                return TextureType.ToString();
            }
            if (IsUavBuffer)
            {
                return UavBufferType.ToString();
            }
            if (IsUavTexture)
            {
                return UavTextureType.ToString();
            }
            if (IsConstantBuffer)
            {
                throw new();
            }
            if (IsStruct)
            {
#pragma warning disable CS8603 // Possible null reference return.
                return structName;
#pragma warning restore CS8603 // Possible null reference return.
            }
            if (IsUnknown)
            {
                throw new UnknownSTypeException();
            }
            throw new();
        }

        public static SType Parse(string type)
        {
            if (map.TryGetValue(type, out var value))
            {
                return value;
            }

            return new(type) { IsStruct = true };
        }

        public override bool Equals(object? obj)
        {
            if (obj is SType type)
            {
                return Equals(type);
            }
            return false;
        }

        public bool Equals(SType other)
        {
            if (other.IsScalar == IsScalar)
            {
                return other.ScalarType == ScalarType;
            }
            else if (other.IsVector == IsVector)
            {
                return other.VectorType == VectorType;
            }
            else if (other.IsMatrix == IsMatrix)
            {
                return other.VectorType == VectorType;
            }
            else if (other.IsSampler == IsSampler)
            {
                return other.SamplerType == SamplerType;
            }
            else if (other.IsBuffer == IsBuffer)
            {
                return other.BufferType == BufferType;
            }
            else if (other.IsTexture == IsTexture)
            {
                return other.TextureType == TextureType;
            }
            else if (other.IsUavBuffer == IsUavBuffer)
            {
                return other.UavBufferType == UavBufferType;
            }
            else if (other.IsUavTexture == IsUavTexture)
            {
                return other.UavTextureType == UavTextureType;
            }
            else if (other.IsConstantBuffer == IsConstantBuffer)
            {
                return true;
            }
            else if (other.IsStruct == IsStruct)
            {
                return other.structName == structName;
            }
            else
            {
                return false;
            }
        }

        public readonly override int GetHashCode()
        {
            HashCode hash = new();
            hash.Add(structName);
            hash.Add(Semantic);
            hash.Add(IsScalar);
            hash.Add(ScalarType);
            hash.Add(IsVector);
            hash.Add(VectorType);
            hash.Add(IsMatrix);
            hash.Add(MatrixType);
            hash.Add(IsSampler);
            hash.Add(SamplerType);
            hash.Add(IsBuffer);
            hash.Add(BufferType);
            hash.Add(IsTexture);
            hash.Add(TextureType);
            hash.Add(IsUavBuffer);
            hash.Add(UavBufferType);
            hash.Add(IsUavTexture);
            hash.Add(UavTextureType);
            hash.Add(IsConstantBuffer);
            hash.Add(IsStruct);
            return hash.ToHashCode();
        }

        public readonly int SizeOf()
        {
            var scalar = ToScalar();
            var components = ComponentCount();
            var scalarSize = scalar.ScalarType switch
            {
                ScalarType.Bool => 4,
                ScalarType.Int => 4,
                ScalarType.UInt => 4,
                ScalarType.Half => 2,
                ScalarType.Float => 4,
                ScalarType.Double => 8,
                _ => throw new NotSupportedException(),
            };
            return scalarSize * components;
        }

        public readonly SType ToScalar()
        {
            if (IsScalar)
            {
                return this;
            }
            else if (IsVector)
            {
                return VectorType switch
                {
                    VectorType.Bool2 => new(ScalarType.Bool),
                    VectorType.Bool3 => new(ScalarType.Bool),
                    VectorType.Bool4 => new(ScalarType.Bool),
                    VectorType.Int2 => new(ScalarType.Int),
                    VectorType.Int3 => new(ScalarType.Int),
                    VectorType.Int4 => new(ScalarType.Int),
                    VectorType.UInt2 => new(ScalarType.UInt),
                    VectorType.UInt3 => new(ScalarType.UInt),
                    VectorType.UInt4 => new(ScalarType.UInt),
                    VectorType.Half2 => new(ScalarType.Half),
                    VectorType.Half3 => new(ScalarType.Half),
                    VectorType.Half4 => new(ScalarType.Half),
                    VectorType.Float2 => new(ScalarType.Float),
                    VectorType.Float3 => new(ScalarType.Float),
                    VectorType.Float4 => new(ScalarType.Float),
                    VectorType.Double2 => new(ScalarType.Double),
                    VectorType.Double3 => new(ScalarType.Double),
                    VectorType.Double4 => new(ScalarType.Double),
                    _ => throw new NotSupportedException()
                };
            }
            else if (IsMatrix)
            {
                return MatrixType switch
                {
                    MatrixType.Bool1x1 => new(ScalarType.Bool),
                    MatrixType.Bool1x2 => new(ScalarType.Bool),
                    MatrixType.Bool1x3 => new(ScalarType.Bool),
                    MatrixType.Bool1x4 => new(ScalarType.Bool),
                    MatrixType.Bool2x1 => new(ScalarType.Bool),
                    MatrixType.Bool2x2 => new(ScalarType.Bool),
                    MatrixType.Bool2x3 => new(ScalarType.Bool),
                    MatrixType.Bool2x4 => new(ScalarType.Bool),
                    MatrixType.Bool3x1 => new(ScalarType.Bool),
                    MatrixType.Bool3x2 => new(ScalarType.Bool),
                    MatrixType.Bool3x3 => new(ScalarType.Bool),
                    MatrixType.Bool3x4 => new(ScalarType.Bool),
                    MatrixType.Bool4x1 => new(ScalarType.Bool),
                    MatrixType.Bool4x2 => new(ScalarType.Bool),
                    MatrixType.Bool4x3 => new(ScalarType.Bool),
                    MatrixType.Bool4x4 => new(ScalarType.Bool),
                    MatrixType.Int1x1 => new(ScalarType.Int),
                    MatrixType.Int1x2 => new(ScalarType.Int),
                    MatrixType.Int1x3 => new(ScalarType.Int),
                    MatrixType.Int1x4 => new(ScalarType.Int),
                    MatrixType.Int2x1 => new(ScalarType.Int),
                    MatrixType.Int2x2 => new(ScalarType.Int),
                    MatrixType.Int2x3 => new(ScalarType.Int),
                    MatrixType.Int2x4 => new(ScalarType.Int),
                    MatrixType.Int3x1 => new(ScalarType.Int),
                    MatrixType.Int3x2 => new(ScalarType.Int),
                    MatrixType.Int3x3 => new(ScalarType.Int),
                    MatrixType.Int3x4 => new(ScalarType.Int),
                    MatrixType.Int4x1 => new(ScalarType.Int),
                    MatrixType.Int4x2 => new(ScalarType.Int),
                    MatrixType.Int4x3 => new(ScalarType.Int),
                    MatrixType.Int4x4 => new(ScalarType.Int),
                    MatrixType.UInt1x1 => new(ScalarType.UInt),
                    MatrixType.UInt1x2 => new(ScalarType.UInt),
                    MatrixType.UInt1x3 => new(ScalarType.UInt),
                    MatrixType.UInt1x4 => new(ScalarType.UInt),
                    MatrixType.UInt2x1 => new(ScalarType.UInt),
                    MatrixType.UInt2x2 => new(ScalarType.UInt),
                    MatrixType.UInt2x3 => new(ScalarType.UInt),
                    MatrixType.UInt2x4 => new(ScalarType.UInt),
                    MatrixType.UInt3x1 => new(ScalarType.UInt),
                    MatrixType.UInt3x2 => new(ScalarType.UInt),
                    MatrixType.UInt3x3 => new(ScalarType.UInt),
                    MatrixType.UInt3x4 => new(ScalarType.UInt),
                    MatrixType.UInt4x1 => new(ScalarType.UInt),
                    MatrixType.UInt4x2 => new(ScalarType.UInt),
                    MatrixType.UInt4x3 => new(ScalarType.UInt),
                    MatrixType.UInt4x4 => new(ScalarType.UInt),
                    MatrixType.Half1x1 => new(ScalarType.Half),
                    MatrixType.Half1x2 => new(ScalarType.Half),
                    MatrixType.Half1x3 => new(ScalarType.Half),
                    MatrixType.Half1x4 => new(ScalarType.Half),
                    MatrixType.Half2x1 => new(ScalarType.Half),
                    MatrixType.Half2x2 => new(ScalarType.Half),
                    MatrixType.Half2x3 => new(ScalarType.Half),
                    MatrixType.Half2x4 => new(ScalarType.Half),
                    MatrixType.Half3x1 => new(ScalarType.Half),
                    MatrixType.Half3x2 => new(ScalarType.Half),
                    MatrixType.Half3x3 => new(ScalarType.Half),
                    MatrixType.Half3x4 => new(ScalarType.Half),
                    MatrixType.Half4x1 => new(ScalarType.Half),
                    MatrixType.Half4x2 => new(ScalarType.Half),
                    MatrixType.Half4x3 => new(ScalarType.Half),
                    MatrixType.Half4x4 => new(ScalarType.Half),
                    MatrixType.Float1x1 => new(ScalarType.Float),
                    MatrixType.Float1x2 => new(ScalarType.Float),
                    MatrixType.Float1x3 => new(ScalarType.Float),
                    MatrixType.Float1x4 => new(ScalarType.Float),
                    MatrixType.Float2x1 => new(ScalarType.Float),
                    MatrixType.Float2x2 => new(ScalarType.Float),
                    MatrixType.Float2x3 => new(ScalarType.Float),
                    MatrixType.Float2x4 => new(ScalarType.Float),
                    MatrixType.Float3x1 => new(ScalarType.Float),
                    MatrixType.Float3x2 => new(ScalarType.Float),
                    MatrixType.Float3x3 => new(ScalarType.Float),
                    MatrixType.Float3x4 => new(ScalarType.Float),
                    MatrixType.Float4x1 => new(ScalarType.Float),
                    MatrixType.Float4x2 => new(ScalarType.Float),
                    MatrixType.Float4x3 => new(ScalarType.Float),
                    MatrixType.Float4x4 => new(ScalarType.Float),
                    MatrixType.Double1x1 => new(ScalarType.Double),
                    MatrixType.Double1x2 => new(ScalarType.Double),
                    MatrixType.Double1x3 => new(ScalarType.Double),
                    MatrixType.Double1x4 => new(ScalarType.Double),
                    MatrixType.Double2x1 => new(ScalarType.Double),
                    MatrixType.Double2x2 => new(ScalarType.Double),
                    MatrixType.Double2x3 => new(ScalarType.Double),
                    MatrixType.Double2x4 => new(ScalarType.Double),
                    MatrixType.Double3x1 => new(ScalarType.Double),
                    MatrixType.Double3x2 => new(ScalarType.Double),
                    MatrixType.Double3x3 => new(ScalarType.Double),
                    MatrixType.Double3x4 => new(ScalarType.Double),
                    MatrixType.Double4x1 => new(ScalarType.Double),
                    MatrixType.Double4x2 => new(ScalarType.Double),
                    MatrixType.Double4x3 => new(ScalarType.Double),
                    MatrixType.Double4x4 => new(ScalarType.Double),
                    _ => throw new NotSupportedException()
                };
            }

            throw new NotSupportedException();
        }

        public readonly int ComponentCount()
        {
            if (IsScalar)
            {
                return 1;
            }
            else if (IsVector)
            {
                return VectorType switch
                {
                    VectorType.Bool2 => 2,
                    VectorType.Bool3 => 3,
                    VectorType.Bool4 => 4,
                    VectorType.Int2 => 2,
                    VectorType.Int3 => 3,
                    VectorType.Int4 => 4,
                    VectorType.UInt2 => 2,
                    VectorType.UInt3 => 3,
                    VectorType.UInt4 => 4,
                    VectorType.Half2 => 2,
                    VectorType.Half3 => 3,
                    VectorType.Half4 => 4,
                    VectorType.Float2 => 2,
                    VectorType.Float3 => 3,
                    VectorType.Float4 => 4,
                    VectorType.Double2 => 2,
                    VectorType.Double3 => 3,
                    VectorType.Double4 => 4,
                    _ => throw new NotSupportedException()
                };
            }
            else if (IsMatrix)
            {
                return MatrixType switch
                {
                    MatrixType.Bool1x1 => 1,
                    MatrixType.Bool1x2 => 2,
                    MatrixType.Bool1x3 => 3,
                    MatrixType.Bool1x4 => 4,
                    MatrixType.Bool2x1 => 2,
                    MatrixType.Bool2x2 => 4,
                    MatrixType.Bool2x3 => 6,
                    MatrixType.Bool2x4 => 8,
                    MatrixType.Bool3x1 => 3,
                    MatrixType.Bool3x2 => 6,
                    MatrixType.Bool3x3 => 9,
                    MatrixType.Bool3x4 => 12,
                    MatrixType.Bool4x1 => 4,
                    MatrixType.Bool4x2 => 8,
                    MatrixType.Bool4x3 => 12,
                    MatrixType.Bool4x4 => 16,
                    MatrixType.Int1x1 => 1,
                    MatrixType.Int1x2 => 2,
                    MatrixType.Int1x3 => 3,
                    MatrixType.Int1x4 => 4,
                    MatrixType.Int2x1 => 2,
                    MatrixType.Int2x2 => 4,
                    MatrixType.Int2x3 => 6,
                    MatrixType.Int2x4 => 8,
                    MatrixType.Int3x1 => 3,
                    MatrixType.Int3x2 => 6,
                    MatrixType.Int3x3 => 9,
                    MatrixType.Int3x4 => 12,
                    MatrixType.Int4x1 => 4,
                    MatrixType.Int4x2 => 8,
                    MatrixType.Int4x3 => 12,
                    MatrixType.Int4x4 => 16,
                    MatrixType.UInt1x1 => 1,
                    MatrixType.UInt1x2 => 2,
                    MatrixType.UInt1x3 => 3,
                    MatrixType.UInt1x4 => 4,
                    MatrixType.UInt2x1 => 2,
                    MatrixType.UInt2x2 => 4,
                    MatrixType.UInt2x3 => 6,
                    MatrixType.UInt2x4 => 8,
                    MatrixType.UInt3x1 => 3,
                    MatrixType.UInt3x2 => 6,
                    MatrixType.UInt3x3 => 9,
                    MatrixType.UInt3x4 => 12,
                    MatrixType.UInt4x1 => 4,
                    MatrixType.UInt4x2 => 8,
                    MatrixType.UInt4x3 => 12,
                    MatrixType.UInt4x4 => 16,
                    MatrixType.Half1x1 => 1,
                    MatrixType.Half1x2 => 2,
                    MatrixType.Half1x3 => 3,
                    MatrixType.Half1x4 => 4,
                    MatrixType.Half2x1 => 2,
                    MatrixType.Half2x2 => 4,
                    MatrixType.Half2x3 => 6,
                    MatrixType.Half2x4 => 8,
                    MatrixType.Half3x1 => 3,
                    MatrixType.Half3x2 => 6,
                    MatrixType.Half3x3 => 9,
                    MatrixType.Half3x4 => 12,
                    MatrixType.Half4x1 => 4,
                    MatrixType.Half4x2 => 8,
                    MatrixType.Half4x3 => 12,
                    MatrixType.Half4x4 => 16,
                    MatrixType.Float1x1 => 1,
                    MatrixType.Float1x2 => 2,
                    MatrixType.Float1x3 => 3,
                    MatrixType.Float1x4 => 4,
                    MatrixType.Float2x1 => 2,
                    MatrixType.Float2x2 => 4,
                    MatrixType.Float2x3 => 6,
                    MatrixType.Float2x4 => 8,
                    MatrixType.Float3x1 => 3,
                    MatrixType.Float3x2 => 6,
                    MatrixType.Float3x3 => 9,
                    MatrixType.Float3x4 => 12,
                    MatrixType.Float4x1 => 4,
                    MatrixType.Float4x2 => 8,
                    MatrixType.Float4x3 => 12,
                    MatrixType.Float4x4 => 16,
                    MatrixType.Double1x1 => 1,
                    MatrixType.Double1x2 => 2,
                    MatrixType.Double1x3 => 3,
                    MatrixType.Double1x4 => 4,
                    MatrixType.Double2x1 => 2,
                    MatrixType.Double2x2 => 4,
                    MatrixType.Double2x3 => 6,
                    MatrixType.Double2x4 => 8,
                    MatrixType.Double3x1 => 3,
                    MatrixType.Double3x2 => 6,
                    MatrixType.Double3x3 => 9,
                    MatrixType.Double3x4 => 12,
                    MatrixType.Double4x1 => 4,
                    MatrixType.Double4x2 => 8,
                    MatrixType.Double4x3 => 12,
                    MatrixType.Double4x4 => 16,
                    _ => throw new NotSupportedException()
                };
            }

            throw new NotSupportedException();
        }

        public readonly bool IsAny(TypeFlags flags)
        {
            return (_flags & flags) != 0;
        }

        public static bool operator ==(SType? left, SType? right)
        {
            if (left is null && right is null)
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(SType? left, SType? right)
        {
            return !(left == right);
        }

        public override readonly string ToString()
        {
            if (IsScalar)
            {
                return ScalarType.ToString().ToLowerInvariant();
            }
            else if (IsVector)
            {
                return VectorType.ToString().ToLowerInvariant();
            }
            else if (IsMatrix)
            {
                return MatrixType.ToString().ToLowerInvariant();
            }
            else if (IsSampler)
            {
                return SamplerType.ToString().ToLowerInvariant();
            }
            else if (IsBuffer)
            {
                return BufferType.ToString().ToLowerInvariant();
            }
            else if (IsTexture)
            {
                return TextureType.ToString().ToLowerInvariant();
            }
            else if (IsUavBuffer)
            {
                return UavBufferType.ToString().ToLowerInvariant();
            }
            else if (IsUavTexture)
            {
                return UavTextureType.ToString().ToLowerInvariant();
            }
            else if (IsConstantBuffer)
            {
                return "cbuffer";
            }
            else if (IsStruct)
            {
                return structName!;
            }
            else if (IsVoid)
            {
                return "void";
            }

            return "unknown";
        }
    }
}