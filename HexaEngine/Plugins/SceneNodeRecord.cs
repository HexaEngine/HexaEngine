namespace HexaEngine.Plugins
{
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Scenes;

    public unsafe struct SceneNodeRecord : IRecord
    {
        public UnsafeString Name;
        public Transform Transform;
        public UnsafeArray<SceneNodeComponent> Components;
        public UnsafeArray<int> Meshes;

        public SceneNodeRecord(SceneNode node)
        {
            Name = node.Name;
            Transform.POS = node.Transform.PositionRotationScale;
        }

        public void Decode(Span<byte> src, Endianness endianness)
        {
            fixed (SceneNodeRecord* @this = &this)
            {
                UnsafeString.Read(&@this->Name, endianness, src);
            }
        }

        public void Encode(Span<byte> dest, Endianness endianness)
        {
            fixed (SceneNodeRecord* @this = &this)
            {
                UnsafeString.Write(&@this->Name, endianness, dest);
            }
        }

        public int Size()
        {
            return Name.Sizeof() + sizeof(Transform) + Components.Sizeof() + Meshes.Sizeof();
        }
    }
}