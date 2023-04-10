namespace HexaEngine.Core.Lights
{
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct ProbeBufferParams
    {
        public uint GlobalProbes;
        public uint LocalProbes;
        public uint Padding0;
        public uint Padding1;
    }

    public struct LightBufferParams
    {
        public uint DirectionalLights;
        public uint PointLights;
        public uint Spotlights;
        public uint Padding;
    }
}