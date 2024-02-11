namespace HexaEngine.Editor.Widgets
{
    public struct SymbolicFrame
    {
        public ulong Address;
        public string Symbol;

        public SymbolicFrame(ulong address, string fullMethodName)
        {
            Address = address;
            Symbol = fullMethodName;
        }
    }
}