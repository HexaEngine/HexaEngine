using HexaEngine.DirectXTex;

public class Program
{
    public static unsafe void Main()
    {
        byte[] data = File.ReadAllBytes("env_o.dds");
        ScratchImage image = new();
        DirectXTex.LoadFromDDSMemory(data, DDSFlags.NONE, &image);
        var me = image.GetMetadata();
        image.Release();
    }
}