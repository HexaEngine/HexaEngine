namespace HexaEngine.Core.IO
{
    public interface IAssetEntry
    {
        void CopyTo(Stream target);
        byte[] GetData();
        VirtualStream GetStream();
    }
}