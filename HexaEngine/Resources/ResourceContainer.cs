namespace HexaEngine.Resources
{
    public class ResourceContainer
    {
        private List<string> files = new();
        private object _lock = new();
    }
}