namespace HexaEngine.Plugins
{
    public interface IPlugin
    {
        string Name { get; }

        string Version { get; }

        string Description { get; }

        void OnEnable();

        void OnInitialize(IServiceProvider provider);

        void OnUninitialize();

        void OnDisable();
    }
}