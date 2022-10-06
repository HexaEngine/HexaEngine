namespace ImGuiNET
{
    public unsafe partial struct ImDrawDataPtr
    {
        public RangePtrAccessor<ImDrawListPtr> CmdListsRange => new(CmdLists.ToPointer(), CmdListsCount);
    }
}
