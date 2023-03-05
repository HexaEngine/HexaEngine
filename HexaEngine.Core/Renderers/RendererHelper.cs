namespace HexaEngine.Core.Renderers
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using System;
    using System.Threading.Tasks;

    public static class RendererHelper
    {
        public static Task<Texture> LoadTexture2DAsync(IGraphicsDevice device, string path)
        {
            var state = (device, path);
            return Task.Factory.StartNew((object? state) =>
            {
                var p = ((IGraphicsDevice, string))state;
                var device = p.Item1;
                var path = Paths.CurrentTexturePath + p.Item2;

                if (FileSystem.Exists(path))
                {
                    try
                    {
                        return Texture.CreateTextureAsync(device, new TextureFileDescription(path, TextureDimension.Texture2D)).Result;
                    }
                    catch (Exception ex)
                    {
                        ImGuiConsole.Log(ex);
                        return Texture.CreateTextureAsync(device, TextureDimension.Texture2D, default).Result;
                    }
                }
                else
                {
                    return Texture.CreateTextureAsync(device, TextureDimension.Texture2D, default).Result;
                }
            }, state);
        }

        public static Task<Texture> LoadTextureCubeAsync(IGraphicsDevice device, string path)
        {
            var state = (device, path);
            return Task.Factory.StartNew((object? state) =>
            {
                var p = ((IGraphicsDevice, string))state;
                var device = p.Item1;
                var path = Paths.CurrentTexturePath + p.Item2;

                if (FileSystem.Exists(path))
                {
                    try
                    {
                        return Texture.CreateTextureAsync(device, new TextureFileDescription(path, TextureDimension.TextureCube)).Result;
                    }
                    catch (Exception ex)
                    {
                        ImGuiConsole.Log(ex);
                        return Texture.CreateTextureAsync(device, TextureDimension.TextureCube, default).Result;
                    }
                }
                else
                {
                    return Texture.CreateTextureAsync(device, TextureDimension.TextureCube, default).Result;
                }
            }, state);
        }
    }
}