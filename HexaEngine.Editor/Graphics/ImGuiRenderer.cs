//based on https://github.com/ocornut/imgui/blob/master/examples/imgui_impl_dx11.cpp

namespace HexaEngine.Editor.Graphics
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Mathematics;
    using Hexa.NET.SDL3;
    using Hexa.NET.Utilities;
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using ImDrawIdx = UInt16;

    public static class ImGuiRenderer
    {
        private static readonly SRVWrapper srvWrapper = new(0);

        private class RendererData
        {
            public IGraphicsDevice device = null!;
            public IGraphicsContext context = null!;
            public IGraphicsPipelineState pso = null!;
            public IBuffer vertexBuffer = null!;
            public IBuffer indexBuffer = null!;
            public IBuffer constantBuffer = null!;
            public ISamplerState fontSampler = null!;
            public int vertexBufferSize = 5000, indexBufferSize = 10000;
        }

        public class ImTexture
        {
            public ITexture2D Texture = null!;
            public IShaderResourceView TextureView = null!;
        }

        // Backend data stored in io.BackendRendererUserData to allow support for multiple Dear ImGui contexts
        // It is STRONGLY preferred that you use docking branch with multi-viewports (== single Dear ImGui context + multiple windows) instead of multiple Dear ImGui contexts.
        private static unsafe RendererData? GetBackendData()
        {
            if (ImGui.GetCurrentContext().IsNull)
            {
                return null;
            }
            var handle = GCHandle.FromIntPtr((nint)ImGui.GetIO().BackendRendererUserData);
            return (RendererData?)handle.Target;
        }

        private static unsafe RendererData AllocateRenderData(ImGuiIO* io)
        {
            RendererData bd = new();
            GCHandle handle = GCHandle.Alloc(bd, GCHandleType.Normal);
            io->BackendRendererUserData = (void*)(nint)handle;
            return bd;
        }

        private static unsafe void FreeRendererData(ImGuiIO* io)
        {
            if (io->BackendRendererUserData == null)
            {
                return;
            }
            GCHandle handle = GCHandle.FromIntPtr((nint)io->BackendRendererUserData);
            handle.Free();
            io->BackendRendererUserData = null;
        }

        private static unsafe void SetupRenderState(ImDrawData* drawData, IGraphicsContext ctx)
        {
            var bd = GetBackendData()!;
            Viewport viewport = new(drawData->DisplaySize.X, drawData->DisplaySize.Y);

            uint stride = (uint)sizeof(ImDrawVert);
            uint offset = 0;

            ctx.SetGraphicsPipelineState(bd.pso);
            ctx.SetViewport(viewport);
            ctx.SetVertexBuffer(bd.vertexBuffer, stride, offset);
            ctx.SetIndexBuffer(bd.indexBuffer, sizeof(ushort) == 2 ? Format.R16UInt : Format.R32UInt, 0);
            ctx.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
        }

        public static readonly Dictionary<ImTextureID, ISamplerState> Samplers = new();

        /// <summary>
        /// Render function
        /// </summary>
        /// <param name="data"></param>
        public static unsafe void RenderDrawData(ImDrawData* data)
        {
            // Avoid rendering when minimized
            if (data->DisplaySize.X <= 0.0f || data->DisplaySize.Y <= 0.0f)
            {
                return;
            }

            if (data->CmdListsCount == 0)
            {
                return;
            }

            var bd = GetBackendData()!;
            var ctx = bd.context;

#if DEBUG
            ctx.BeginEvent("ImGuiRenderer");
#endif

            if (data->Textures != null)
            {
                var textures = data->Textures->Data;
                var textureCount = data->Textures->Size;
                for (int i = 0; i < textureCount; i++)
                {
                    ImTextureData* tex = textures[i];
                    if (tex->Status != ImTextureStatus.Ok)
                    {
                        UpdateTexture(tex);
                    }
                }
            }

            // Create and grow vertex/index buffers if needed
            if (bd.vertexBuffer == null || bd.vertexBufferSize < data->TotalVtxCount)
            {
                bd.vertexBuffer?.Dispose();
                bd.vertexBufferSize = data->TotalVtxCount + 5000;
                BufferDescription desc = new();
                desc.Usage = Usage.Dynamic;
                desc.ByteWidth = bd.vertexBufferSize * sizeof(ImDrawVert);
                desc.BindFlags = BindFlags.VertexBuffer;
                desc.CPUAccessFlags = CpuAccessFlags.Write;
                bd.vertexBuffer = bd.device.CreateBuffer(desc);
            }

            if (bd.indexBuffer == null || bd.indexBufferSize < data->TotalIdxCount)
            {
                bd.indexBuffer?.Dispose();
                bd.indexBufferSize = data->TotalIdxCount + 10000;
                BufferDescription desc = new();
                desc.Usage = Usage.Dynamic;
                desc.ByteWidth = bd.indexBufferSize * sizeof(ImDrawIdx);
                desc.BindFlags = BindFlags.IndexBuffer;
                desc.CPUAccessFlags = CpuAccessFlags.Write;
                bd.indexBuffer = bd.device.CreateBuffer(desc);
            }

            // Upload vertex/index data into a single contiguous GPU buffer
            var vertexResource = ctx.Map(bd.vertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var indexResource = ctx.Map(bd.indexBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
            var vertexResourcePointer = (ImDrawVert*)vertexResource.PData;
            var indexResourcePointer = (ImDrawIdx*)indexResource.PData;
            for (int n = 0; n < data->CmdListsCount; n++)
            {
                var cmdlList = data->CmdLists.Data[n];

                var vertBytes = cmdlList.VtxBuffer.Size * sizeof(ImDrawVert);
                Buffer.MemoryCopy(cmdlList.VtxBuffer.Data, vertexResourcePointer, vertBytes, vertBytes);

                var idxBytes = cmdlList.IdxBuffer.Size * sizeof(ImDrawIdx);
                Buffer.MemoryCopy(cmdlList.IdxBuffer.Data, indexResourcePointer, idxBytes, idxBytes);

                vertexResourcePointer += cmdlList.VtxBuffer.Size;
                indexResourcePointer += cmdlList.IdxBuffer.Size;
            }
            ctx.Unmap(bd.vertexBuffer, 0);
            ctx.Unmap(bd.indexBuffer, 0);

            // Setup orthographic projection matrix into our constant buffer
            // Our visible imgui space lies from draw_data->DisplayPos (top left) to draw_data->DisplayPos+data_data->DisplaySize (bottom right). DisplayPos is (0,0) for single viewport apps.
            {
                var mappedResource = ctx.Map(bd.constantBuffer, 0, MapMode.WriteDiscard, MapFlags.None);
                Matrix4x4* constant_buffer = (Matrix4x4*)mappedResource.PData;

                float L = data->DisplayPos.X;
                float R = data->DisplayPos.X + data->DisplaySize.X;
                float T = data->DisplayPos.Y;
                float B = data->DisplayPos.Y + data->DisplaySize.Y;
                Matrix4x4 mvp = new
                    (
                     2.0f / (R - L), 0.0f, 0.0f, 0.0f,
                     0.0f, 2.0f / (T - B), 0.0f, 0.0f,
                     0.0f, 0.0f, 0.5f, 0.0f,
                     (R + L) / (L - R), (T + B) / (B - T), 0.5f, 1.0f
                     );
                Buffer.MemoryCopy(&mvp, constant_buffer, sizeof(Matrix4x4), sizeof(Matrix4x4));
                ctx.Unmap(bd.constantBuffer, 0);
            }

            // Setup desired state
            SetupRenderState(data, ctx);

            // Render command lists
            // (Because we merged all buffers into a single one, we maintain our own offset into them)
            int global_idx_offset = 0;
            int global_vtx_offset = 0;
            Vector2 clip_off = data->DisplayPos;
            for (int n = 0; n < data->CmdListsCount; n++)
            {
                var cmdList = data->CmdLists.Data[n];

                for (int i = 0; i < cmdList.CmdBuffer.Size; i++)
                {
                    var cmd = cmdList.CmdBuffer.Data[i];
                    if (cmd.UserCallback != null)
                    {
                        // User callback, registered via ImDrawList::AddCallback()
                        // (ImDrawCallback_ResetRenderState is a special callback value used by the user to request the renderer to reset render state.)
                        if ((nint)cmd.UserCallback == -1)
                        {
                            SetupRenderState(data, ctx);
                        }
                        else
                        {
                            delegate*<ImDrawList*, ImDrawCmd*, void> callback = (delegate*<ImDrawList*, ImDrawCmd*, void>)cmd.UserCallback;
                            callback(cmdList, &cmd);
                        }
                    }
                    else
                    {
                        // Project scissor/clipping rectangles into framebuffer space
                        Vector2 clip_min = new(cmd.ClipRect.X - clip_off.X, cmd.ClipRect.Y - clip_off.Y);
                        Vector2 clip_max = new(cmd.ClipRect.Z - clip_off.X, cmd.ClipRect.W - clip_off.Y);
                        if (clip_max.X <= clip_min.X || clip_max.Y <= clip_min.Y)
                        {
                            continue;
                        }

                        // Apply scissor/clipping rectangle
                        ctx.SetScissorRect((int)clip_min.X, (int)clip_min.Y, (int)clip_max.X, (int)clip_max.Y);

                        // Bind texture, Draw
                        var texId = cmd.TexRef.GetTexID();
                        srvWrapper.NativePointer = (nint)texId.Handle;
                        bd.pso.Bindings.SetSRV("fontTex", srvWrapper);
                        if (Samplers.TryGetValue(texId, out var sampler))
                        {
                            bd.pso.Bindings.SetSampler("fontSampler", sampler);
                        }
                        else
                        {
                            bd.pso.Bindings.SetSampler("fontSampler", bd.fontSampler);
                        }

                        ctx.SetGraphicsPipelineState(bd.pso);
                        ctx.DrawIndexedInstanced(cmd.ElemCount, 1, (uint)(cmd.IdxOffset + global_idx_offset), (int)(cmd.VtxOffset + global_vtx_offset), 0);
                    }
                }
                global_idx_offset += cmdList.IdxBuffer.Size;
                global_vtx_offset += cmdList.VtxBuffer.Size;
            }

            ctx.SetGraphicsPipelineState(null);
            ctx.SetViewport(default);
            ctx.SetVertexBuffer(null, 0, 0);
            ctx.SetIndexBuffer(null, default, 0);
            ctx.SetPrimitiveTopology(PrimitiveTopology.Undefined);

#if DEBUG
            ctx.EndEvent();
#endif
        }

        private static readonly ImTextureID ImTextureID_Invalid = new(0);

        private static unsafe ImTexture? GetImTexture(ImTextureData* tex)
        {
            if (tex->BackendUserData == null)
            {
                return null;
            }
            return (ImTexture?)GCHandle.FromIntPtr((nint)tex->BackendUserData).Target;
        }

        private static unsafe void DestroyTexture(ImTextureData* tex)
        {
            var backendTex = GetImTexture(tex);
            if (backendTex != null)
            {
                backendTex.TextureView.Dispose();
                backendTex.Texture.Dispose();
                GCHandle.FromIntPtr((nint)tex->BackendUserData).Free();
                tex->SetTexID(ImTextureID_Invalid);
                tex->BackendUserData = null;
            }

            tex->SetStatus(ImTextureStatus.Destroyed);
        }

        private static unsafe void UpdateTexture(ImTextureData* tex)
        {
            var bd = GetBackendData()!;
            if (tex->Status == ImTextureStatus.WantCreate)
            {
                // Create and upload new texture to graphics system
                //IMGUI_DEBUG_LOG("UpdateTexture #%03d: WantCreate %dx%d\n", tex->UniqueID, tex->Width, tex->Height);
                Debug.Assert(tex->TexID == ImTextureID_Invalid && tex->BackendUserData == null);
                Debug.Assert(tex->Format == ImTextureFormat.Rgba32);
                uint* pixels = (uint*)tex->GetPixels();
                ImTexture backendTex = new();
                GCHandle handle = GCHandle.Alloc(backendTex, GCHandleType.Normal);

                // Create texture
                Texture2DDescription desc = default;
                desc.Width = tex->Width;
                desc.Height = tex->Height;
                desc.MipLevels = 1;
                desc.ArraySize = 1;
                desc.Format = Format.R8G8B8A8UNorm;
                desc.SampleDescription.Count = 1;
                desc.Usage = Usage.Default;
                desc.BindFlags = BindFlags.ShaderResource;
                desc.CPUAccessFlags = 0;
                SubresourceData subResource;
                subResource.DataPointer = (nint)pixels;
                subResource.RowPitch = desc.Width * 4;
                subResource.SlicePitch = 0;
                backendTex.Texture = bd.device.CreateTexture2D(desc, [subResource]);

                // Create texture view
                ShaderResourceViewDescription srvDesc = default;
                srvDesc.Format = Format.R8G8B8A8UNorm;
                srvDesc.ViewDimension = ShaderResourceViewDimension.Texture2D;
                srvDesc.Texture2D.MipLevels = desc.MipLevels;
                srvDesc.Texture2D.MostDetailedMip = 0;
                backendTex.TextureView = bd.device.CreateShaderResourceView(backendTex.Texture, srvDesc);

                // Store identifiers
                tex->SetTexID((ImTextureID)backendTex.TextureView.NativePointer);
                tex->SetStatus(ImTextureStatus.Ok);
                tex->BackendUserData = (void*)(nint)handle;
            }
            else if (tex->Status == ImTextureStatus.WantUpdates)
            {
                // Update selected blocks. We only ever write to textures regions which have never been used before!
                // This backend choose to use tex->Updates[] but you can use tex->UpdateRect to upload a single region.
                ImTexture backendTex = GetImTexture(tex)!;

                Debug.Assert(backendTex.TextureView.NativePointer == tex->TexID);
                var rects = tex->Updates.Data;
                var rectCount = tex->Updates.Size;
                for (int i = 0; i < rectCount; ++i)
                {
                    var r = rects[i];
                    Box box = new() { Left = r.X, Top = r.Y, Right = (uint)(r.X + r.W), Bottom = (uint)(r.Y + r.H), Front = 0, Back = 1 };
                    MappedSubresource mappedResource = new(tex->GetPixelsAt(r.X, r.Y), (uint)tex->GetPitch(), 0);
                    bd.context.UpdateSubresource(backendTex.Texture, 0, box, mappedResource);
                }
                tex->SetStatus(ImTextureStatus.Ok);
            }
            if (tex->Status == ImTextureStatus.WantDestroy && tex->UnusedFrames > 0)
            {
                DestroyTexture(tex);
            }
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

        private static unsafe void CreateDeviceObjects()
        {
            var bd = GetBackendData()!;
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
                ScissorEnable = true,
                DepthClipEnable = false,
            };

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

            var inputElements = new[]
            {
                new InputElementDescription( "POSITION", 0, Format.R32G32Float,   0, 0, InputClassification.PerVertexData, 0 ),
                new InputElementDescription( "TEXCOORD", 0, Format.R32G32Float,   8,  0, InputClassification.PerVertexData, 0 ),
                new InputElementDescription( "COLOR",    0, Format.R8G8B8A8UNorm, 16, 0, InputClassification.PerVertexData, 0 ),
            };

            bd.pso = bd.device.CreateGraphicsPipelineState(new GraphicsPipelineDesc()
            {
                VertexShader = "HexaEngine.Core:shaders/internal/imgui/vs.hlsl",
                PixelShader = "HexaEngine.Core:shaders/internal/imgui/ps.hlsl",
            }, new()
            {
                Blend = blendDesc,
                DepthStencil = depthDesc,
                Rasterizer = rasterDesc,
                InputElements = inputElements
            });

            var constBufferDesc = new BufferDescription
            {
                ByteWidth = sizeof(Matrix4x4),
                Usage = Usage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
                CPUAccessFlags = CpuAccessFlags.Write,
            };
            bd.constantBuffer = bd.device.CreateBuffer(constBufferDesc);
            bd.pso.Bindings.SetCBV("matrixBuffer", bd.constantBuffer);

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
            bd.fontSampler = bd.device.CreateSamplerState(samplerDesc);

            bd.pso.Bindings.SetSampler("fontSampler", bd.fontSampler);
        }

        private unsafe static void InvalidateDeviceObjects()
        {
            var bd = GetBackendData()!;
            var textures = ImGui.GetPlatformIO().Textures;
            for (int i = 0; i < textures.Size; i++)
            {
                ImTextureData* tex = textures.Data[i];
                if (tex->RefCount == 1)
                {
                    DestroyTexture(tex);
                }
            }

            bd.pso.Dispose();
            bd.fontSampler.Dispose();
            bd.indexBuffer.Dispose();
            bd.vertexBuffer.Dispose();
            bd.constantBuffer.Dispose();
        }

        public static unsafe bool Init(IGraphicsDevice device, IGraphicsContext context)
        {
            var io = ImGui.GetIO();
            Trace.Assert(io.BackendRendererUserData == null, "Already initialized a renderer backend!");

            // Setup backend capabilities flags
            var bd = AllocateRenderData(io);
            io.BackendRendererName = "ImGui_Generic_Renderer".ToUTF8Ptr();
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset; // We can honor the ImDrawCmd::VtxOffset field, allowing for large meshes.
            io.BackendFlags |= ImGuiBackendFlags.RendererHasTextures;   // We can honor ImGuiPlatformIO::Textures[] requests during render.
            io.BackendFlags |= ImGuiBackendFlags.RendererHasViewports; // We can create multi-viewports on the Renderer side (optional)

            var platformIo = ImGui.GetPlatformIO();
            platformIo.RendererTextureMaxHeight = platformIo.RendererTextureMaxWidth = 16384;

            bd.device = device;
            bd.context = context;

            CreateDeviceObjects();

            if ((io.ConfigFlags & ImGuiConfigFlags.ViewportsEnable) != 0)
            {
                InitPlatformInterface();
            }

            return true;
        }

        public static unsafe void Shutdown()
        {
            var bd = GetBackendData();
            Trace.Assert(bd != null, "No renderer backend to shutdown, or already shutdown?");
            var io = ImGui.GetIO();
            var platformIo = ImGui.GetPlatformIO();

            ShutdownPlatformInterface();
            InvalidateDeviceObjects();

            io.BackendRendererName = null;
            io.BackendRendererUserData = null;
            io.BackendFlags &= ~(ImGuiBackendFlags.RendererHasVtxOffset | ImGuiBackendFlags.RendererHasTextures | ImGuiBackendFlags.RendererHasViewports);
            //platformIo.ClearRendererHandlers();
            FreeRendererData(io);
        }

        public static unsafe void NewFrame()
        {
            var bd = GetBackendData();
            Trace.Assert(bd != null, "Did you call Init()?");

            if (bd.fontSampler == null)
            {
                CreateDeviceObjects();
            }
        }

        //--------------------------------------------------------------------------------------------------------
        // MULTI-VIEWPORT / PLATFORM INTERFACE SUPPORT
        // This is an _advanced_ and _optional_ feature, allowing the backend to create and handle multiple viewports simultaneously.
        // If you are new to dear imgui or creating a new binding for dear imgui, it is recommended that you completely ignore this section first..
        //--------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Helper structure we store in the void* RendererUserData field of each ImGuiViewport to easily retrieve our backend data.
        /// </summary>
        private class ViewportData
        {
            public ISwapChain SwapChain = null!;
            public IRenderTargetView RTView = null!;
        };

        private static unsafe ViewportData AllocateViewportData(ImGuiViewport* vp)
        {
            ViewportData vd = new();
            GCHandle handle = GCHandle.Alloc(vd, GCHandleType.Normal);
            vp->RendererUserData = (void*)(nint)handle;
            return vd;
        }

        private static unsafe ViewportData? GetViewportData(ImGuiViewport* vp)
        {
            if (vp->RendererUserData == null)
            {
                return null;
            }
            return (ViewportData?)GCHandle.FromIntPtr((nint)vp->RendererUserData).Target;
        }

        private static unsafe void FreeViewportData(ImGuiViewport* vp)
        {
            if (vp->RendererUserData == null)
            {
                return;
            }
            GCHandle handle = GCHandle.FromIntPtr((nint)vp->RendererUserData);
            handle.Free();
            vp->RendererUserData = null;
        }

        private static unsafe void CreateWindow(ImGuiViewport* viewport)
        {
            var bd = GetBackendData()!;
            var vd = AllocateViewportData(viewport);

            // PlatformHandleRaw should always be a HWND, whereas PlatformHandle might be a higher-level handle (e.g. GLFWWindow*, SDL_Window*).
            // Some backends will leave PlatformHandleRaw == 0, in which case we assume PlatformHandle will contain the HWND.
            SDLWindow* window = SDL.GetWindowFromID((uint)viewport->PlatformHandle);
            int w, h;
            SDL.GetWindowSize(window, &w, &h);

            SwapChainDescription description = new()
            {
                BufferCount = 1,
                Format = Format.R8G8B8A8UNorm,
                Width = (uint)w,
                Height = (uint)h,
                SampleDesc = SampleDescription.Default,
                Scaling = Scaling.None,
                Stereo = false,
                SwapEffect = SwapEffect.FlipSequential,
                AlphaMode = SwapChainAlphaMode.Unspecified,
                Flags = SwapChainFlags.None,
            };

            // Create swap chain
            vd.SwapChain = bd.device.CreateSwapChain(window);

            // Create the render target
            if (vd.SwapChain != null)
            {
                vd.RTView = vd.SwapChain.BackbufferRTV;
            }
        }

        private static unsafe void DestroyWindow(ImGuiViewport* viewport)
        {
            // The main viewport (owned by the application) will always have RendererUserData == nullptr since we didn't create the data for it.
            var vd = GetViewportData(viewport);
            if (vd != null)
            {
                vd.SwapChain?.Dispose();
                vd.SwapChain = null!;
                vd.RTView = null!;
                GCHandle handle = GCHandle.FromIntPtr((nint)viewport->RendererUserData);
                handle.Free();
            }
            viewport->RendererUserData = null;
        }

        private static unsafe void SetWindowSize(ImGuiViewport* viewport, Vector2 size)
        {
            var vd = GetViewportData(viewport)!;

            vd.RTView = null!;

            if (vd.SwapChain != null)
            {
                vd.SwapChain.Resize((int)size.X, (int)size.Y);
                vd.SwapChain.Active = true;
                vd.SwapChain.VSync = false;
                vd.SwapChain.LimitFPS = false;
                vd.RTView = vd.SwapChain.BackbufferRTV;
            }
        }

        private static unsafe void RenderWindow(ImGuiViewport* viewport, void* userdata)
        {
            var bd = GetBackendData()!;
            var vd = GetViewportData(viewport)!;
            bd.context.SetRenderTarget(vd.RTView, null);
            if ((viewport->Flags & ImGuiViewportFlags.NoRendererClear) != 0)
            {
                bd.context.ClearRenderTargetView(vd.RTView, new Vector4(0.0f, 0.0f, 0.0f, 1.0f));
            }

            RenderDrawData(viewport->DrawData);
        }

        private static unsafe void SwapBuffers(ImGuiViewport* viewport, void* userdata)
        {
            var vd = GetViewportData(viewport)!;
            vd.SwapChain.Present(false); // Present without vsync
        }

        private static unsafe void InitPlatformInterface()
        {
            ImGuiPlatformIOPtr platformIo = ImGui.GetPlatformIO();
            platformIo.RendererCreateWindow = (void*)Marshal.GetFunctionPointerForDelegate<RendererCreateWindow>(CreateWindow);
            platformIo.RendererDestroyWindow = (void*)Marshal.GetFunctionPointerForDelegate<RendererDestroyWindow>(DestroyWindow);
            platformIo.RendererSetWindowSize = (void*)Marshal.GetFunctionPointerForDelegate<RendererSetWindowSize>(SetWindowSize);
            platformIo.RendererRenderWindow = (void*)Marshal.GetFunctionPointerForDelegate<RendererRenderWindow>(RenderWindow);
            platformIo.RendererSwapBuffers = (void*)Marshal.GetFunctionPointerForDelegate<RendererSwapBuffers>(SwapBuffers);
        }

        private static unsafe void ShutdownPlatformInterface()
        {
            ImGui.DestroyPlatformWindows();
        }
    }
}