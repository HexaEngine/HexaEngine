using HexaEngine.Core.IO.Binary.Archives;
using System.Diagnostics.CodeAnalysis;

namespace HexaEngine.Core.IO
{
    public interface IAssetNamespace
    {
        public string Name { get; set; }

        public bool TryGetAsset(Guid guid, [NotNullWhen(true)] out IAssetEntry? entry);

        public bool TryGetAsset(ReadOnlySpan<char> path, [NotNullWhen(true)] out IAssetEntry? entry);
    }
}