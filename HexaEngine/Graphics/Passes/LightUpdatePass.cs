namespace HexaEngine.Graphics.Passes
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Lights.Structs;
    using HexaEngine.Lights.Types;
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

            uint csmCount = 0;

            var probes = lights.Probes;
            var activeLights = lights.Active;

            for (int i = 0; i < probes.Count; i++)
            {
                var probe = probes[i];
                if (!probe.IsEnabled)
                {
                    continue;
                }

                // needs to extended.
            }

            for (int i = 0; i < activeLights.Count; i++)
            {
                var lightSource = activeLights[i];
                if (lightSource is not Light light)
                    continue;

                if (light.ShadowMapEnable)
                {
                    switch (light.LightType)
                    {
                        case LightType.Directional:
                            if (csmCount == 1)
                            {
                                continue;
                            }
                            var dir = (DirectionalLight)light;
                            dir.QueueIndex = ShadowDataBuffer.Count;
                            LightBuffer.Add(new(dir));
                            ShadowDataBuffer.Add(new(dir, dir.ShadowMapSize));
                            dir.UpdateShadowBuffer(ShadowDataBuffer, CameraManager.Current);
                            csmCount++;
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
                            LightBuffer.Add(new());
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
    }
}