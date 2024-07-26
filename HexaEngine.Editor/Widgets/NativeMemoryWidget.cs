using HexaEngine.Core.Extensions;

namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using Hexa.NET.Utilities;
    using HexaEngine.Editor.Attributes;
    using System.Runtime.InteropServices;

    [EditorWindowCategory("Debug")]
    public class NativeMemoryWidget : EditorWindow
    {
        protected override string Name { get; } = "Native Memory";

        public override void DrawContent(IGraphicsContext context)
        {
            if (Allocator is AllocationCallbacks callbacks)
            {
#if TRACELEAK
                long sum = callbacks.Allocations.Sum(x => x.Size);
                ImGui.Text($"Allocated memory: {sum.FormatDataSize()}");
                foreach (var allocation in callbacks.Allocations.OrderByDescending(x => x.Size))
                {
                    ImGui.Text($"{allocation.Caller}, {((long)allocation.Size).FormatDataSize()}");
                }
#else
                ReadOnlySpan<byte> utf8 = "Memory profiling disabled."u8;
                ref byte utf8Ref = ref MemoryMarshal.GetReference(utf8);
                ImGui.Text(ref utf8Ref);
#endif
            }
        }
    }
}