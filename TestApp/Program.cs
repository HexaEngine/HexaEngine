using HexaEngine.TextEditor;
using ImGuiNET;
using System.Numerics;
using System.Text.Unicode;

public class Program
{
    public static unsafe void Main()
    {
        var context = ImGui.CreateContext();
        ImGui.SetCurrentContext(context);
        ImGui.GetIO().DisplaySize = new(512, 512);
        var config = new ImFontConfigPtr(ImGuiNative.ImFontConfig_ImFontConfig());

        ImGui.GetIO().Fonts.AddFontDefault(config);
        ImGui.StyleColorsDark();
        ImGui.NewFrame();
        var editor = Native.CreateTextEditor();

        Span<byte> span = new byte[8];
        Utf8.FromUtf16("Test", span, out _, out _);
        fixed (byte* ptr = span)
        {
            Vector2 size = new(256, 256);
            Native.Render(editor, ptr, &size, false);
        }

        Native.ReleaseTextEditor(&editor);
    }
}