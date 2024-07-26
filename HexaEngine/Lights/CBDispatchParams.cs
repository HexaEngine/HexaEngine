namespace HexaEngine.Lights
{
    using Hexa.NET.Mathematics;

    public struct CBDispatchParams
    {
        public UPoint3 NumThreadGroups;
        public float Padding0;
        public UPoint3 NumThreads;
        public float Padding1;
    }
}