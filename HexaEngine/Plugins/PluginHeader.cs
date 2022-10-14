namespace HexaEngine.Plugins
{
    using HexaEngine.Core.Unsafes;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PluginHeader
    {
        public Endianness Endianness;
        public UnsafeString Name;
        public UnsafeString Description;
        public Version Version;
        public Version FormatVersion;
    }
}