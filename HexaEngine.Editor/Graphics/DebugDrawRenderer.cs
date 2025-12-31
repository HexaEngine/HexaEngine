namespace HexaEngine.Graphics.Renderers
{
    using Hexa.NET.DebugDraw;
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using System;
    using System.Numerics;

    public static unsafe class DebugDrawRenderer
    {
        private static IGraphicsDevice device = null!;
        private static IGraphicsContext context = null!;
        private static IGraphicsPipelineState pso = null!;
        private static IBuffer vertexBuffer = null!;
        private static IBuffer indexBuffer = null!;
        private static IBuffer constantBuffer = null!;
        private static ISamplerState fontSampler = null!;
        private static IShaderResourceView fontTextureView = null!;
        private static readonly SRVWrapper srvWrapper = new(0);

        private static DebugDrawContext debugDrawContext = null!;

        private static int vertexBufferSize = 5000;
        private static int indexBufferSize = 10000;

        public static void Init(IGraphicsDevice device)
        {
            DebugDrawRenderer.device = device;
            context = device.Context;
            debugDrawContext = DebugDraw.CreateContext();
            CreateDeviceObjects();
        }

        private static void CreateDeviceObjects()
        {
            var blendDesc = new BlendDescription
            {
                AlphaToCoverageEnable = false
            };

            blendDesc.RenderTarget[0] = new RenderTargetBlendDescription
            {
                BlendOperationAlpha = BlendOperation.Add,
                IsBlendEnabled = true,
                BlendOperation = BlendOperation.Add,
                DestinationBlendAlpha = Blend.InverseSourceAlpha,
                DestinationBlend = Blend.InverseSourceAlpha,
                SourceBlend = Blend.SourceAlpha,
                SourceBlendAlpha = Blend.SourceAlpha,
                RenderTargetWriteMask = ColorWriteEnable.All
            };

            var rasterDesc = new RasterizerDescription
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                DepthClipEnable = true,
                AntialiasedLineEnable = true,
            };

            pso = device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.Editor:shaders/debugdraw/vs.hlsl",
                PixelShader = "HexaEngine.Editor:shaders/debugdraw/ps.hlsl",
            }, new()
            {
                DepthStencil = DepthStencilDescription.DepthRead,
                Blend = BlendDescription.NonPremultiplied,
                Rasterizer = rasterDesc,
                InputElements =
                [
                     new("POSITION", 0, Format.R32G32B32Float, 0),
                    new("TEXCOORD", 0, Format.R32G32Float, 0),
                    new("COLOR", 0, Format.R8G8B8A8UNorm, 0),
                ]
            });

            vertexBuffer = device.CreateBuffer(new BufferDescription(vertexBufferSize * sizeof(DebugDrawVert), BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            indexBuffer = device.CreateBuffer(new BufferDescription(indexBufferSize * sizeof(uint), BindFlags.IndexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            constantBuffer = device.CreateBuffer(new BufferDescription(sizeof(Matrix4x4), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            pso.Bindings.SetCBV("matrixBuffer", constantBuffer);

            CreateFontsTexture();
        }

        private static unsafe void CreateFontsTexture()
        {
            int width = 1;
            int height = 1;

            uint* pixels = AllocT<uint>(width * height);
            MemsetT(pixels, 0xffffffff, width * height);

            var texDesc = new Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8UNorm,
                SampleDescription = new SampleDescription { Count = 1 },
                Usage = Usage.Immutable,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None
            };

            var subResource = new SubresourceData
            {
                DataPointer = (nint)pixels,
                RowPitch = texDesc.Width * 4,
                SlicePitch = 0
            };

            var texture = device.CreateTexture2D(texDesc, new[] { subResource });

            var resViewDesc = new ShaderResourceViewDescription
            {
                Format = Format.R8G8B8A8UNorm,
                ViewDimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new Texture2DShaderResourceView { MipLevels = texDesc.MipLevels, MostDetailedMip = 0 }
            };
            fontTextureView = device.CreateShaderResourceView(texture, resViewDesc);
            texture.Dispose();
            debugDrawContext.FontTextureId = fontTextureView.NativePointer;
            var samplerDesc = new SamplerStateDescription
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MipLODBias = 0f,
                ComparisonFunction = ComparisonFunction.Always,
                MinLOD = 0f,
                MaxLOD = 0f
            };
            fontSampler = device.CreateSamplerState(samplerDesc);
            pso.Bindings.SetSRV("tex", fontTextureView);
            pso.Bindings.SetSampler("samplerState", fontSampler);
            Free(pixels);
        }

        public static void BeginDraw()
        {
            DebugDraw.SetCurrentContext(debugDrawContext);
            DebugDraw.NewFrame();
        }

        public static void EndDraw()
        {
            DebugDraw.Render();
            Render(DebugDraw.GetDrawData());
        }

        private class SRVWrapper : IShaderResourceView
        {
            public SRVWrapper(nint p)
            {
                NativePointer = p;
            }

            public ShaderResourceViewDescription Description { get; }

            public nint NativePointer { get; set; }

            public string? DebugName { get; set; }

            public bool IsDisposed { get; }

            public event EventHandler? OnDisposed;

            public void Dispose()
            {
            }
        }

        private static void SetupRenderState(DebugDrawData data)
        {
            context.SetViewport(new(data.Viewport.Offset, data.Viewport.Size));
            context.SetVertexBuffer(vertexBuffer, (uint)sizeof(DebugDrawVert));
            context.SetIndexBuffer(indexBuffer, Format.R32UInt, 0);
            context.SetGraphicsPipelineState(pso);
        }

        private static void Render(DebugDrawData data)
        {
            if (!Application.InEditorMode || data.Viewport.Width <= 0 || data.Viewport.Height <= 0)
            {
                return;
            }
#if DEBUG
            context.BeginEvent("DebugDrawRenderer");
#endif

            if (data.TotalVertices > vertexBufferSize)
            {
                vertexBuffer.Dispose();
                vertexBufferSize = (int)(data.TotalVertices * 1.5f);
                vertexBuffer = device.CreateBuffer(new BufferDescription(vertexBufferSize * sizeof(DebugDrawVert), BindFlags.VertexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            }

            if (data.TotalIndices > indexBufferSize)
            {
                indexBuffer.Dispose();
                indexBufferSize = (int)(data.TotalIndices * 1.5f);
                indexBuffer = device.CreateBuffer(new BufferDescription(indexBufferSize * sizeof(uint), BindFlags.IndexBuffer, Usage.Dynamic, CpuAccessFlags.Write));
            }

            var vertexResource = context.Map(vertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var indexResource = context.Map(indexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var vertexResourcePointer = (DebugDrawVert*)vertexResource.PData;
            var indexResourcePointer = (uint*)indexResource.PData;

            for (int i = 0; i < data.CmdLists.Count; i++)
            {
                var list = data.CmdLists[i];
                MemcpyT(list.Vertices, vertexResourcePointer, list.VertexCount);
                MemcpyT(list.Indices, indexResourcePointer, list.IndexCount);
                vertexResourcePointer += list.VertexCount;
                indexResourcePointer += list.IndexCount;
            }

            context.Unmap(vertexBuffer, 0);
            context.Unmap(indexBuffer, 0);

            var camera = data.Camera;

            SetupRenderState(data);

            for (int i = 0; i < data.CmdLists.Count; i++)
            {
                int voffset = 0;
                uint ioffset = 0;
                var list = data.CmdLists[i];
                for (int j = 0; j < list.Commands.Count; j++)
                {
                    var cmd = list.Commands[j];

                    Matrix4x4 mvp = cmd.Transform * camera;

                    context.Write(constantBuffer, Matrix4x4.Transpose(mvp));

                    var texId = cmd.TextureId;
                    if (texId == 0)
                    {
                        texId = fontTextureView.NativePointer;
                    }

                    srvWrapper.NativePointer = texId;
                    pso.Bindings.SetSRV("fontTex", srvWrapper);
                    context.SetGraphicsPipelineState(pso);
                    context.SetPrimitiveTopology((PrimitiveTopology)cmd.Topology);
                    context.DrawIndexedInstanced(cmd.IndexCount, 1, ioffset, voffset, 0);
                    voffset += (int)cmd.VertexCount;
                    ioffset += cmd.IndexCount;
                }
            }

            context.SetGraphicsPipelineState(null);
            context.SetViewport(default);
            context.SetVertexBuffer(null, 0, 0);
            context.SetIndexBuffer(null, default, 0);
            context.SetPrimitiveTopology(PrimitiveTopology.Undefined);

#if DEBUG
            context.EndEvent();
#endif
        }

        public static void Shutdown()
        {
            debugDrawContext.Dispose();
            pso.Dispose();
            constantBuffer.Dispose();
            vertexBuffer.Dispose();
            indexBuffer.Dispose();
            fontSampler.Dispose();
            fontTextureView.Dispose();
        }
    }
}