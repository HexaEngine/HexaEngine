namespace HexaEngine.Plugins.Records
{
    using HexaEngine.Core;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Plugins;
    using HexaEngine.Scenes;

    public unsafe struct SceneNodeRecord : IRecord
    {
        public UnsafeString* Name;
        public Transform Transform;
        public UnsafeArray<SceneNodeComponent> Components;
        public UnsafeArray<int> Meshes;

        public SceneNodeRecord(SceneNode node)
        {
            Name = Utilities.AsPointer(new UnsafeString(node.Name));
            Transform.POS = node.Transform.PositionRotationScale;
        }

        public int Decode(Span<byte> src, Endianness endianness)
        {
            fixed (SceneNodeRecord* @this = &this)
            {
                return UnsafeString.Read(&@this->Name, endianness, src);
            }
        }

        public int Encode(Span<byte> dest, Endianness endianness)
        {
            fixed (SceneNodeRecord* @this = &this)
            {
                return UnsafeString.Write(@this->Name, endianness, dest);
            }
        }

        public void Overwrite(void* pRecord)
        {
            SceneNodeRecord* record = (SceneNodeRecord*)pRecord;
            Name = record->Name;
            Transform = record->Transform;
            Components = record->Components;
            Meshes = record->Meshes;

            throw new NotImplementedException();
        }

        public int Size()
        {
            return Name->Sizeof() + sizeof(Transform) + Components.Sizeof() + Meshes.Sizeof();
        }
    }
}