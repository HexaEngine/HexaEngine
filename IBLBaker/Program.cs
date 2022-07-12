// See https://aka.ms/new-console-template for more information
using DirectXTexNet;
using HexaEngine.Core.Graphics;
using HexaEngine.D3D11;
using HexaEngine.Graphics;
using HexaEngine.Shaders.Effects;

if (args.Length != 2)
{
    Console.WriteLine("Invalid args");
    return;
}

Mode mode;
if (!Enum.TryParse(args[0], true, out mode))
{
    Console.WriteLine("Invalid args, couldn't parse mode");
    return;
}

if (!File.Exists(args[1]))
{
    Console.WriteLine("Invalid args, file not found");
    return;
}

D3D11GraphicsDevice device = new(null);
D3D11GraphicsContext context = (D3D11GraphicsContext)device.Context;

void ExportDDS(Texture cube, string name)
{
    ScratchImage image = TexHelper.Instance.CaptureTexture(device.NativePointer, context.NativePointer, cube.Resource.NativePointer);

    {
        ScratchImage image1 = image.Compress(DXGI_FORMAT.BC1_UNORM, TEX_COMPRESS_FLAGS.PARALLEL, 0.5f);
        image.Dispose();
        image = image1;
    }

    image.SaveToDDSFile(DDS_FLAGS.NONE, name);
}

IrradianceFilterEffect irradianceFilter = new(device);
PreFilterEffect preFilter = new(device);
preFilter.Roughness = 0;

Texture irr = new(device, TextureDescription.CreateTextureCubeWithRTV(512, 1, Format.RGBA32Float));
Texture prf = new(device, TextureDescription.CreateTextureCubeWithRTV(1024, 1, Format.RGBA32Float));
var irrRTV = irr.CreateRTVArray(device);
var prfRTV = prf.CreateRTVArray(device);
irradianceFilter.Targets = irrRTV;
preFilter.Targets = prfRTV;

ISamplerState samplerState = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
irradianceFilter.Samplers.Add(new(samplerState, ShaderStage.Pixel, 0));
preFilter.Samplers.Add(new(samplerState, ShaderStage.Pixel, 0));

Console.WriteLine("Load Texture");
switch (mode)
{
    case Mode.Cube:
        Texture cube = new(device, new TextureFileDescription(args[1], TextureDimension.TextureCube));
        irradianceFilter.Resources.Add(new(cube.ResourceView, ShaderStage.Pixel, 0));
        preFilter.Resources.Add(new(cube.ResourceView, ShaderStage.Pixel, 0));
        break;

    case Mode.Panorama:
        Texture source = new(device, new TextureFileDescription(args[1]));
        EquiRectangularToCubeEffect filter = new(device);
        filter.Resources.Add(new(source.ResourceView, ShaderStage.Pixel, 0));
        filter.Samplers.Add(new(samplerState, ShaderStage.Pixel, 0));
        Texture cube1 = new(device, TextureDescription.CreateTextureCubeWithRTV(2048, 1, Format.RGBA32Float));
        var cu = cube1.CreateRTVArray(device);
        filter.Targets = cu;
        filter.Draw(context, null);
        context.GenerateMips(cube1.ResourceView);
        ExportDDS(cube1, "env_o.dds");
        irradianceFilter.Resources.Add(new(source.ResourceView, ShaderStage.Pixel, 0));
        preFilter.Resources.Add(new(source.ResourceView, ShaderStage.Pixel, 0));
        break;
}

Console.WriteLine("Filter IrradianceFilter");
irradianceFilter.Draw(context, null);
context.GenerateMips(irr.ResourceView);
Console.WriteLine("Filter PreFilter");
preFilter.Draw(context, null);
context.GenerateMips(prf.ResourceView);

Console.WriteLine("Exporting");
ExportDDS(irr, "irradiance_o.dds");
ExportDDS(prf, "prefilter_o.dds");
Console.WriteLine("Finished");