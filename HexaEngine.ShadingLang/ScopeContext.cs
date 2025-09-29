using HexaEngine.ShadingLang.Text;

namespace HexaEngine.ShadingLang
{
    public unsafe struct ScopeContext
    {
        public TextSpan Name;
        public ScopeType Type;
        public void* Userdata;

        public ScopeContext(TextSpan name, ScopeType type, void* userdata)
        {
            Name = name;
            Type = type;
            Userdata = userdata;
        }
    }
}