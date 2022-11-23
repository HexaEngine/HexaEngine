namespace HexaEngine.Plugins
{
    using HexaEngine.Core.Unsafes;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PluginHeader
    {
        public Endianness Endianness;
        public PluginVersion FormatVersion;
        public PluginVersion Version;
        public ulong RecordCount;
        public char* Name;
        public char* Description;
        public char** Dependencies;
        public uint DependencyCount;
    }
}