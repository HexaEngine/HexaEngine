namespace HexaEngine.Vulkan
{
    using HexaEngine.Core;
    using HexaEngine.Core.IO;
    using HexaEngine.Shaderc;

    public unsafe class IncludeHandler
    {
        private readonly Stack<string> paths = new();
        private string basePath;

        public IncludeHandler(string basepath)
        {
            basePath = basepath;
        }

        public ShadercIncludeResult* Include(void* userdata, byte* requestedSource, int type, byte* requestingSource, nuint includeDepth)
        {
            string requested = ToStringFromUTF8(requestedSource) ?? string.Empty;
            string requesting = ToStringFromUTF8(requestingSource) ?? string.Empty;
            string path = Path.Combine(basePath, requested);

            ShadercIncludeResult* result = AllocT<ShadercIncludeResult>();

            paths.Push(basePath);
            var dirName = Path.GetDirectoryName(path);
            basePath = dirName;

            var blob = FileSystem.ReadBlob(path);
            result->Content = blob.Data;
            result->ContentLength = (nuint)blob.Length;
            result->SourceName = path.ToUTF8Ptr();
            result->SourceNameLength = (nuint)path.Length;
            return result;
        }

        public void IncludeRelease(void* userdata, ShadercIncludeResult* result)
        {
            basePath = paths.Pop();
            Free(result->Content);
            Free(result->SourceName);
            Free(result);
        }
    }
}