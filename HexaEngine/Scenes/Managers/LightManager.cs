using HexaEngine.Core.Graphics;

namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using HexaEngine.Pipelines.Forward;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using Texture = Texture;

    public class LightManager
    {
        private readonly List<Light> lights = new();
        private readonly List<Light> activeLights = new();
        private readonly IInstanceManager instanceManager;
        private readonly ConstantBuffer<CBLight> cbLights;

        private readonly StructuredUavBuffer<DirectionalLightData> directionalLights;
        private readonly StructuredUavBuffer<PointLightData> pointLights;
        private readonly StructuredUavBuffer<SpotlightData> spotlights;

        private readonly StructuredUavBuffer<ShadowDirectionalLightData> shadowDirectionalLights;
        private readonly StructuredUavBuffer<ShadowPointLightData> shadowPointLights;
        private readonly StructuredUavBuffer<ShadowSpotlightData> shadowSpotlights;

        private CSMPipeline csmPipeline;
        private Texture csmDepthBuffer;
        private ConstantBuffer<Matrix4x4> csmMvpBuffer;

        private OSMPipeline osmPipeline;
        private ConstantBuffer<Matrix4x4> osmBuffer;
        private IBuffer osmParamBuffer;
        private Texture[] osmDepthBuffers;
        private IShaderResourceView[] osmSRVs;

        private PSMPipeline psmPipeline;
        private IBuffer psmBuffer;
        private Texture[] psmDepthBuffers;
        private IShaderResourceView[] psmSRVs;

        public LightManager(IGraphicsDevice device, IInstanceManager instanceManager)
        {
            this.instanceManager = instanceManager;
            cbLights = new(device, CpuAccessFlags.Write);
        }

        public IReadOnlyList<Light> Lights => lights;

        public IReadOnlyList<Light> Active => activeLights;

        public int Count => lights.Count;

        public int ActiveCount => activeLights.Count;

        public void Clear()
        {
            lock (lights)
            {
                lights.Clear();
            }
        }

        public void AddLight(Light light)
        {
            lock (lights)
            {
                lights.Add(light);
            }
        }

        public void RemoveLight(Light light)
        {
            lock (lights)
            {
                lights.Remove(light);
            }
        }

        public unsafe void Update(IGraphicsContext context, Camera camera)
        {
            var lights = cbLights.Local;
            CBLight.Update(lights, activeLights);
            UpdateShadowMaps(context, camera, lights);
        }

        public unsafe void DeferredPass(IGraphicsContext context)
        {
        }

        public unsafe void UpdateShadowMaps(IGraphicsContext context, Camera camera, CBLight* lights)
        {
            uint directsd = 0;
            uint pointsd = 0;
            uint spotsd = 0;

            // Draw light depth
            for (int i = 0; i < activeLights.Count; i++)
            {
                Light light = activeLights[i];

                switch (light.LightType)
                {
                    case LightType.Directional:
                        if (!light.CastShadows || !light.Updated)
                        {
                            if (light.CastShadows)
                                directsd++;
                            continue;
                        }
                        light.Updated = false;
                        UpdateDirectionalLight(context, directsd, camera, (DirectionalLight)light, lights);
                        directsd++;
                        break;

                    case LightType.Point:
                        if (!light.CastShadows || !light.Updated)
                        {
                            if (light.CastShadows)
                                pointsd++;
                            continue;
                        }
                        light.Updated = false;
                        UpdatePointLight(context, pointsd, (PointLight)light);
                        pointsd++;
                        break;

                    case LightType.Spot:
                        if (!light.CastShadows || !light.Updated)
                        {
                            if (light.CastShadows)
                                spotsd++;
                            continue;
                        }
                        light.Updated = false;
                        UpdateSpotlight(context, spotsd, (Spotlight)light, lights);
                        spotsd++;
                        break;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void UpdateDirectionalLight(IGraphicsContext context, uint index, Camera camera, DirectionalLight light, CBLight* lights)
        {
            CBDirectionalLightSD* directionalLight = lights->GetDirectionalLightSDs();
            Matrix4x4* views = directionalLight->GetViews();
            float* cascades = directionalLight->GetCascades();
            var mtxs = CSMHelper.GetLightSpaceMatrices(camera.Transform, light.Transform, views, cascades);
            context.Write(csmMvpBuffer.Buffer, mtxs, sizeof(Matrix4x4) * 16);

            csmDepthBuffer.ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);
            context.SetRenderTarget(csmDepthBuffer.RenderTargetView, csmDepthBuffer.DepthStencilView);
            DrawScene(context, csmPipeline, csmDepthBuffer.Viewport, light.Transform.Frustum);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void UpdatePointLight(IGraphicsContext context, uint index, PointLight light)
        {
            OSMHelper.GetLightSpaceMatrices(light.Transform, light.ShadowRange, osmBuffer.Local, light.ShadowBox);
            osmBuffer.Update(context);
            context.Write(osmParamBuffer, new Vector4(light.Transform.GlobalPosition, light.ShadowRange));

            osmDepthBuffers[index].ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);
            context.SetRenderTarget(osmDepthBuffers[index].RenderTargetView, osmDepthBuffers[index].DepthStencilView);
            DrawScene(context, osmPipeline, osmDepthBuffers[index].Viewport, *light.ShadowBox);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void UpdateSpotlight(IGraphicsContext context, uint index, Spotlight light, CBLight* lights)
        {
            CBSpotlightSD* spotlights = lights->GetSpotlightSDs();
            context.Write(psmBuffer, spotlights[index].View);

            psmDepthBuffers[index].ClearTarget(context, Vector4.Zero, DepthStencilClearFlags.All);
            context.SetRenderTarget(psmDepthBuffers[index].RenderTargetView, psmDepthBuffers[index].DepthStencilView);
            DrawScene(context, psmPipeline, psmDepthBuffers[index].Viewport, light.Transform.Frustum);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void DrawScene(IGraphicsContext context, IGraphicsPipeline pipeline, Viewport viewport, BoundingBox box)
        {
            pipeline.BeginDraw(context, viewport);
            for (int j = 0; j < instanceManager.TypeCount; j++)
            {
                instanceManager.Types[j].UpdateFrustumInstanceBuffer(box);
                instanceManager.Types[j].DrawNoOcclusion(context);
            }
            context.ClearState();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void DrawScene(IGraphicsContext context, IGraphicsPipeline pipeline, Viewport viewport, BoundingFrustum frustum)
        {
            pipeline.BeginDraw(context, viewport);
            for (int j = 0; j < instanceManager.TypeCount; j++)
            {
                instanceManager.Types[j].UpdateFrustumInstanceBuffer(frustum);
                instanceManager.Types[j].DrawNoOcclusion(context);
            }
            context.ClearState();
        }
    }
}