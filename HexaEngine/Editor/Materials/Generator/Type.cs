namespace HexaEngine.Editor.Materials.Generator
{
    using HexaEngine.Editor.Materials.Generator.Enums;

    public class Type
    {
        private static readonly Dictionary<string, Type> map = new();
        private readonly string structName;

        static Type()
        {
            // pre-generate map for fast type parsing
            foreach (var val in Enum.GetValues<ScalarType>())
            {
                map.Add(val.ToString().ToLowerInvariant(), new(val.ToString().ToLowerInvariant()));
            }
            foreach (var val in Enum.GetValues<VectorType>())
            {
                map.Add(val.ToString().ToLowerInvariant(), new(val.ToString().ToLowerInvariant()));
            }
            foreach (var val in Enum.GetValues<MatrixType>())
            {
                map.Add(val.ToString().ToLowerInvariant(), new(val.ToString().ToLowerInvariant()));
            }
            foreach (var val in Enum.GetValues<SamplerType>())
            {
                map.Add(val.ToString(), new(val.ToString()));
            }
            foreach (var val in Enum.GetValues<BufferType>())
            {
                map.Add(val.ToString(), new(val.ToString()));
            }
            foreach (var val in Enum.GetValues<TextureType>())
            {
                map.Add(val.ToString(), new(val.ToString()));
            }
            foreach (var val in Enum.GetValues<UavBufferType>())
            {
                map.Add(val.ToString(), new(val.ToString()));
            }
            foreach (var val in Enum.GetValues<UavTextureType>())
            {
                map.Add(val.ToString(), new(val.ToString()));
            }
        }

        public string Name { get => GetTypeName(); }

        public string? Semantic;
        public bool IsScalar;
        public ScalarType ScalarType;
        public bool IsVector;
        public VectorType VectorType;
        public bool IsMatrix;
        public MatrixType MatrixType;
        public bool IsSampler;
        public SamplerType SamplerType;
        public bool IsBuffer;
        public BufferType BufferType;
        public bool IsTexture;
        public TextureType TextureType;
        public bool IsUavBuffer;
        public UavBufferType UavBufferType;
        public bool IsUavTexture;
        public UavTextureType UavTextureType;
        public bool IsConstantBuffer;
        public bool IsStruct;

        public Type(string name)
        {
            structName = name;
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
                return structName;
            }
            throw new();
        }

        public static Type Parse(string type)
        {
            if (map.TryGetValue(type, out var value))
            {
                return value;
            }

            return new(type) { IsStruct = true };
        }
    }
}