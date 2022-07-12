//based on https://github.com/ocornut/imgui/blob/master/examples/imgui_impl_dx11.cpp

using HexaEngine.Core.Debugging;
using HexaEngine.Core.Graphics;
using HexaEngine.Editor;
using HexaEngine.Mathematics;
using HexaEngine.Windows;
using ImGuiNET;
using ImGuizmoNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using ImDrawIdx = System.UInt16;

namespace HexaEngine.Rendering
{
    public unsafe class ImGuiRenderer
    {
        private const int VertexConstantBufferSize = 16 * 4;

        private IGraphicsDevice device;
        private IGraphicsContext context;
        private ISwapChain swapChain;
        private ImGuiInputHandler inputHandler;
        private IBuffer vertexBuffer;
        private IBuffer indexBuffer;
        private Blob vertexShaderBlob;
        private IVertexShader vertexShader;
        private IInputLayout inputLayout;
        private IBuffer constantBuffer;
        private Blob pixelShaderBlob;
        private IPixelShader pixelShader;
        private ISamplerState fontSampler;
        private IShaderResourceView fontTextureView;
        private IRasterizerState rasterizerState;
        private IBlendState blendState;
        private IDepthStencilState depthStencilState;
        private int vertexBufferSize = 5000, indexBufferSize = 10000;

        private static readonly Dictionary<IntPtr, IShaderResourceView> textureResources = new();

        public ImGuiRenderer(GameWindow window)
        {
            IntPtr igContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(igContext);
            ImGuizmo.SetImGuiContext(igContext);

            device = window.Device;
            context = window.Context;
            swapChain = window.Device.SwapChain;

            var io = ImGui.GetIO();
            io.Fonts.AddFontDefault();
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset | ImGuiBackendFlags.HasMouseCursors;

            CreateDeviceObjects();
            inputHandler = new(window);
        }

        public void BeginDraw()
        {
            inputHandler.Update();
            ImGui.NewFrame();
            ImGuizmo.BeginFrame();
            ImGuiConsole.Draw();
            Designer.Draw();
        }

        public void EndDraw()
        {
            ImGui.Render();
            ImGui.EndFrame();
            context.SetRenderTargets(swapChain.BackbufferRTV, null);
            Render(ImGui.GetDrawData());
        }

        public void Render(ImDrawDataPtr data)
        {
            // Avoid rendering when minimized
            if (data.DisplaySize.X <= 0.0f || data.DisplaySize.Y <= 0.0f)
                return;

            if (data.CmdListsCount == 0)
            {
                return;
            }

            IGraphicsContext ctx = context;

            if (vertexBuffer == null || vertexBufferSize < data.TotalVtxCount)
            {
                vertexBuffer?.Dispose();

                vertexBufferSize = (int)(data.TotalVtxCount * 1.5f);
                BufferDescription desc = new();
                desc.Usage = Usage.Dynamic;
                desc.ByteWidth = vertexBufferSize * sizeof(ImDrawVert);
                desc.BindFlags = BindFlags.VertexBuffer;
                desc.CPUAccessFlags = CpuAccessFlags.Write;
                vertexBuffer = device.CreateBuffer(desc);
            }

            if (indexBuffer == null || indexBufferSize < data.TotalIdxCount)
            {
                indexBuffer?.Dispose();

                indexBufferSize = (int)(data.TotalIdxCount * 1.5f);

                BufferDescription desc = new();
                desc.Usage = Usage.Dynamic;
                desc.ByteWidth = indexBufferSize * sizeof(ImDrawIdx);
                desc.BindFlags = BindFlags.IndexBuffer;
                desc.CPUAccessFlags = CpuAccessFlags.Write;
                indexBuffer = device.CreateBuffer(desc);
            }

            // Upload vertex/index data into a single contiguous GPU buffer
            var vertexResource = ctx.Map(vertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var indexResource = ctx.Map(indexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var vertexResourcePointer = (ImDrawVert*)vertexResource.PData;
            var indexResourcePointer = (ImDrawIdx*)indexResource.PData;
            for (int n = 0; n < data.CmdListsCount; n++)
            {
                var cmdlList = data.CmdListsRange[n];

                var vertBytes = cmdlList.VtxBuffer.Size * sizeof(ImDrawVert);
                Buffer.MemoryCopy((void*)cmdlList.VtxBuffer.Data, vertexResourcePointer, vertBytes, vertBytes);

                var idxBytes = cmdlList.IdxBuffer.Size * sizeof(ImDrawIdx);
                Buffer.MemoryCopy((void*)cmdlList.IdxBuffer.Data, indexResourcePointer, idxBytes, idxBytes);

                vertexResourcePointer += cmdlList.VtxBuffer.Size;
                indexResourcePointer += cmdlList.IdxBuffer.Size;
            }
            ctx.Unmap(vertexBuffer, 0);
            ctx.Unmap(indexBuffer, 0);

            // Setup orthographic projection matrix into our constant buffer
            // Our visible imgui space lies from draw_data.DisplayPos (top left) to draw_data.DisplayPos+data_data.DisplaySize (bottom right). DisplayPos is (0,0) for single viewport apps.

            var constResource = ctx.Map(constantBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var span = constResource.AsSpan<byte>(VertexConstantBufferSize);
            ImGuiIOPtr io = ImGui.GetIO();
            Matrix4x4 mvp = MathUtil.OrthoOffCenterLH(0f, io.DisplaySize.X, io.DisplaySize.Y, 0, -1, 1);
            MemoryMarshal.Write(span, ref mvp);
            ctx.Unmap(constantBuffer, 0);

            SetupRenderState(data, ctx);

            data.ScaleClipRects(io.DisplayFramebufferScale);

            // Render command lists
            // (Because we merged all buffers into a single one, we maintain our own offset into them)
            int vtx_offset = 0;
            int idx_offset = 0;

            for (int n = 0; n < data.CmdListsCount; n++)
            {
                var cmdList = data.CmdListsRange[n];
                ImGuizmo.SetDrawlist(cmdList);

                for (int i = 0; i < cmdList.CmdBuffer.Size; i++)
                {
                    var cmd = cmdList.CmdBuffer[i];
                    if (cmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException("user callbacks not implemented");
                    }
                    else
                    {
                        ctx.SetScissorRect((int)cmd.ClipRect.X, (int)cmd.ClipRect.Y, (int)cmd.ClipRect.Z, (int)cmd.ClipRect.W);

                        textureResources.TryGetValue(cmd.TextureId, out var texture);
                        if (texture != null)
                            ctx.SetShaderResource(texture, ShaderStage.Pixel, 0);

                        ctx.DrawIndexed((int)cmd.ElemCount, idx_offset, vtx_offset);
                    }
                    idx_offset += (int)cmd.ElemCount;
                }
                vtx_offset += cmdList.VtxBuffer.Size;
            }

            ctx.ClearState();
        }

        public void Dispose()
        {
            if (device == null)
                return;

            InvalidateDeviceObjects();
        }

        private void SetupRenderState(ImDrawDataPtr drawData, IGraphicsContext ctx)
        {
            var viewport = new Viewport(drawData.DisplaySize.X, drawData.DisplaySize.Y);
            ctx.SetViewport(viewport);

            int stride = sizeof(ImDrawVert);
            int offset = 0;
            ctx.SetInputLayout(inputLayout);
            ctx.SetVertexBuffer(vertexBuffer, stride, offset);
            ctx.SetIndexBuffer(indexBuffer, sizeof(ushort) == 2 ? Format.R16UInt : Format.R32UInt, 0);
            ctx.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            ctx.VSSetShader(vertexShader);
            ctx.SetConstantBuffer(constantBuffer, ShaderStage.Vertex, 0);
            ctx.PSSetShader(pixelShader);
            ctx.SetSampler(fontSampler, ShaderStage.Pixel, 0);
            ctx.GSSetShader(null);
            ctx.HSSetShader(null);
            ctx.DSSetShader(null);
            ctx.CSSetShader(null);

            ctx.SetBlendState(blendState);
            ctx.SetDepthStencilState(depthStencilState);
            ctx.SetRasterizerState(rasterizerState);
        }

        private void CreateFontsTexture()
        {
            var io = ImGui.GetIO();
            byte* pixels;
            int width, height;
            io.Fonts.GetTexDataAsRGBA32(out pixels, out width, out height);

            var texDesc = new Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.RGBA8UNorm,
                SampleDescription = new SampleDescription { Count = 1 },
                Usage = Usage.Default,
                BindFlags = BindFlags.ShaderResource,
                CPUAccessFlags = CpuAccessFlags.None
            };

            var subResource = new SubresourceData
            {
                DataPointer = (IntPtr)pixels,
                RowPitch = texDesc.Width * 4,
                SlicePitch = 0
            };

            var texture = device.CreateTexture2D(texDesc, new[] { subResource });

            var resViewDesc = new ShaderResourceViewDescription
            {
                Format = Format.RGBA8UNorm,
                ViewDimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new Texture2DShaderResourceView { MipLevels = texDesc.MipLevels, MostDetailedMip = 0 }
            };
            fontTextureView = device.CreateShaderResourceView(texture, resViewDesc);
            texture.Dispose();

            io.Fonts.TexID = RegisterTexture(fontTextureView);

            var samplerDesc = new SamplerDescription
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
        }

        public static IntPtr RegisterTexture(IShaderResourceView texture)
        {
            var imguiID = texture.NativePointer;
            textureResources.Add(imguiID, texture);
            return imguiID;
        }

        public static IntPtr TryRegisterTexture(IShaderResourceView texture)
        {
            var imguiID = texture.NativePointer;
            if (!textureResources.ContainsKey(imguiID))
                textureResources.Add(imguiID, texture);
            return imguiID;
        }

        public static void UnregisterTexture(IShaderResourceView texture)
        {
            textureResources.Remove(texture.NativePointer);
        }

        private void CreateDeviceObjects()
        {
            var vertexShaderCode =
                @"
                    cbuffer vertexBuffer : register(b0)
                    {
                        float4x4 ProjectionMatrix;
                    };

                    struct VS_INPUT
                    {
                        float2 pos : POSITION;
                        float2 uv  : TEXCOORD0;
                        float4 col : COLOR0;
                    };

                    struct PS_INPUT
                    {
                        float4 pos : SV_POSITION;
                        float4 col : COLOR0;
                        float2 uv  : TEXCOORD0;
                    };

                    PS_INPUT main(VS_INPUT input)
                    {
                        PS_INPUT output;
                        output.pos = mul(ProjectionMatrix, float4(input.pos.xy, 0.f, 1.f));
                        output.col = input.col;
                        output.uv  = input.uv;
                        return output;
                    }";

            device.Compile(vertexShaderCode, "main", "vs", "vs_4_0", out vertexShaderBlob, out var errorBlob);
            if (vertexShaderBlob == null)
                throw new Exception("error compiling vertex shader");

            vertexShader = device.CreateVertexShader(vertexShaderBlob.AsBytes());

            var inputElements = new[]
            {
                new InputElementDescription( "POSITION", 0, Format.RG32Float,   0, 0, InputClassification.PerVertexData, 0 ),
                new InputElementDescription( "TEXCOORD", 0, Format.RG32Float,   8,  0, InputClassification.PerVertexData, 0 ),
                new InputElementDescription( "COLOR",    0, Format.RGBA8UNorm, 16, 0, InputClassification.PerVertexData, 0 ),
            };

            inputLayout = device.CreateInputLayout(inputElements, vertexShaderBlob);

            var constBufferDesc = new BufferDescription
            {
                ByteWidth = VertexConstantBufferSize,
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = CpuAccessFlags.Write,
            };
            constantBuffer = device.CreateBuffer(constBufferDesc);

            var pixelShaderCode =
                @"struct PS_INPUT
                    {
                        float4 pos : SV_POSITION;
                        float4 col : COLOR0;
                        float2 uv  : TEXCOORD0;
                    };

                    sampler sampler0;
                    Texture2D texture0;

                    float4 main(PS_INPUT input) : SV_Target
                    {
                        float4 out_col = input.col * texture0.Sample(sampler0, input.uv);
                        return out_col;
                    }";

            device.Compile(pixelShaderCode, "main", "ps", "ps_4_0", out pixelShaderBlob, out errorBlob);
            if (pixelShaderBlob == null)
                throw new Exception("error compiling pixel shader");

            pixelShader = device.CreatePixelShader(pixelShaderBlob.AsBytes());

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

            blendState = device.CreateBlendState(blendDesc);

            var rasterDesc = new RasterizerDescription
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                ScissorEnable = true,
                DepthClipEnable = false,
            };

            rasterizerState = device.CreateRasterizerState(rasterDesc);

            var stencilOpDesc = new DepthStencilOperationDescription(StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep, ComparisonFunction.Never);
            var depthDesc = new DepthStencilDescription
            {
                DepthEnable = false,
                DepthWriteMask = DepthWriteMask.Zero,
                DepthFunc = ComparisonFunction.Always,
                StencilEnable = false,
                FrontFace = stencilOpDesc,
                BackFace = stencilOpDesc
            };

            depthStencilState = device.CreateDepthStencilState(depthDesc);

            CreateFontsTexture();
        }

        private void InvalidateDeviceObjects()
        {
            fontSampler.Dispose();
            fontTextureView.Dispose();
            indexBuffer.Dispose();
            vertexBuffer.Dispose();
            blendState.Dispose();
            depthStencilState.Dispose();
            rasterizerState.Dispose();
            pixelShader.Dispose();
            pixelShaderBlob.Dispose();
            constantBuffer.Dispose();
            inputLayout.Dispose();
            vertexShader.Dispose();
            vertexShaderBlob.Dispose();
        }
    }
}