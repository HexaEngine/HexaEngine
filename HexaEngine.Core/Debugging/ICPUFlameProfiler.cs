namespace HexaEngine.Core.Debugging
{
    using HexaEngine.Core.UI;

    public interface ICPUFlameProfiler : ICPUProfiler
    {
        ref Scope this[int index] { get; }

        unsafe Entry* Current { get; }

        ImGuiWidgetFlameGraph.ValuesGetter Getter { get; }

        int StageCount { get; }

        void DestroyStage(uint id);

        void EndFrame();

        int GetCurrentEntryIndex();
    }
}