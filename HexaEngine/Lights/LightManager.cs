namespace HexaEngine.Lights
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Lights.Probes;
    using HexaEngine.Lights.Structs;
    using HexaEngine.Lights.Types;
    using HexaEngine.Rendering;
    using HexaEngine.Scenes;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public partial class LightManager : ISystem
    {
        private readonly List<ILightProbeComponent> probes = new();
        private readonly List<Light> lights = new();
        private readonly List<Light> activeLights = new();

        private readonly ConcurrentQueue<ILightProbeComponent> probeUpdateQueue = new();
        private readonly ConcurrentQueue<Light> lightUpdateQueue = new();
        public readonly ConcurrentQueue<IRendererComponent> RendererUpdateQueue = new();

        public StructuredUavBuffer<GlobalProbeData> GlobalProbes;
        public StructuredUavBuffer<LightData> LightBuffer;
        public StructuredUavBuffer<ShadowData> ShadowDataBuffer;

        public const int MaxGlobalLightProbes = 1;
        public const int MaxDirectionalLightSDs = 1;
        public const int MaxPointLightSDs = 32;
        public const int MaxSpotlightSDs = 32;

        public LightManager()
        {
        }

        public static LightManager? Current => SceneManager.Current?.LightManager;

        public IReadOnlyList<ILightProbeComponent> Probes => probes;

        public IReadOnlyList<Light> Lights => lights;

        public IReadOnlyList<Light> Active => activeLights;

        public int Count => lights.Count;

        public int ActiveCount => activeLights.Count;

        public string Name => "Lights";

        public SystemFlags Flags { get; } = SystemFlags.None;

        public async Task Initialize(IGraphicsDevice device)
        {
            GlobalProbes = new(device, CpuAccessFlags.Write);
            LightBuffer = new(device, CpuAccessFlags.Write);
            ShadowDataBuffer = new(device, CpuAccessFlags.Write);
        }

        private void LightTransformed(GameObject gameObject)
        {
            if (gameObject is Light light)
            {
                lightUpdateQueue.Enqueue(light);
            }
        }

        private void LightPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is Light light)
            {
                lightUpdateQueue.Enqueue(light);
            }
        }

        public void Clear()
        {
            lock (lights)
            {
                lights.Clear();
            }
        }

        public unsafe void Register(GameObject gameObject)
        {
            if (gameObject is Light light)
            {
                AddLight(light);
            }
            if (gameObject.TryGetComponent<ILightProbeComponent>(out var component))
            {
                AddProbe(component);
            }
        }

        public unsafe void Unregister(GameObject gameObject)
        {
            if (gameObject is Light light)
            {
                light.DestroyShadowMap();
                RemoveLight(light);
            }
            if (gameObject.TryGetComponent<ILightProbeComponent>(out var component))
            {
                RemoveProbe(component);
            }
        }

        public unsafe void AddLight(Light light)
        {
            lock (lights)
            {
                lights.Add(light);
                lightUpdateQueue.Enqueue(light);
                light.Transformed += LightTransformed;
                light.PropertyChanged += LightPropertyChanged;
            }
        }

        public unsafe void AddProbe(ILightProbeComponent probe)
        {
            lock (probes)
            {
                probes.Add(probe);
                probeUpdateQueue.Enqueue(probe);
            }
        }

        public unsafe void RemoveLight(Light light)
        {
            lock (lights)
            {
                light.PropertyChanged -= LightPropertyChanged;
                light.Transformed -= LightTransformed;
                lights.Remove(light);
                activeLights.Remove(light);
            }
        }

        public unsafe void RemoveProbe(ILightProbeComponent probe)
        {
            lock (probes)
            {
                probes.Remove(probe);
            }
        }

        public readonly Queue<Light> UpdateShadowLightQueue = new();

        public unsafe void Update(IGraphicsContext context, ShadowAtlas shadowAtlas, Camera camera)
        {
            while (lightUpdateQueue.TryDequeue(out var light))
            {
                if (light.IsEnabled)
                {
                    if (!activeLights.Contains(light))
                    {
                        activeLights.Add(light);
                    }

                    if (light.ShadowMapEnable)
                    {
                        light.CreateShadowMap(context.Device, shadowAtlas);
                    }
                    else
                    {
                        light.DestroyShadowMap();
                    }

                    if (!light.InUpdateQueue)
                    {
                        light.InUpdateQueue = true;
                        UpdateShadowLightQueue.Enqueue(light);
                    }
                }
                else
                {
                    activeLights.Remove(light);
                }
            }

            UpdateLights(context, camera);

            while (RendererUpdateQueue.TryDequeue(out var renderer))
            {
                for (int i = 0; i < activeLights.Count; i++)
                {
                    var light = activeLights[i];
                    if (!light.ShadowMapEnable || light.ShadowMapUpdateMode != ShadowUpdateMode.OnDemand)
                    {
                        continue;
                    }

                    if (light.IntersectFrustum(renderer.BoundingBox) && !light.InUpdateQueue)
                    {
                        light.InUpdateQueue = true;
                        UpdateShadowLightQueue.Enqueue(light);
                    }
                }
            }

            for (int i = 0; i < activeLights.Count; i++)
            {
                var light = activeLights[i];
                if (light.ShadowMapEnable && light is DirectionalLight && !light.InUpdateQueue || light.ShadowMapEnable && light.ShadowMapUpdateMode == ShadowUpdateMode.EveryFrame)
                {
                    light.InUpdateQueue = true;
                    UpdateShadowLightQueue.Enqueue(light);
                }
                light.ComputeImportance(camera, shadowAtlas);
            }
        }

        private unsafe void UpdateLights(IGraphicsContext context, Camera camera)
        {
            GlobalProbes.ResetCounter();
            LightBuffer.ResetCounter();
            LightBuffer.Clear(context);
            ShadowDataBuffer.ResetCounter();
            uint globalProbesCount = 0;
            uint csmCount = 0;

            for (int i = 0; i < probes.Count; i++)
            {
                var probe = probes[i];
                if (!(probe.IsEnabled && probe.IsVaild))
                {
                    continue;
                }

                switch (probe.Type)
                {
                    case ProbeType.Global:
                        if (globalProbesCount == MaxGlobalLightProbes)
                        {
                            continue;
                        }
                        GlobalProbes.Add((GlobalLightProbeComponent)probe);
                        //forwardSrvs[nForwardIndirectSrvsBase + globalProbesCount] = forwardClusterdSrvs[nForwardClusterdIndirectSrvsBase + globalProbesCount] = indirectSrvs[nIndirectSrvsBase + globalProbesCount] = (void*)(probe.DiffuseTex?.SRV?.NativePointer ?? 0);
                        //forwardSrvs[nForwardIndirectSrvsBase + MaxGlobalLightProbes + globalProbesCount] = forwardClusterdSrvs[nForwardClusterdIndirectSrvsBase + MaxGlobalLightProbes + globalProbesCount] = indirectSrvs[nIndirectSrvsBase + MaxGlobalLightProbes + globalProbesCount] = (void*)(probe.SpecularTex?.SRV?.NativePointer ?? 0);
                        globalProbesCount++;
                        break;

                    case ProbeType.Local:
                        break;
                }
            }

            for (uint i = globalProbesCount; i < MaxGlobalLightProbes; i++)
            {
                //forwardSrvs[nForwardIndirectSrvsBase + i] = forwardClusterdSrvs[nForwardClusterdIndirectSrvsBase + i] = indirectSrvs[nIndirectSrvsBase + i] = null;
                //forwardSrvs[nForwardIndirectSrvsBase + MaxGlobalLightProbes + i] = forwardClusterdSrvs[nForwardClusterdIndirectSrvsBase + MaxGlobalLightProbes + i] = indirectSrvs[nIndirectSrvsBase + MaxGlobalLightProbes + i] = null;
            }

            for (int i = 0; i < activeLights.Count; i++)
            {
                var light = activeLights[i];

                if (light.ShadowMapEnable)
                {
                    switch (light.LightType)
                    {
                        case LightType.Directional:
                            if (csmCount == MaxDirectionalLightSDs)
                            {
                                continue;
                            }

                            light.QueueIndex = ShadowDataBuffer.Count;
                            LightBuffer.Add(new((DirectionalLight)light));
                            ShadowDataBuffer.Add(new((DirectionalLight)light, DirectionalLight.ShadowMapSize));
                            //forwardSrvs[14] = forwardClusterdSrvs[16] = deferredSrvs[9] = deferredClusterdSrvs[11] = (void*)light.GetShadowMap()?.NativePointer;
                            ((DirectionalLight)light).UpdateShadowBuffer(ShadowDataBuffer, camera);
                            csmCount++;
                            break;

                        case LightType.Point:
                            light.QueueIndex = ShadowDataBuffer.Count;
                            LightBuffer.Add(new((PointLight)light));
                            ShadowDataBuffer.Add(new((PointLight)light, PointLight.ShadowMapSize));
                            ((PointLight)light).UpdateShadowBuffer(ShadowDataBuffer);
                            break;

                        case LightType.Spot:
                            light.QueueIndex = ShadowDataBuffer.Count;
                            LightBuffer.Add(new((Spotlight)light));
                            ShadowDataBuffer.Add(new((Spotlight)light, Spotlight.ShadowMapSize));
                            ((Spotlight)light).UpdateShadowBuffer(ShadowDataBuffer);
                            break;
                    }
                }
                else
                {
                    switch (light.LightType)
                    {
                        case LightType.Directional:
                            light.QueueIndex = LightBuffer.Count;
                            LightBuffer.Add(new((DirectionalLight)light));
                            break;

                        case LightType.Point:
                            light.QueueIndex = LightBuffer.Count;
                            LightBuffer.Add(new((PointLight)light));
                            break;

                        case LightType.Spot:
                            light.QueueIndex = LightBuffer.Count;
                            LightBuffer.Add(new((Spotlight)light));
                            break;
                    }
                }
            }
        }

        public unsafe void BakePass(IGraphicsContext context, IRendererComponent renderer, Camera camera)
        {
            if ((renderer.Flags & RendererFlags.Bake) == 0)
            {
                return;
            }
            if ((renderer.Flags & RendererFlags.Clustered) != 0)
            {
                BakeClustered(context, renderer, camera);
            }
            else
            {
                Bake(context, renderer, camera);
            }
        }

        private unsafe void Bake(IGraphicsContext context, IRendererComponent renderer, Camera camera)
        {
            /*
            var lightParams = forwardLightParamsBuffer.Local;
            lightParams->LightCount = LightBuffer.Count;
            lightParams->GlobalProbes = GlobalProbes.Count;
            forwardLightParamsBuffer.Update(context);
            cbs[0] = (void*)forwardLightParamsBuffer.Buffer?.NativePointer;

            if ((renderer.Flags & RendererFlags.NoDepthTest) != 0)
            {
                context.SetRenderTargets(nForwardRtvs, forwardRtvs, null);
            }
            else
            {
                context.SetRenderTargets(nForwardRtvs, forwardRtvs, DSV.Value);
            }

            context.SetViewport(Output.Viewport);
            context.VSSetConstantBuffers(1, 1, &cbs[1]);
            context.DSSetConstantBuffers(1, 1, &cbs[1]);
            context.CSSetConstantBuffers(1, 1, &cbs[1]);
            context.PSSetConstantBuffers(0, nConstantBuffers, cbs);
            context.PSSetShaderResources(8, nForwardSrvs, forwardSrvs);
            context.PSSetSamplers(8, nSamplers, smps);

            renderer.Draw(context, RenderPath.Forward);

            nint* null_samplers = stackalloc nint[(int)nSamplers];
            context.PSSetSamplers(8, nSamplers, (void**)null_samplers);

            nint* null_srvs = stackalloc nint[(int)nForwardSrvs];
            context.PSSetShaderResources(8, nForwardSrvs, (void**)null_srvs);

            nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
            context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);
            */
        }

        private unsafe void BakeClustered(IGraphicsContext context, IRendererComponent renderer, Camera camera)
        {
            /*
            var lightParams = forwardLightParamsBuffer.Local;
            lightParams->LightCount = LightBuffer.Count;
            lightParams->GlobalProbes = GlobalProbes.Count;
            forwardLightParamsBuffer.Update(context);
            cbs[0] = (void*)forwardLightParamsBuffer.Buffer?.NativePointer;

            if ((renderer.Flags & RendererFlags.NoDepthTest) != 0)
            {
                context.SetRenderTargets(nForwardRtvs, forwardRtvs, null);
            }
            else
            {
                context.SetRenderTargets(nForwardRtvs, forwardRtvs, DSV.Value);
            }

            context.SetViewport(Output.Viewport);
            context.VSSetConstantBuffers(1, 1, &cbs[1]);
            context.DSSetConstantBuffers(1, 1, &cbs[1]);
            context.CSSetConstantBuffers(1, 1, &cbs[1]);
            context.PSSetConstantBuffers(0, nConstantBuffers, cbs);
            context.PSSetShaderResources(0, nForwardClusterdSrvs, forwardClusterdSrvs);
            context.PSSetSamplers(8, nSamplers, smps);

            renderer.Bake(context);

            nint* null_samplers = stackalloc nint[(int)nSamplers];
            context.PSSetSamplers(8, nSamplers, (void**)null_samplers);

            nint* null_srvs = stackalloc nint[(int)nForwardClusterdSrvs];
            context.PSSetShaderResources(8, nForwardClusterdSrvs, (void**)null_srvs);

            nint* null_cbs = stackalloc nint[(int)nConstantBuffers];
            context.PSSetConstantBuffers(0, nConstantBuffers, (void**)null_cbs);
            */
        }

        public unsafe void Dispose()
        {
            GlobalProbes.Dispose();
            LightBuffer.Dispose();
            ShadowDataBuffer.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Awake()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Update(float dt)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void FixedUpdate()
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Destroy()
        {
        }
    }
}