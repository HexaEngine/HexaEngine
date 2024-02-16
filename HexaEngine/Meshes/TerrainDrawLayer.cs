﻿namespace HexaEngine.Meshes
{
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using System.Numerics;
    using MaterialShader = Resources.MaterialShader;

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
        public Guid Id = Guid.NewGuid();
        public MaterialShader Shader;
        public MaterialTextureList TextureList = [];

        private readonly StaticTerrainLayer?[] terrainLayers;

        public TerrainMaterialBundle(StaticTerrainLayer?[] terrainLayers)
        {
            List<ShaderMacro> layerMacros = new();
            for (int i = 0; i < terrainLayers.Length; i++)
            {
                var layer = terrainLayers[i];
                if (layer == null || layer.Data == null) continue;

                var material = layer.Data;

                var macros = material.GetShaderMacros();
                for (int j = 0; j < macros.Length; j++)
                {
                    macros[j].Name += i;
                }

                layerMacros.AddRange(macros);

                for (int j = 0; j < material.Textures.Count; j++)
                {
                    var textureDesc = material.Textures[j];
                    var texture = ResourceManager.Shared.LoadTexture(textureDesc);
                    TextureList.Add(texture);
                }
            }

            TextureList.StartTextureSlot = 12;
            TextureList.StartSamplerSlot = 4;
            TextureList.Update();

            MaterialShaderDesc shaderDesc = GetMaterialShaderDesc(Id, layerMacros.ToArray(), false);

            Shader?.Dispose();
            Shader = ResourceManager.Shared.LoadMaterialShader(shaderDesc) ?? throw new NotSupportedException();
            this.terrainLayers = terrainLayers;
        }

        public void Update()
        {
            for (int i = 0; i < TextureList.Count; i++)
            {
                TextureList[i]?.Dispose();
            }
            TextureList.Clear();

            List<ShaderMacro> layerMacros = new();
            for (int i = 0; i < terrainLayers.Length; i++)
            {
                var layer = terrainLayers[i];
                if (layer == null || layer.Data == null) continue;

                var material = layer.Data;
                var macros = material.GetShaderMacros();

                for (int j = 0; j < macros.Length; j++)
                {
                    macros[j].Name += i;
                }

                layerMacros.AddRange(macros);

                for (int j = 0; j < material.Textures.Count; j++)
                {
                    var textureDesc = material.Textures[j];
                    var texture = ResourceManager.Shared.LoadTexture(textureDesc);
                    TextureList.Add(texture);
                }
            }

            TextureList.StartTextureSlot = 12;
            TextureList.StartSamplerSlot = 4;
            TextureList.Update();

            MaterialShaderDesc shaderDesc = GetMaterialShaderDesc(Id, layerMacros.ToArray(), false);
            ResourceManager.Shared.UpdateMaterialShader(Shader, shaderDesc);
        }

        public void Setup()
        {
        }

        private static MaterialShaderPassDesc[] GetMaterialShaderPasses(bool alphaBlend)
        {
            List<MaterialShaderPassDesc> passes = [];

            //flags = 0;
            var elements = TerrainCellData.InputElements;

            ShaderMacro[] macros = [new("CLUSTERED_FORWARD", "1")];

            bool twoSided = false;

            bool blendFunc = alphaBlend;

            RasterizerDescription rasterizer = RasterizerDescription.CullBack;
            if (twoSided)
            {
                rasterizer = RasterizerDescription.CullNone;
            }

            BlendDescription blend = BlendDescription.Opaque;
            if (blendFunc)
            {
                blend = BlendDescription.AlphaBlend;
            }

            GraphicsPipelineDesc pipelineDescForward = new()
            {
                VertexShader = $"forward/terrain/vs.hlsl",
                PixelShader = $"forward/terrain/ps.hlsl",
                Macros = macros,
            };

            GraphicsPipelineStateDesc pipelineStateDescForward = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = rasterizer,
                Blend = blend,
                Topology = PrimitiveTopology.TriangleList,
                BlendFactor = Vector4.One,
            };

            GraphicsPipelineDesc pipelineDescDeferred = new()
            {
                VertexShader = $"deferred/terrain/vs.hlsl",
                PixelShader = $"deferred/terrain/ps.hlsl",
                Macros = macros,
            };

            GraphicsPipelineStateDesc pipelineStateDescDeferred = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = rasterizer,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            passes.Add(new("Forward", pipelineDescForward, pipelineStateDescForward));
            passes.Add(new("Deferred", pipelineDescDeferred, pipelineStateDescDeferred));

            pipelineDescDeferred.VertexShader = "forward/terrain/depthVS.hlsl";
            pipelineDescDeferred.PixelShader = "forward/terrain/depthPS.hlsl";

            passes.Add(new("DepthOnly", pipelineDescDeferred, pipelineStateDescDeferred));

            GraphicsPipelineDesc csmPipelineDesc = new()
            {
                VertexShader = "forward/terrain/csm/vs.hlsl",
                GeometryShader = "forward/terrain/csm/gs.hlsl",
                Macros = macros,
            };

            GraphicsPipelineStateDesc csmPipelineStateDesc = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            GraphicsPipelineDesc osmPipelineDesc = new()
            {
                VertexShader = "forward/terrain/osm/vs.hlsl",
                PixelShader = "forward/terrain/osm/ps.hlsl",
                Macros = macros,
            };

            GraphicsPipelineStateDesc osmPipelineStateDesc = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            GraphicsPipelineDesc psmPipelineDesc = new()
            {
                VertexShader = "forward/terrain/psm/vs.hlsl",
                Macros = macros,
            };

            GraphicsPipelineStateDesc psmPipelineStateDesc = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullFront,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
                InputElements = elements,
            };

            passes.Add(new("Directional", csmPipelineDesc, csmPipelineStateDesc));
            passes.Add(new("Omnidirectional", osmPipelineDesc, osmPipelineStateDesc));
            passes.Add(new("Perspective", psmPipelineDesc, psmPipelineStateDesc));

            //flags |= MaterialShaderFlags.Shadow | MaterialShaderFlags.DepthTest;

            return [.. passes];
        }

        private static MaterialShaderDesc GetMaterialShaderDesc(Guid id, ShaderMacro[] macros, bool alphaBlend)
        {
            var passes = GetMaterialShaderPasses(alphaBlend);
            return new(id, [.. macros], TerrainCellData.InputElements, passes);
        }

        public bool BeginDraw(IGraphicsContext context, string passName)
        {
            var pass = Shader.Find(passName);
            if (pass == null)
            {
                return false;
            }

            if (!pass.BeginDraw(context))
            {
                return false;
            }

            TextureList.Bind(context);

            return true;
        }

        public void EndDraw(IGraphicsContext context)
        {
            TextureList.Unbind(context);
            context.SetPipelineState(null);
        }

        public void DrawIndexedInstanced(IGraphicsContext context, string pass, uint indexCount, uint instanceCount, uint indexOffset = 0, int vertexOffset = 0, uint instanceOffset = 0)
        {
            if (!BeginDraw(context, pass))
            {
                return;
            }
            context.DrawIndexedInstanced(indexCount, instanceCount, indexOffset, vertexOffset, instanceOffset);
            EndDraw(context);
        }

        public void DrawInstanced(IGraphicsContext context, string pass, uint vertexCount, uint instanceCount, uint vertexOffset = 0, uint instanceOffset = 0)
        {
            if (!BeginDraw(context, pass))
            {
                return;
            }
            context.DrawInstanced(vertexCount, instanceCount, vertexOffset, instanceOffset);
            EndDraw(context);
        }
    }

    public class TerrainDrawLayer
    {
        public Texture2D Mask;
        public TerrainMaterialBundle Material;
        public ChannelMask UsedChannels;
        public StaticTerrainLayer?[] TerrainLayers = new StaticTerrainLayer[4];
        public ISamplerState MaskSampler;

        public unsafe TerrainDrawLayer(IGraphicsDevice device, bool isDefault = false)
        {
            if (isDefault)
            {
                var data = AllocT<ulong>(1024 * 1024);
                for (int i = 0; i < 1024 * 1024; i++)
                {
                    data[i] = EncodePixel(1, 0, 0, 0);
                }
                Texture2DDescription desc = new(Format.R16G16B16A16UNorm, 1024, 1024, 1, 1, GpuAccessFlags.All);
                Mask = new(device, desc, new SubresourceData(data, sizeof(ulong) * 1024));
                Free(data);
            }
            else
            {
                Mask = new(device, Format.R16G16B16A16UNorm, 1024, 1024, 1, 1, CpuAccessFlags.None, GpuAccessFlags.All);
            }

            Material = new(TerrainLayers);

            MaskSampler = device.CreateSamplerState(SamplerStateDescription.LinearClamp);
        }

        public static ulong EncodePixel(float r, float g, float b, float a)
        {
            ulong encodedPixel = 0;
            encodedPixel |= (ulong)(r * 65535.0f);
            encodedPixel |= (ulong)(g * 65535.0f) << 16;
            encodedPixel |= (ulong)(b * 65535.0f) << 32;
            encodedPixel |= (ulong)(a * 65535.0f) << 48;
            return encodedPixel;
        }

        public ChannelMask GetChannelMask(StaticTerrainLayer layer)
        {
            return GetChannelMask(Array.IndexOf(TerrainLayers, layer));
        }

        public Texture2D GetMask() => Mask;

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

        public bool AddLayer(StaticTerrainLayer layer)
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

        public bool RemoveLayer(StaticTerrainLayer layer)
        {
            int index = Array.IndexOf(TerrainLayers, layer);
            if (index == -1)
                return false;
            ChannelMask mask = GetChannelMask(layer);
            UsedChannels ^= mask;
            TerrainLayers[index] = null;
            return true;
        }

        public bool ContainsLayer(StaticTerrainLayer layer)
        {
            return Array.IndexOf(TerrainLayers, layer) != -1;
        }

        public void UpdateLayerMaterials(IGraphicsDevice device)
        {
            Material.Update();
        }
    }

    public class StaticTerrainLayer
    {
        private AssetRef material;

        public StaticTerrainLayer(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public MaterialData? Data { get; set; }

        public AssetRef Material
        {
            get => material;
            set
            {
                Data = BaseRendererComponent.GetMaterial(value);
                material = value;
            }
        }
    }
}