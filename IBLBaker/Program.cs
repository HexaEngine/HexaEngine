// See https://aka.ms/new-console-template for more information
using DirectXTexNet;
using HexaEngine.Rendering.D3D;
using HexaEngine.Shaders.Effects;
using Vortice.Direct3D11;
using Vortice.DXGI;

static void ExportDDS(Texture cube, string name)
{
    ScratchImage image = TexHelper.Instance.CaptureTexture(D3D11DeviceManager.ID3D11Device.NativePointer, D3D11DeviceManager.ID3D11DeviceContext.NativePointer, ((ID3D11Texture2D)cube).NativePointer);

    {
        ScratchImage image1 = image.Compress(DXGI_FORMAT.BC1_UNORM, TEX_COMPRESS_FLAGS.PARALLEL, 0.5f);
        image.Dispose();
        image = image1;
    }

    image.SaveToDDSFile(DDS_FLAGS.NONE, name);
}

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

D3D11DeviceManager.Initialize();

IrradianceFilterEffect irradianceFilter = new();
PreFilterEffect preFilter = new();
preFilter.Roughness = 0;

Texture irr = new(D3D11DeviceManager.ID3D11Device, TextureDescription.CreateTextureCubeWithRTV(512, 1, Format.R32G32B32A32_Float));
Texture prf = new(D3D11DeviceManager.ID3D11Device, TextureDescription.CreateTextureCubeWithRTV(1024, 1, Format.R32G32B32A32_Float));
irradianceFilter.Targets = irr;
preFilter.Targets = prf;

SamplerState samplerState = new(SamplerDescription.AnisotropicClamp);
samplerState.Add(new(ShaderStage.Pixel, 0));
irradianceFilter.Samplers.Add(samplerState);
preFilter.Samplers.Add(samplerState);

Console.WriteLine("Load Texture");
switch (mode)
{
    case Mode.Cube:
        TextureCube cube = new(D3D11DeviceManager.ID3D11Device, BindFlags.ShaderResource | BindFlags.RenderTarget, args[1]);
        cube.AddBinding(new(ShaderStage.Pixel, 0));
        irradianceFilter.Resources.Add(cube);
        preFilter.Resources.Add(cube);
        break;

    case Mode.Panorama:
        Texture source = new(D3D11DeviceManager.ID3D11Device, new TextureFileDescription(args[1], 1));
        source.AddBinding(new(ShaderStage.Pixel, 0));
        EquiRectangularToCubeEffect filter = new();
        filter.Resources.Add(source);
        filter.Samplers.Add(samplerState);
        Texture cube1 = new(D3D11DeviceManager.ID3D11Device, TextureDescription.CreateTextureCubeWithRTV(2048, 1, Format.R32G32B32A32_Float));
        cube1.AddBinding(new(ShaderStage.Pixel, 0));
        filter.Targets = cube1;
        filter.Draw(D3D11DeviceManager.ID3D11DeviceContext, null);
        D3D11DeviceManager.ID3D11DeviceContext.GenerateMips(cube1);
        ExportDDS(cube1, "env_o.dds");
        irradianceFilter.Resources.Add(cube1);
        preFilter.Resources.Add(cube1);
        break;
}

Console.WriteLine("Filter IrradianceFilter");
irradianceFilter.Draw(D3D11DeviceManager.ID3D11DeviceContext, null);
D3D11DeviceManager.ID3D11DeviceContext.GenerateMips(irr);
Console.WriteLine("Filter PreFilter");
preFilter.Draw(D3D11DeviceManager.ID3D11DeviceContext, null);
D3D11DeviceManager.ID3D11DeviceContext.GenerateMips(prf);

Console.WriteLine("Exporting");
ExportDDS(irr, "irradiance_o.dds");
ExportDDS(prf, "prefilter_o.dds");
Console.WriteLine("Finished");