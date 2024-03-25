namespace HexaEngine.Lights
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics;
    using HexaEngine.Lights.Structs;
    using HexaEngine.Lights.Types;
    using HexaEngine.Scenes;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public partial class LightManager : ISceneSystem
    {
        private readonly IGraphicsDevice device;

        private readonly List<Probe> probes = [];
        private readonly List<Light> lights = [];
        private readonly List<Light> activeLights = [];

        private readonly ConcurrentQueue<Probe> probeUpdateQueue = new();
        private readonly ConcurrentQueue<Light> lightUpdateQueue = new();
        public readonly ConcurrentQueue<IRendererComponent> RendererUpdateQueue = new();

        public readonly StructuredUavBuffer<ProbeData> GlobalProbes;
        public readonly StructuredUavBuffer<LightData> LightBuffer;
        public readonly StructuredUavBuffer<ShadowData> ShadowDataBuffer;

        public LightManager(IGraphicsDevice device)
        {
            this.device = device;
            GlobalProbes = new(device, CpuAccessFlags.Write);
            LightBuffer = new(device, CpuAccessFlags.Write);
            ShadowDataBuffer = new(device, CpuAccessFlags.Write);
        }

        public static LightManager? Current => SceneManager.Current?.LightManager;

        public IReadOnlyList<Probe> Probes => probes;

        public IReadOnlyList<Light> Lights => lights;

        public IReadOnlyList<Light> Active => activeLights;

        public int Count => lights.Count;

        public int ActiveCount => activeLights.Count;

        public string Name => "Lights";

        public SystemFlags Flags { get; } = SystemFlags.None;

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
            if (gameObject is Probe probe)
            {
                AddProbe(probe);
            }
        }

        public unsafe void Unregister(GameObject gameObject)
        {
            if (gameObject is Light light)
            {
                light.DestroyShadowMap();
                RemoveLight(light);
            }
            if (gameObject is Probe probe)
            {
                RemoveProbe(probe);
            }
        }

        public unsafe void AddLight(Light light)
        {
            lock (lights)
            {
                lights.Add(light);
                lightUpdateQueue.Enqueue(light);
                light.OnTransformed += LightTransformed;
                light.PropertyChanged += LightPropertyChanged;
            }
        }

        public unsafe void AddProbe(Probe probe)
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
                light.OnTransformed -= LightTransformed;
                lights.Remove(light);
                activeLights.Remove(light);
            }
        }

        public unsafe void RemoveProbe(Probe probe)
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

            UpdateLights(camera);

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
                if (light.ShadowMapEnable && !light.InUpdateQueue)
                {
                    if (light is DirectionalLight || light.ShadowMapUpdateMode == ShadowUpdateMode.EveryFrame | light.UpdateShadowMapSize(camera, shadowAtlas))
                    {
                        light.InUpdateQueue = true;
                        UpdateShadowLightQueue.Enqueue(light);
                    }
                }
            }
        }

        private unsafe void UpdateLights(Camera camera)
        {
            GlobalProbes.ResetCounter();
            LightBuffer.ResetCounter();
            ShadowDataBuffer.ResetCounter();

            for (int i = 0; i < probes.Count; i++)
            {
                var probe = probes[i];
                if (!probe.IsEnabled)
                {
                    continue;
                }

                // extend code or move it directly to the pass code.
            }

            for (int i = 0; i < activeLights.Count; i++)
            {
                var light = activeLights[i];

                if (light.ShadowMapEnable)
                {
                    switch (light.LightType)
                    {
                        case LightType.Directional:
                            var dir = (DirectionalLight)light;
                            dir.QueueIndex = ShadowDataBuffer.Count;
                            LightBuffer.Add(new(dir));
                            ShadowDataBuffer.Add(new(dir, dir.ShadowMapSize));
                            dir.UpdateShadowBuffer(ShadowDataBuffer, camera);
                            break;

                        case LightType.Point:
                            var point = (PointLight)light;
                            point.QueueIndex = ShadowDataBuffer.Count;
                            LightBuffer.Add(new(point));
                            ShadowDataBuffer.Add(new(point, point.ShadowMapSize));
                            point.UpdateShadowBuffer(ShadowDataBuffer);
                            break;

                        case LightType.Spot:
                            var spot = (Spotlight)light;
                            spot.QueueIndex = ShadowDataBuffer.Count;
                            LightBuffer.Add(new(spot));
                            ShadowDataBuffer.Add(new(spot, spot.ShadowMapSize));
                            spot.UpdateShadowBuffer(ShadowDataBuffer);
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

        public unsafe void Dispose()
        {
            GlobalProbes.Dispose();
            LightBuffer.Dispose();
            ShadowDataBuffer.Dispose();
        }
    }
}