namespace HexaEngine.UI.Graphics
{
    public unsafe struct UIDrawCommand : IComparable<UIDrawCommand>
    {
        public UIVertex* Vertices;
        public uint* Indices;
        public uint VertexCount;
        public uint IndexCount;
        public uint IndexOffset;
        public uint VertexOffset;
        public int ZIndex;
        public ClipRectangle ClipRect;
        public UICommandType Type;
        public Brush? Brush;
        public nint TextureId0;
        public nint TextureId1;
        public long Id;

        internal static long id;

        public UIDrawCommand(UIVertex* vertices, uint* indices, uint vertexCount, uint indexCount, uint vertexOffset, uint indexOffset, int zIndex, ClipRectangle clipRect, UICommandType type, Brush? brush, nint textureId0 = 0, nint textureId1 = 0)
        {
            Vertices = vertices;
            Indices = indices;
            VertexCount = vertexCount;
            IndexCount = indexCount;
            VertexOffset = vertexOffset;
            IndexOffset = indexOffset;
            ZIndex = zIndex;
            ClipRect = clipRect;
            Type = type;
            Brush = brush;
            TextureId0 = textureId0;
            TextureId1 = textureId1;
            Id = Interlocked.Increment(ref id);
        }

        public int CompareTo(UIDrawCommand other)
        {
            return ZIndex.CompareTo(other.ZIndex);
        }

        public override string ToString()
        {
            return $"{Id}";
        }
    }
}