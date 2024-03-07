namespace HexaEngine.UI
{
    public unsafe struct UIDrawCommand
    {
        public RmGuiVertex* Vertices;
        public uint* Indices;
        public uint VertexCount;
        public uint IndexCount;
        public uint IndexOffset;
        public uint VertexOffset;
        public nint TextureId;

        public UIDrawCommand(RmGuiVertex* vertices, uint* indices, uint vertexCount, uint indexCount, uint vertexOffset, uint indexOffset, nint textureId = 0)
        {
            Vertices = vertices;
            Indices = indices;
            VertexCount = vertexCount;
            IndexCount = indexCount;
            VertexOffset = vertexOffset;
            IndexOffset = indexOffset;
            TextureId = textureId;
        }
    }
}