namespace HexaEngine.Meshes
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Lights;
    using HexaEngine.Resources;

    public enum ChannelMask
    {
        None = 0,
        R = 1,
        G = 2,
        B = 4,
        A = 8,
        All = R | G | B | A,
    }

    public class TerrainMaterialBundle
    {
        public TerrainShader Shader;

        public TerrainMaterialBundle(IGraphicsDevice device)
        {
            Shader = new(device, Array.Empty<ShaderMacro>(), false);
            Shader.Initialize();
        }

        public void Update(IGraphicsDevice device, TerrainLayer?[] terrainLayers)
        {
            List<ShaderMacro> layerMacros = new();
            for (int i = 0; i < terrainLayers.Length; i++)
            {
                var layer = terrainLayers[i];
                if (layer == null || layer.Data == null) continue;

                var macros = layer.Data.GetShaderMacros();
                for (int j = 0; j < macros.Length; j++)
                {
                    macros[j].Name += i;
                }

                layerMacros.AddRange(macros);
            }

            Shader?.Dispose();
            Shader = new(device, layerMacros.ToArray(), false);
            Shader.Initialize();
        }

        public bool Bind(IGraphicsContext context)
        {
            return true;
        }

        public void Unbind(IGraphicsContext context)
        {
        }
    }

    public class TerrainDrawLayer
    {
        public UavTexture2D Mask;
        public TerrainMaterialBundle Material;
        public ChannelMask UsedChannels;
        public TerrainLayer?[] TerrainLayers = new TerrainLayer[4];
        public ISamplerState MaskSampler;

        public TerrainDrawLayer(IGraphicsDevice device)
        {
            Material = new(device);
            Mask = new(device, Format.R16G16B16A16UNorm, 1024, 1024, 1, 1, true, true);
            MaskSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
        }

        public ChannelMask GetChannelMask(TerrainLayer layer)
        {
            return GetChannelMask(Array.IndexOf(TerrainLayers, layer));
        }

        public UavTexture2D GetMask() => Mask;

        public static ChannelMask GetChannelMask(int layer)
        {
            return layer switch
            {
                0 => ChannelMask.R,
                1 => ChannelMask.G,
                2 => ChannelMask.B,
                3 => ChannelMask.A,
                _ => ChannelMask.None,
            };
        }

        public bool AddLayer(TerrainLayer layer)
        {
            if (UsedChannels == ChannelMask.All)
                return false;

            if ((UsedChannels & ChannelMask.R) == 0)
            {
                TerrainLayers[0] = layer;
                UsedChannels |= ChannelMask.R;
                return true;
            }
            if ((UsedChannels & ChannelMask.G) == 0)
            {
                TerrainLayers[1] = layer;
                UsedChannels |= ChannelMask.G;
                return true;
            }
            if ((UsedChannels & ChannelMask.B) == 0)
            {
                TerrainLayers[2] = layer;
                UsedChannels |= ChannelMask.B;
                return true;
            }
            if ((UsedChannels & ChannelMask.A) == 0)
            {
                TerrainLayers[3] = layer;
                UsedChannels |= ChannelMask.A;
                return true;
            }

            return false;
        }

        public bool RemoveLayer(TerrainLayer layer)
        {
            int index = Array.IndexOf(TerrainLayers, layer);
            if (index == -1)
                return false;
            ChannelMask mask = GetChannelMask(layer);
            UsedChannels ^= mask;
            TerrainLayers[index] = null;
            return true;
        }

        public bool ContainsLayer(TerrainLayer layer)
        {
            return Array.IndexOf(TerrainLayers, layer) != -1;
        }

        public void UpdateLayerMaterials(IGraphicsDevice device)
        {
            Material.Update(device, TerrainLayers);
        }

        public bool BeginDrawForward(IGraphicsContext context)
        {
            if (!Material.Shader.BeginDrawForward(context))
            {
                return false;
            }
            if (!Material.Bind(context))
            {
                return false;
            }

            context.PSSetShaderResource(19, Mask.SRV);

            return true;
        }

        public bool BeginDrawForward(IGraphicsContext context, IBuffer camera)
        {
            if (!Material.Shader.BeginDrawForward(context, camera))
            {
                return false;
            }
            if (!Material.Bind(context))
            {
                return false;
            }

            context.PSSetShaderResource(19, Mask.SRV);

            return true;
        }

        public void EndDrawForward(IGraphicsContext context)
        {
            Material.Shader.EndDrawForward(context);
            Material.Unbind(context);
            context.PSSetShaderResource(19, null);
        }

        public bool BeginDrawDeferred(IGraphicsContext context)
        {
            if (!Material.Shader.BeginDrawDeferred(context))
            {
                return false;
            }
            if (!Material.Bind(context))
            {
                return false;
            }

            context.PSSetShaderResource(19, Mask.SRV);

            return true;
        }

        public bool BeginDrawDeferred(IGraphicsContext context, IBuffer camera)
        {
            if (!Material.Shader.BeginDrawDeferred(context, camera))
            {
                return false;
            }
            if (!Material.Bind(context))
            {
                return false;
            }

            context.PSSetShaderResource(19, Mask.SRV);

            return true;
        }

        public void EndDrawDeferred(IGraphicsContext context)
        {
            Material.Shader.EndDrawDeferred(context);
            Material.Unbind(context);
            context.PSSetShaderResource(19, null);
        }

        public bool BeginDrawDepth(IGraphicsContext context, IBuffer camera)
        {
            if (!Material.Shader.BeginDrawDepth(context, camera))
            {
                return false;
            }

            context.PSSetShaderResource(0, Mask.SRV);
            context.PSSetSampler(0, MaskSampler);

            return true;
        }

        public bool BeginDrawDepth(IGraphicsContext context)
        {
            if (!Material.Shader.BeginDrawDepth(context))
            {
                return false;
            }

            context.PSSetShaderResource(0, Mask.SRV);
            context.PSSetSampler(0, MaskSampler);

            return true;
        }

        public void EndDrawDepth(IGraphicsContext context)
        {
            Material.Shader.EndDrawDepth(context);
            context.PSSetShaderResource(0, null);
            context.PSSetSampler(0, null);
        }

        public bool BeginDrawShadow(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (!Material.Shader.BeginDrawShadow(context, light, type))
            {
                return false;
            }

            return true;
        }

        public void EndDrawShadow(IGraphicsContext context)
        {
            Material.Shader.EndDrawShadow(context);
        }
    }

    public class TerrainLayer
    {
        public TerrainLayer(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public MaterialData? Data { get; set; }
    }
}