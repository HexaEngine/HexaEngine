using HexaEngine.Core.Graphics;

namespace HexaEngine.Scenes.Managers
{
    public readonly unsafe struct LightIndex
    {
        public LightIndex(uint index)
        {
            Value = Alloc<uint>();
            *Value = index;
        }

        public readonly uint* Value;

        public uint GetValue() => *Value;

        public void Release()
        {
            Free(Value);
        }
    }
}