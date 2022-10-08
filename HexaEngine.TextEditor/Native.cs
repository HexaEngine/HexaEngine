namespace HexaEngine.TextEditor
{
    using ImGuiNET;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public unsafe struct TextEditor
    {
    }

    public static unsafe partial class Native
    {
        public const string LibName = "HexaEngine.TextEditor.Native.dll";

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern TextEditor* CreateTextEditor();

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern void ReleaseTextEditor(TextEditor** editor);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        public static extern void Render(TextEditor* editor, byte* aTitle, Vector2* aSize, bool aBorder);
    }
}