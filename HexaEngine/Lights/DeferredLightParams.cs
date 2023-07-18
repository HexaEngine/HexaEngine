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

    public struct DeferredLightParams
    {
        public uint LightCount;
        public Vector3 Padding;

        public DeferredLightParams(uint lightCount)
        {
            LightCount = lightCount;
            Padding = default;
        }
    }

    public struct ForwardLightParams
    {
        public uint LightCount;
        public uint GlobalProbes;
        public uint LocalProbes;
        public uint Padding;
    }

    public struct CullLightParams
    {
        public uint LightCount;
        public Vector3 Padding;

        public CullLightParams(uint lightCount)
        {
            LightCount = lightCount;
            Padding = default;
        }
    }
}