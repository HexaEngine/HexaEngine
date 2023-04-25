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

    public struct ForwardLightBufferParams
    {
        public uint DirectionalLights;
        public uint PointLights;
        public uint Spotlights;
        public uint Padding0;
        public uint DirectionalLightSDs;
        public uint PointLightSDs;
        public uint SpotlightSDs;
        public uint Padding1;
        public uint GlobalProbes;
        public uint LocalProbes;
        public uint Padding2;
        public uint Padding3;
    }
}