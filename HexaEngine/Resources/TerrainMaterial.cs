namespace HexaEngine.Resources
{
    using HexaEngine.Configuration;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Resources.Factories;
    using System;
    using System.Numerics;

    public class TerrainMaterial
    {
        public Guid Id;
        private MaterialShader shader;
        private readonly MaterialTextureList textureList = [];

        private readonly TerrainLayerGroup group;

        public unsafe Guid Combine(Guid guid1, Guid guid2)
        {
            return Combine(&guid1, &guid2);
        }

        public unsafe Guid Combine(Guid* guid1, Guid* guid2)
        {
            Guid result = Guid.Empty;
            byte* p0 = (byte*)&result;
            byte* p1 = (byte*)guid1;
            byte* p2 = (byte*)guid2;
            for (int i = 0; i < sizeof(Guid); i++)
            {
                p0[i] = (byte)(p1[i] ^ p2[i]);
            }
            return result;
        }

        public TerrainMaterial(TerrainLayerGroup group)
        {
            this.group = group;
            Guid guid = Guid.Empty;
            bool hasDisplacement = false;
            List<ShaderMacro> layerMacros = new();
            for (int i = 0; i < group.Count; i++)
            {
                var layer = group[i];
                if (layer == null || layer.Data == null)
                {
                    continue;
                }

                guid = Combine(guid, layer.Material.Guid);

                var material = layer.Data;
                material.Updated += OnMaterialUpdated;
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
                    if (textureDesc.Type == MaterialTextureType.Displacement)
                    {
                        hasDisplacement = true;
                    }
                    textureList.Add(texture);
                }
            }
            Id = guid;

            MaterialShaderDesc shaderDesc = GetMaterialShaderDesc(Id, [.. layerMacros], true, hasDisplacement);
            shader = ResourceManager.Shared.LoadMaterialShader<TerrainMaterial>(shaderDesc);

            textureList.Update(shader);
        }

        public MaterialShader Shader => shader;

        public void Update()
        {
            textureList.DisposeResources();

            Guid guid = Guid.Empty;
            bool hasDisplacement = false;
            List<ShaderMacro> layerMacros = [];
            for (int i = 0; i < group.Count; i++)
            {
                var layer = group[i];
                if (layer == null || layer.Data == null)
                {
                    continue;
                }

                guid = Combine(guid, layer.Material.Guid);

                var material = layer.Data;
                material.Updated -= OnMaterialUpdated;
                material.Updated += OnMaterialUpdated;
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
                    if (textureDesc.Type == MaterialTextureType.Displacement)
                    {
                        hasDisplacement = true;
                    }
                    textureList.Add(texture);
                }
            }

            MaterialShaderDesc shaderDesc = GetMaterialShaderDesc(guid, [.. layerMacros], true, hasDisplacement);

            if (Id != guid)
            {
                var tmp = shader;
                Volatile.Write(ref shader, ResourceManager.Shared.LoadMaterialShader<TerrainMaterial>(shaderDesc));
                tmp.Dispose();
            }
            else
            {
                ResourceManager.Shared.UpdateMaterialShader(shader, shaderDesc);
            }
            Id = guid;

            textureList.Update(shader);
        }

        protected virtual void OnMaterialUpdated(object? sender, MaterialData e)
        {
            Update();
        }

        private static MaterialShaderPassDesc[] GetMaterialShaderPasses(bool alphaBlend, bool tessellate)
        {
            List<MaterialShaderPassDesc> passes = [];

            //flags = 0;

            ShaderMacro[] macros = [new("SOFT_SHADOWS", (int)GraphicsSettings.SoftShadowMode)];
            ShaderMacro[] shadowMacros = [new("SOFT_SHADOWS", (int)GraphicsSettings.SoftShadowMode)];

            if (tessellate)
            {
                macros = [.. macros, new("TESSELLATION", "1")];
            }

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
                blend = BlendDescription.Additive;
            }

            GraphicsPipelineDesc pipelineDescForward = new()
            {
                VertexShader = "HexaEngine.Core:shaders/forward/terrain/lit/vs.hlsl",
                PixelShader = "HexaEngine.Core:shaders/forward/terrain/lit/ps.hlsl",
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
                VertexShader = "HexaEngine.Core:shaders/deferred/terrain/vs.hlsl",
                PixelShader = "HexaEngine.Core:shaders/deferred/terrain/ps.hlsl",
                Macros = macros,
            };

            GraphicsPipelineStateDesc pipelineStateDescDeferred = new()
            {
                DepthStencil = DepthStencilDescription.DepthRead,
                Rasterizer = rasterizer,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            GraphicsPipelineDesc pipelineDescDepth = new()
            {
                VertexShader = "HexaEngine.Core:shaders/forward/terrain/depth/vs.hlsl",
                PixelShader = "HexaEngine.Core:shaders/forward/terrain/depth/ps.hlsl",
                Macros = macros,
            };

            GraphicsPipelineStateDesc pipelineStateDescDepth = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = rasterizer,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            if (tessellate)
            {
                pipelineDescForward.HullShader = "HexaEngine.Core:shaders/forward/terrain/lit/hs.hlsl";
                pipelineDescForward.DomainShader = "HexaEngine.Core:shaders/forward/terrain/lit/ds.hlsl";
                pipelineStateDescForward.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                pipelineDescDeferred.HullShader = "HexaEngine.Core:shaders/deferred/terrain/hs.hlsl";
                pipelineDescDeferred.DomainShader = "HexaEngine.Core:shaders/deferred/terrain/ds.hlsl";
                pipelineStateDescDeferred.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                pipelineDescDepth.HullShader = "HexaEngine.Core:shaders/forward/terrain/depth/hs.hlsl";
                pipelineDescDepth.DomainShader = "HexaEngine.Core:shaders/forward/terrain/depth/ds.hlsl";
            }

            passes.Add(new("Forward", pipelineDescForward, pipelineStateDescForward, false, "HexaEngine.Core:shaders/forward/terrain/surface.hlsl"));
            passes.Add(new("Deferred", pipelineDescDeferred, pipelineStateDescDeferred, false, "HexaEngine.Core:shaders/deferred/terrain/surface.hlsl"));
            passes.Add(new("DepthOnly", pipelineDescDepth, pipelineStateDescDepth, false));

            GraphicsPipelineDesc pipelineDescUnlit = new()
            {
                VertexShader = "HexaEngine.Core:shaders/deferred/terrain/unlit/vs.hlsl",
                PixelShader = "HexaEngine.Core:shaders/deferred/terrain/unlit/ps.hlsl",
                Macros = macros,
            };

            GraphicsPipelineStateDesc pipelineStateDescUnlit = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = rasterizer,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            GraphicsPipelineDesc csmPipelineDesc = new()
            {
                VertexShader = "HexaEngine.Core:shaders/forward/terrain/csm/vs.hlsl",
                GeometryShader = "HexaEngine.Core:shaders/forward/terrain/csm/gs.hlsl",
                PixelShader = "HexaEngine.Core:shaders/forward/terrain/csm/ps.hlsl",
                Macros = shadowMacros,
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
                VertexShader = "HexaEngine.Core:shaders/forward/terrain/osm/vs.hlsl",
                PixelShader = "HexaEngine.Core:shaders/forward/terrain/osm/ps.hlsl",
                Macros = shadowMacros,
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
                VertexShader = "HexaEngine.Core:shaders/forward/terrain/psm/vs.hlsl",
                PixelShader = "HexaEngine.Core:shaders/forward/terrain/psm/ps.hlsl",
                Macros = shadowMacros,
            };

            GraphicsPipelineStateDesc psmPipelineStateDesc = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = RasterizerDescription.CullNone,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            passes.Add(new("Directional", csmPipelineDesc, csmPipelineStateDesc, false));
            passes.Add(new("Omnidirectional", osmPipelineDesc, osmPipelineStateDesc, false));
            passes.Add(new("Perspective", psmPipelineDesc, psmPipelineStateDesc, false));

            //flags |= MaterialShaderFlags.Shadow | MaterialShaderFlags.DepthTest;

            return [.. passes];
        }

        private static MaterialShaderDesc GetMaterialShaderDesc(Guid id, ShaderMacro[] macros, bool alphaBlend, bool tessellate)
        {
            var passes = GetMaterialShaderPasses(alphaBlend, tessellate);
            return new(id, [.. macros], [], TerrainCellData.InputElements, passes);
        }

        public MaterialShaderPass? GetPass(string passName)
        {
            var pass = shader.Find(passName);
            if (pass == null)
            {
                return null;
            }

            return pass;
        }

        public bool BeginDraw(IGraphicsContext context, string passName)
        {
            var pass = shader.Find(passName);
            if (pass == null)
            {
                return false;
            }

            if (!pass.BeginDraw(context))
            {
                return false;
            }

            return true;
        }

        public void EndDraw(IGraphicsContext context)
        {
            context.SetGraphicsPipelineState(null);
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

        public void DrawIndexedInstancedIndirect(IGraphicsContext context, string pass, IBuffer drawArgs, uint offset)
        {
            if (!BeginDraw(context, pass))
            {
                return;
            }
            context.DrawIndexedInstancedIndirect(drawArgs, offset);
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

        public void Dispose()
        {
            for (int i = 0; i < group.Count; i++)
            {
                var layer = group[i];
                if (layer == null || layer.Data == null)
                {
                    continue;
                }

                var material = layer.Data;
                material.Updated -= OnMaterialUpdated;
            }

            for (int i = 0; i < textureList.Count; i++)
            {
                textureList[i]?.Dispose();
            }

            shader.Dispose();
            textureList.Dispose();
        }
    }
}