namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.Unsafes;

    public struct Entry
    {
        public ulong Start;
        public ulong End;
        public UnsafeList<Scope> Stages = new();

        public Entry()
        {
        }

        public uint IdToIndex(uint id)
        {
            for (uint i = 0; i < Stages.Size; i++)
            {
                if (Stages[i].Id == id)
                    return i;
            }
            return unchecked((uint)-1);
        }
    }
}