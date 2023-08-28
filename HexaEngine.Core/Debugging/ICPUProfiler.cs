namespace HexaEngine.Core.Debugging
{
    public interface ICPUProfiler
    {
        double this[string stage] { get; }

        void CreateStage(string name);

        void DestroyStage(string name);

        void BeginFrame();

        void Begin(string stage);

        void End(string stage);
    }
}