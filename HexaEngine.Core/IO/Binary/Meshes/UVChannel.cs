namespace HexaEngine.Core.IO.Binary.Meshes
{
    using System.Numerics;

    public struct UVChannel
    {
        public UVChannel(UVType type, uint vertexCount)
        {
            this.type = type;
            switch (type)
            {
                case UVType.UV2D:
                    uv2D = new Vector2[vertexCount];
                    break;

                case UVType.UV3D:
                    uv3D = new Vector3[vertexCount];
                    break;

                case UVType.UV4D:
                    uv4D = new Vector4[vertexCount];
                    break;
            }
        }

        private UVType type;

        private Vector2[]? uv2D;

        private Vector3[]? uv3D;

        private Vector4[]? uv4D;

        public readonly UVType Type => type;

        public void SetUVs(Vector2[] uvs)
        {
            type = UVType.UV2D;
            uv2D = uvs;
        }

        public void SetUVs(Vector3[] uvs)
        {
            type = UVType.UV3D;
            uv3D = uvs;
        }

        public void SetUVs(Vector4[] uvs)
        {
            type = UVType.UV4D;
            uv4D = uvs;
        }

        public readonly Vector2[] GetUV2D() => type == UVType.UV2D ? uv2D! : throw new InvalidOperationException("UV2D is not active.");

        public readonly Vector3[] GetUV3D() => type == UVType.UV3D ? uv3D! : throw new InvalidOperationException("UV3D is not active.");

        public readonly Vector4[] GetUV4D() => type == UVType.UV4D ? uv4D! : throw new InvalidOperationException("UV4D is not active.");

        public readonly object GetUVs()
        {
            return type switch
            {
                UVType.UV2D => uv2D!,
                UVType.UV3D => uv3D!,
                UVType.UV4D => uv4D!,
                _ => throw new InvalidOperationException()
            };
        }
    }
}