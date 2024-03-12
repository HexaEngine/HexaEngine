namespace HexaEngine.UI.Graphics
{
    public unsafe struct UIDrawCommand
    {
        public UIVertex* Vertices;
        public uint* Indices;
        public uint VertexCount;
        public uint IndexCount;
        public uint IndexOffset;
        public uint VertexOffset;
        public UICommandType Type;
        public nint TextureId0;
        public nint TextureId1;

        public UIDrawCommand(UIVertex* vertices, uint* indices, uint vertexCount, uint indexCount, uint vertexOffset, uint indexOffset, UICommandType type, nint textureId0 = 0, nint textureId1 = 0)
        {
            Vertices = vertices;
            Indices = indices;
            VertexCount = vertexCount;
            IndexCount = indexCount;
            VertexOffset = vertexOffset;
            IndexOffset = indexOffset;
            Type = type;
            TextureId0 = textureId0;
            TextureId1 = textureId1;
        }
    }
}