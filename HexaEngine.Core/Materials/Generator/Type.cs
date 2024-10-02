namespace HexaEngine.Materials.Generator
{
    using HexaEngine.Materials.Generator.Enums;
    using HexaEngine.Materials.Generator.Structs;

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

    public struct SType
    {
        private static readonly Dictionary<string, SType> map = new();
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

        public static SType Void => new() { _flags = TypeFlags.Void };

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

        public string GetTypeName()
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

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
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
    }
}