namespace AssetsBundler
{
    public class Asset
    {
        public string Path { get; set; }

        public byte[] Data { get; set; }

        public bool IsEmpty => Data is null || Data.Length == 0;
    }
}