namespace HexaEngine.Plugins
{
    public unsafe struct Plugin
    {
        public PluginHeader* Header;
        public Record** Records;
    }
}