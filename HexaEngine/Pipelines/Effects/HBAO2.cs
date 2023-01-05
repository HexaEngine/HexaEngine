namespace HexaEngine.Pipelines.Effects
{
    using HexaEngine.Cameras;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Objects.Primitives;
    using HexaEngine.Rendering.ConstantBuffers;
    using HexaEngine.Resources;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public class HBAO2 : IEffect
    {
        private IGraphicsDevice device;
        private int width;
        private int height;
        private Quad quad;
        private GraphicsPipeline hbaoPipeline;
        private IBuffer cbCamera;
        private IBuffer cbHBAO;
        private HBAOCamera hbaoCamera = new();
        private HBAOParams hbaoParams = new();
        private ITexture2D hbaoBuffer;
        private unsafe void** hbaoSRVs;
        private unsafe void** hbaoCBs;
        private IShaderResourceView hbaoSRV;
        private IRenderTargetView hbaoRTV;

        private ISamplerState samplerLinear;

        public IRenderTargetView? Output;
        public IBuffer? Camera;
        public IShaderResourceView? Depth;
        public IShaderResourceView? Normal;

        private bool isDirty = true;
        private bool disposedValue;
        private bool enabled;

        #region Structs

        private struct HBAOCamera
        {
            public Matrix4x4 ProjectionMatrix;
            public Vector4 ViewFrustumVectors1;
            public Vector4 ViewFrustumVectors2;
            public Vector4 ViewFrustumVectors3;
            public Vector4 ViewFrustumVectors4;
            public Vector2 RenderTargetResolution;

            public HBAOCamera(Camera camera, Vector2 res)
            {
                ProjectionMatrix = Matrix4x4.Transpose(camera.Transform.Projection);
                ViewFrustumVectors1 = new(camera.Transform.Frustum.Corners[0] / camera.Transform.Frustum.Corners[0].Z, 1);
                ViewFrustumVectors2 = new(camera.Transform.Frustum.Corners[1] / camera.Transform.Frustum.Corners[1].Z, 1);
                ViewFrustumVectors3 = new(camera.Transform.Frustum.Corners[2] / camera.Transform.Frustum.Corners[2].Z, 1);
                ViewFrustumVectors4 = new(camera.Transform.Frustum.Corners[3] / camera.Transform.Frustum.Corners[3].Z, 1);
                RenderTargetResolution = res;
            }
        }

        private struct HBAOParams
        {
            public Vector2 SampleDirections1;
            public Vector2 SampleDirections2;
            public Vector2 SampleDirections3;
            public Vector2 SampleDirections4;
            public Vector2 SampleDirections5;
            public Vector2 SampleDirections6;
            public Vector2 SampleDirections7;
            public Vector2 SampleDirections8;
            public Vector2 SampleDirections9;
            public Vector2 SampleDirections10;
            public Vector2 SampleDirections11;
            public Vector2 SampleDirections12;
            public Vector2 SampleDirections13;
            public Vector2 SampleDirections14;
            public Vector2 SampleDirections15;
            public Vector2 SampleDirections16;
            public Vector2 SampleDirections17;
            public Vector2 SampleDirections18;
            public Vector2 SampleDirections19;
            public Vector2 SampleDirections20;
            public Vector2 SampleDirections21;
            public Vector2 SampleDirections22;
            public Vector2 SampleDirections23;
            public Vector2 SampleDirections24;
            public Vector2 SampleDirections25;
            public Vector2 SampleDirections26;
            public Vector2 SampleDirections27;
            public Vector2 SampleDirections28;
            public Vector2 SampleDirections29;
            public Vector2 SampleDirections30;
            public Vector2 SampleDirections31;
            public Vector2 SampleDirections32;
            public float StrengthPerRay = 0.1875f;  // strength / numRays
            public uint NumRays = 8;
            public uint MaxStepsPerRay = 5;
            public float HalfSampleRadius = .25f;   // sampleRadius / 2
            public float FallOff = 2.0f;            // the maximum distance to count samples
            public float DitherScale;              // the ratio between the render target size and the dither texture size. Normally: renderTargetResolution / 4
            public float Bias = .03f;				// minimum factor to start counting occluders

            public HBAOParams()
            {
                StrengthPerRay = 0.1875f;
                NumRays = 8;
                MaxStepsPerRay = 5;
                HalfSampleRadius = .25f;
                FallOff = 2.0f;
                Bias = .03f;
            }
        }

        private struct BlurParams
        {
            public float Sharpness;
            public Vector2 InvResolutionDirection;
            public float padd;

            public BlurParams()
            {
                Sharpness = 3;
            }
        }

        #endregion Structs

        #region Properties

        public bool Enabled { get => enabled; set => enabled = value; }

        #endregion Properties

        public async Task Initialize(IGraphicsDevice device, int width, int height)
        {
            this.device = device;
            this.width = width;
            this.height = height;
            Output = ResourceManager.AddTextureRTV("SSAOBuffer", TextureDescription.CreateTexture2DWithRTV(width / 2, height / 2, 1, Format.R32Float));

            quad = new Quad(device);

            hbaoPipeline = new(device, new()
            {
                VertexShader = "effects/hbao2/vs.hlsl",
                PixelShader = "effects/hbao2/ps.hlsl",
            });
            cbCamera = device.CreateBuffer(hbaoCamera, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            cbHBAO = device.CreateBuffer(hbaoParams, BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            hbaoBuffer = device.CreateTexture2D(Format.RG32Float, width / 2, height / 2, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceMiscFlag.None);
            hbaoRTV = device.CreateRenderTargetView(hbaoBuffer, new(width / 2, height / 2));
            hbaoSRV = device.CreateShaderResourceView(hbaoBuffer);

            Depth = await ResourceManager.GetResourceAsync<IShaderResourceView>("SwapChain.SRV");
            Normal = await ResourceManager.GetResourceAsync<IShaderResourceView>("GBuffer.Normal");
            Camera = await ResourceManager.GetConstantBufferAsync("CBCamera");

            unsafe
            {
                hbaoSRVs = AllocArray(2);
                hbaoSRVs[0] = (void*)Depth.NativePointer;
                hbaoSRVs[1] = (void*)Normal.NativePointer;
                hbaoCBs = AllocArray(2);
                hbaoCBs[0] = (void*)cbCamera.NativePointer;
                hbaoCBs[1] = (void*)cbHBAO.NativePointer;
            }

            samplerLinear = device.CreateSamplerState(SamplerDescription.LinearClamp);
        }

        public void BeginResize()
        {
            ResourceManager.RequireUpdate("SSAOBuffer");
        }

        public async void EndResize(int width, int height)
        {
            this.width = width;
            this.height = height;
            Output = ResourceManager.UpdateTextureRTV("SSAOBuffer", TextureDescription.CreateTexture2DWithRTV(width / 2, height / 2, 1, Format.R32Float));

            hbaoBuffer.Dispose();
            hbaoRTV.Dispose();
            hbaoSRV.Dispose();
            hbaoBuffer = device.CreateTexture2D(Format.RG32Float, width / 2, height / 2, 1, 1, null, BindFlags.ShaderResource | BindFlags.RenderTarget, ResourceMiscFlag.None);
            hbaoRTV = device.CreateRenderTargetView(hbaoBuffer, new(width / 2, height / 2));
            hbaoSRV = device.CreateShaderResourceView(hbaoBuffer);

            hbaoParams.DitherScale = 0.5f;

            Depth = await ResourceManager.GetTextureSRVAsync("SwapChain.SRV");
            Normal = await ResourceManager.GetTextureSRVAsync("GBuffer.Normal");
            Camera = await ResourceManager.GetConstantBufferAsync("CBCamera");

            unsafe
            {
                hbaoSRVs[0] = (void*)Depth.NativePointer;
                hbaoSRVs[1] = (void*)Normal.NativePointer;

                hbaoCBs[1] = (void*)Camera.NativePointer;
            }

            isDirty = true;
        }

        public unsafe void Draw(IGraphicsContext context)
        {
            if (Output is null) return;
            if (isDirty)
            {
                context.Write(cbHBAO, hbaoParams);
                isDirty = false;
            }

            context.Write(cbCamera, new HBAOCamera(CameraManager.Current, new(width, height)));
            context.SetRenderTarget(Output, null);
            context.VSSetConstantBuffer(cbCamera, 0);
            context.PSSetShaderResources(hbaoSRVs, 3, 0);
            context.PSSetConstantBuffers(hbaoCBs, 2, 0);
            context.PSSetSampler(samplerLinear, 0);
            quad.DrawAuto(context, hbaoPipeline, Output.Viewport);
        }

        protected virtual unsafe void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Free(hbaoCBs);

                quad.Dispose();
                hbaoPipeline.Dispose();
                cbHBAO.Dispose();
                cbCamera.Dispose();
                hbaoBuffer.Dispose();
                hbaoRTV.Dispose();
                hbaoSRV.Dispose();

                samplerLinear.Dispose();
                disposedValue = true;
            }
        }

        ~HBAO2()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}