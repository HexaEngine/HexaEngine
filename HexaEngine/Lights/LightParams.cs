namespace HexaEngine.Lights
{
    using System.Numerics;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct ProbeBufferParams
    {
        public uint GlobalProbes;
        public uint LocalProbes;
        public uint Padding0;
        public uint Padding1;
    }

    public struct LightParams
    {
        public uint LightCount;
        public Vector3 Padding;

        public LightParams(uint lightCount)
        {
            LightCount = lightCount;
            Padding = default;
        }
    }

    public struct ForwardLightBufferParams
    {
        public uint LightCount;
        public uint GlobalProbes;
        public uint LocalProbes;
        public uint Padding;
    }
}