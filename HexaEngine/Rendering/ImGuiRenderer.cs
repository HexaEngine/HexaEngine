//based on https://github.com/ocornut/imgui/blob/master/examples/imgui_impl_dx11.cpp
#nullable disable

using HexaEngine.Core;
using HexaEngine.Core.Graphics;
using HexaEngine.Editor;
using HexaEngine.IO;
using HexaEngine.Mathematics;
using ImGuiNET;
using ImGuizmoNET;
using ImNodesNET;
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
        private nint test;
        private IBuffer vertexBuffer;
        private IBuffer indexBuffer;
        private IVertexShader vertexShader;
        private IInputLayout inputLayout;
        private IBuffer constantBuffer;
        private IPixelShader pixelShader;
        private ISamplerState fontSampler;
        private IShaderResourceView fontTextureView;
        private IRasterizerState rasterizerState;
        private IBlendState blendState;
        private IDepthStencilState depthStencilState;
        private int vertexBufferSize = 5000, indexBufferSize = 10000;

        public ImGuiRenderer(SdlWindow window, IGraphicsDevice device)
        {
            IntPtr igContext = ImGui.CreateContext();
            ImGui.SetCurrentContext(igContext);
            ImGuizmo.SetImGuiContext(igContext);

            ImNodes.SetImGuiContext(igContext);
            ImNodes.Initialize();

            this.device = device;
            context = device.Context;
            swapChain = device.SwapChain;

            var io = ImGui.GetIO();
            var config = new ImFontConfigPtr(ImGuiNative.ImFontConfig_ImFontConfig());

            io.Fonts.AddFontDefault(config);
            config.MergeMode = true;
            config.GlyphMinAdvanceX = 18;
            config.GlyphOffset = new(0, 4);
            var range = new char[] { (char)0xE700, (char)0xF800, (char)0 };
            fixed (char* buffer = range)
            {
                var bytes = FileSystem.ReadAllBytes("assets/fonts/SEGMDL2.TTF");
                fixed (byte* buffer2 = bytes)
                {
                    io.Fonts.AddFontFromMemoryTTF((nint)buffer2, bytes.Length, 14, config, (IntPtr)buffer);
                }
            }

            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset | ImGuiBackendFlags.HasMouseCursors;
            ImGui.StyleColorsDark();
            CreateDeviceObjects();

            var colors = ImGui.GetStyle().Colors;
            colors[(int)ImGuiCol.FrameBg] = new Vector4(0.48f, 0.16f, 0.44f, 0.54f);
            colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.98f, 0.26f, 0.95f, 0.40f);
            colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.86f, 0.26f, 0.98f, 0.67f);
            colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.45f, 0.16f, 0.48f, 1.00f);
            colors[(int)ImGuiCol.CheckMark] = new Vector4(0.94f, 0.26f, 0.98f, 1.00f);
            colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.80f, 0.24f, 0.88f, 1.00f);
            colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.96f, 0.26f, 0.98f, 1.00f);
            colors[(int)ImGuiCol.Button] = new Vector4(0.98f, 0.26f, 0.95f, 0.40f);
            colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.96f, 0.26f, 0.98f, 1.00f);
            colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.89f, 0.06f, 0.98f, 1.00f);
            colors[(int)ImGuiCol.Header] = new Vector4(0.92f, 0.26f, 0.98f, 0.31f);
            colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.92f, 0.26f, 0.98f, 0.80f);
            colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.92f, 0.26f, 0.98f, 1.00f);
            colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.70f, 0.10f, 0.75f, 0.78f);
            colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.70f, 0.10f, 0.75f, 1.00f);
            colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.92f, 0.26f, 0.98f, 0.20f);
            colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.92f, 0.26f, 0.98f, 0.67f);
            colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(0.92f, 0.26f, 0.98f, 0.95f);
            colors[(int)ImGuiCol.Tab] = new Vector4(0.55f, 0.18f, 0.58f, 0.86f);
            colors[(int)ImGuiCol.TabHovered] = new Vector4(0.92f, 0.26f, 0.98f, 0.80f);
            colors[(int)ImGuiCol.TabActive] = new Vector4(0.64f, 0.20f, 0.68f, 1.00f);
            colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.14f, 0.07f, 0.15f, 0.97f);
            colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.40f, 0.14f, 0.42f, 1.00f);
            colors[(int)ImGuiCol.DockingPreview] = new Vector4(0.92f, 0.26f, 0.98f, 0.70f);
            colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.92f, 0.26f, 0.98f, 0.35f);
            colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.92f, 0.26f, 0.98f, 1.00f);
            colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.98f, 0.56f, 0.95f, 0.40f);
            colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.96f, 0.46f, 0.98f, 1.00f);
            var bg = colors[(int)ImGuiCol.WindowBg];
            ImNodes.StyleColorsDark();
            var ncolors = ImNodes.GetStyle()->colors;
            ncolors[(int)ColorStyle.NodeBackground] = new Vector4(0.06f, 0.06f, 0.06f, 0.94f).Pack();
            ncolors[(int)ColorStyle.NodeBackgroundHovered] = new Vector4(0.12f, 0.12f, 0.12f, 0.94f).Pack();
            ncolors[(int)ColorStyle.NodeBackgroundSelected] = new Vector4(0.16f, 0.16f, 0.16f, 0.94f).Pack();
            ncolors[(int)ColorStyle.TitleBar] = new Vector4(0.92f, 0.26f, 0.98f, 0.31f).Pack();
            ncolors[(int)ColorStyle.TitleBarHovered] = new Vector4(0.92f, 0.26f, 0.98f, 0.80f).Pack();
            ncolors[(int)ColorStyle.TitleBarSelected] = new Vector4(0.70f, 0.10f, 0.75f, 1.00f).Pack();
            ncolors[(int)ColorStyle.Link] = new Vector4(0.92f, 0.26f, 0.98f, 0.31f).Pack();
            ncolors[(int)ColorStyle.LinkHovered] = new Vector4(0.92f, 0.26f, 0.98f, 0.80f).Pack();
            ncolors[(int)ColorStyle.LinkSelected] = new Vector4(0.70f, 0.10f, 0.75f, 1.00f).Pack();
            ncolors[(int)ColorStyle.Pin] = new Vector4(0.92f, 0.26f, 0.98f, 0.31f).Pack();
            ncolors[(int)ColorStyle.PinHovered] = new Vector4(0.92f, 0.26f, 0.98f, 0.80f).Pack();

            inputHandler = new(window);
        }

        public void BeginDraw()
        {
            inputHandler.Update();
            ImGui.NewFrame();
            ImGuizmo.BeginFrame();
            ImGui.PushStyleColor(ImGuiCol.WindowBg, Vector4.Zero);
            ImGui.DockSpaceOverViewport(null, ImGuiDockNodeFlags.PassthruCentralNode);
            ImGui.PopStyleColor();
        }

        public void EndDraw()
        {
            ImGui.Render();
            ImGui.EndFrame();
            context.SetRenderTarget(swapChain.BackbufferRTV, null);
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

                        ctx.PSSetShaderResource((void*)cmd.TextureId, 0);

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
            ImGui.SaveIniSettingsToDisk("imgui.ini");
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
            ctx.VSSetConstantBuffer(constantBuffer, 0);
            ctx.PSSetShader(pixelShader);
            ctx.PSSetSampler(fontSampler, 0);
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
            io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height);

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

            io.Fonts.TexID = fontTextureView.NativePointer;

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

        private void CreateDeviceObjects()
        {
            if (!ShaderCache.GetShader("Internal:ImGui:VS", Array.Empty<ShaderMacro>(), out var vBytes))
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

                device.Compile(vertexShaderCode, "main", "vs", "vs_5_0", out var vertexShaderBlob, out var errorBlob);
                if (vertexShaderBlob == null)
                    throw new Exception("error compiling vertex shader");

                ShaderCache.CacheShader("Internal:ImGui:VS", Array.Empty<ShaderMacro>(), vertexShaderBlob);
                vBytes = vertexShaderBlob.AsSpan();
                vertexShaderBlob.Dispose();
            }

            vertexShader = device.CreateVertexShader(vBytes);

            var inputElements = new[]
            {
                new InputElementDescription( "POSITION", 0, Format.RG32Float,   0, 0, InputClassification.PerVertexData, 0 ),
                new InputElementDescription( "TEXCOORD", 0, Format.RG32Float,   8,  0, InputClassification.PerVertexData, 0 ),
                new InputElementDescription( "COLOR",    0, Format.RGBA8UNorm, 16, 0, InputClassification.PerVertexData, 0 ),
            };

            inputLayout = device.CreateInputLayout(inputElements, vBytes.ToArray());

            var constBufferDesc = new BufferDescription
            {
                ByteWidth = VertexConstantBufferSize,
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = CpuAccessFlags.Write,
            };
            constantBuffer = device.CreateBuffer(constBufferDesc);
            if (!ShaderCache.GetShader("Internal:ImGui:PS", Array.Empty<ShaderMacro>(), out var pBytes))
            {
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

                device.Compile(pixelShaderCode, "main", "ps", "ps_5_0", out var pixelShaderBlob, out var errorBlob);
                if (pixelShaderBlob == null)
                    throw new Exception("error compiling pixel shader");

                ShaderCache.CacheShader("Internal:ImGui:PS", Array.Empty<ShaderMacro>(), pixelShaderBlob);
                pBytes = pixelShaderBlob.AsSpan();
                pixelShaderBlob.Dispose();
            }

            pixelShader = device.CreatePixelShader(pBytes);

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
            constantBuffer.Dispose();
            inputLayout.Dispose();
            vertexShader.Dispose();
        }
    }
}