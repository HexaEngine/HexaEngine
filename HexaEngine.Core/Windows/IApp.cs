namespace HexaEngine.Core.Windows
{
    public interface IApp
    {
        void Startup();

        void Initialize();

        void Uninitialize();
    }
}