namespace HexaEngine.Rendering.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Structs;
    using HexaEngine.Lights.Types;
    using HexaEngine.Rendering.Graph;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;

    public class LightUpdatePass : RenderPass
    {
        public StructuredUavBuffer<ProbeData> GlobalProbes;
        public StructuredUavBuffer<LightData> LightBuffer;
        public StructuredUavBuffer<ShadowData> ShadowDataBuffer;

        public LightUpdatePass() : base("LightUpdate")
        {
        }

        public override void Execute(IGraphicsContext context, GraphResourceBuilder creator, ICPUProfiler? profiler)
        {
            var current = SceneManager.Current;
            if (current == null)
            {
                return;
            }

            var lights = current.LightManager;

            GlobalProbes.ResetCounter();
            LightBuffer.ResetCounter();
            LightBuffer.Clear(context);
            ShadowDataBuffer.ResetCounter();
            uint globalProbesCount = 0;
            uint csmCount = 0;

            var probes = lights.Probes;
            var activeLights = lights.Active;

            for (int i = 0; i < probes.Count; i++)
            {
                var probe = probes[i];
                if (!(probe.IsEnabled))
                {
                    continue;
                }

                // needs to extended.
            }

            for (int i = 0; i < activeLights.Count; i++)
            {
                var light = activeLights[i];

                if (light.ShadowMapEnable)
                {
                    switch (light.LightType)
                    {
                        case LightType.Directional:
                            if (csmCount == LightManager.MaxDirectionalLightSDs)
                            {
                                continue;
                            }

                            light.QueueIndex = ShadowDataBuffer.Count;
                            LightBuffer.Add(new((DirectionalLight)light));
                            ShadowDataBuffer.Add(new((DirectionalLight)light, DirectionalLight.ShadowMapSize));
                            //forwardSrvs[14] = forwardClusterdSrvs[16] = deferredSrvs[9] = deferredClusterdSrvs[11] = (void*)light.GetShadowMap()?.NativePointer;
                            ((DirectionalLight)light).UpdateShadowBuffer(ShadowDataBuffer, CameraManager.Current);
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
    }
}