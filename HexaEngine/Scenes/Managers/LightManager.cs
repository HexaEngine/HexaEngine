namespace HexaEngine.Scenes.Managers
{
    using HexaEngine.Cameras;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Buffers;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Pipelines.Forward;
    using HexaEngine.Rendering.ConstantBuffers;
    using HexaEngine.Resources;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using Texture = Graphics.Texture;

    public struct DirectionalLightData
    {
        public Vector4 Color;
        public Vector3 Direction;

        public DirectionalLightData(DirectionalLight light)
        {
            Color = light.Color;
            Direction = light.Transform.Forward;
        }

        public void Update(DirectionalLight light)
        {
            Color = light.Color;
            Direction = light.Transform.Forward;
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }

    public struct PointLightData
    {
        public Vector4 Color;
        public Vector3 Position;

        public PointLightData(PointLight point) : this()
        {
            Color = point.Color * point.Strength;
            Position = point.Transform.GlobalPosition;
        }

        public void Update(PointLight point)
        {
            Color = point.Color * point.Strength;
            Position = point.Transform.GlobalPosition;
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }

    public struct SpotlightData
    {
        public Vector4 Color;
        public Vector3 Position;
        public float CutOff;
        public Vector3 Direction;
        public float OuterCutOff;

        public SpotlightData(Spotlight spotlight)
        {
            Color = spotlight.Color * spotlight.Strength;
            Position = spotlight.Transform.GlobalPosition;
            CutOff = MathF.Cos((spotlight.ConeAngle / 2).ToRad());
            Direction = spotlight.Transform.Forward;
            OuterCutOff = MathF.Cos((MathUtil.Lerp(0, spotlight.ConeAngle, 1 - spotlight.Blend) / 2).ToRad());
        }

        public void Update(Spotlight spotlight)
        {
            Color = spotlight.Color * spotlight.Strength;
            Position = spotlight.Transform.GlobalPosition;
            CutOff = MathF.Cos((spotlight.ConeAngle / 2).ToRad());
            Direction = spotlight.Transform.Forward;
            OuterCutOff = MathF.Cos((MathUtil.Lerp(0, spotlight.ConeAngle, 1 - spotlight.Blend) / 2).ToRad());
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }

    public struct ShadowDirectionalLightData
    {
        public static readonly unsafe int CascadePointerOffset = sizeof(Matrix4x4) * 16;

        public Matrix4x4 View0;
        public Matrix4x4 View1;
        public Matrix4x4 View2;
        public Matrix4x4 View3;
        public Matrix4x4 View4;
        public Matrix4x4 View5;
        public Matrix4x4 View6;
        public Matrix4x4 View7;
        public Matrix4x4 View8;
        public Matrix4x4 View9;
        public Matrix4x4 View10;
        public Matrix4x4 View11;
        public Matrix4x4 View12;
        public Matrix4x4 View13;
        public Matrix4x4 View14;
        public Matrix4x4 View15;

        public float Cascade0;
        public float Cascade1;
        public float Cascade2;
        public float Cascade3;
        public float Cascade4;
        public float Cascade5;
        public float Cascade6;
        public float Cascade7;
        public float Cascade8;
        public float Cascade9;
        public float Cascade10;
        public float Cascade11;
        public float Cascade12;
        public float Cascade13;
        public float Cascade14;
        public float Cascade15;

        public Vector4 Color;
        public Vector3 Direction;

        public ShadowDirectionalLightData(DirectionalLight light)
        {
            Color = light.Color;
            Direction = light.Transform.Forward;
        }

        public void Update(DirectionalLight light)
        {
            Color = light.Color;
            Direction = light.Transform.Forward;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Matrix4x4* GetViews()
        {
            fixed (ShadowDirectionalLightData* @this = &this)
            {
                return (Matrix4x4*)@this;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe float* GetCascades()
        {
            fixed (ShadowDirectionalLightData* @this = &this)
            {
                return (float*)((byte*)@this + CascadePointerOffset);
            }
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }

    public struct ShadowPointLightData
    {
        public Vector4 Color;
        public Vector3 Position;
        public float Far;

        public ShadowPointLightData(PointLight point) : this()
        {
            Color = point.Color * point.Strength;
            Position = point.Transform.GlobalPosition;
            Far = point.ShadowRange;
        }

        public void Update(PointLight point)
        {
            Color = point.Color * point.Strength;
            Position = point.Transform.GlobalPosition;
            Far = point.ShadowRange;
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }

    public struct ShadowSpotlightData
    {
        public Matrix4x4 View;
        public Vector4 Color;
        public Vector3 Position;
        public float CutOff;
        public Vector3 Direction;
        public float OuterCutOff;

        public ShadowSpotlightData(Spotlight spotlight)
        {
            View = PSMHelper.GetLightSpaceMatrix(spotlight.Transform, spotlight.ConeAngle.ToRad());
            Color = spotlight.Color * spotlight.Strength;
            Position = spotlight.Transform.GlobalPosition;
            CutOff = MathF.Cos((spotlight.ConeAngle / 2).ToRad());
            Direction = spotlight.Transform.Forward;
            OuterCutOff = MathF.Cos((MathUtil.Lerp(0, spotlight.ConeAngle, 1 - spotlight.Blend) / 2).ToRad());
        }

        public void Update(Spotlight spotlight)
        {
            View = PSMHelper.GetLightSpaceMatrix(spotlight.Transform, spotlight.ConeAngle.ToRad());
            Color = spotlight.Color * spotlight.Strength;
            Position = spotlight.Transform.GlobalPosition;
            CutOff = MathF.Cos((spotlight.ConeAngle / 2).ToRad());
            Direction = spotlight.Transform.Forward;
            OuterCutOff = MathF.Cos((MathUtil.Lerp(0, spotlight.ConeAngle, 1 - spotlight.Blend) / 2).ToRad());
        }

        public override string ToString()
        {
            return Color.ToString();
        }
    }

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

                switch (light.Type)
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
        private unsafe void DrawScene(IGraphicsContext context, GraphicsPipeline pipeline, Viewport viewport, BoundingBox box)
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
        private unsafe void DrawScene(IGraphicsContext context, GraphicsPipeline pipeline, Viewport viewport, BoundingFrustum frustum)
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