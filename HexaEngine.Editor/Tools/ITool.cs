namespace HexaEngine.Editor.Tools
{
    using System.Threading.Tasks;

    public interface ITool
    {
        string Name { get; }

        string Filter { get; }

        bool CanOpen(string path);

        Task Open(string path);
    }
}