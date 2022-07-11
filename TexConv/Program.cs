// See https://aka.ms/new-console-template for more information
using DirectXTexNet;

foreach (var item in Directory.EnumerateFiles(args[0], "*.*", SearchOption.AllDirectories))
{
    ScratchImage image;
    string path;
    Console.WriteLine($"reading: {item}");

    switch (Path.GetExtension(item))
    {
        case ".dds":
            path = item;
            image = TexHelper.Instance.LoadFromDDSFile(item, DDS_FLAGS.NONE);
            break;

        case ".png":
        case ".jpg":
            path = Path.ChangeExtension(item, ".dds");
            image = TexHelper.Instance.LoadFromWICFile(item, WIC_FLAGS.NONE);
            break;

        default:
            continue;
    }

    {
        Console.WriteLine($"compressing: {item}");
        var image1 = image.Compress(DXGI_FORMAT.BC7_UNORM, TEX_COMPRESS_FLAGS.PARALLEL, 0.5f);
        image.Dispose();
        image = image1;
    }

    Console.WriteLine($"writing: {item}");
    image.SaveToDDSFile(DDS_FLAGS.NONE, path);
}