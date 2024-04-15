namespace HexaEngine.Graphics
{
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;

    public class LODSystem : ISceneSystem
    {
        private readonly ComponentTypeQuery<ILODRendererComponent> lodRenderers = new();

        public string Name { get; } = "LOD System";

        public SystemFlags Flags { get; } = SystemFlags.Awake | SystemFlags.Update | SystemFlags.Destroy;

        public float LODBias { get; set; }

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(lodRenderers);
        }

        private readonly float accumulationThreshold = 1;
        private float accumulator;

        public void Update(float delta)
        {
            var camera = CameraManager.Current;

            if (camera == null)
            {
                return;
            }

            accumulator += delta;

            if (accumulator < accumulationThreshold)
            {
                return;
            }
            accumulator -= accumulationThreshold;

            var cameraPosition = camera.Transform.GlobalPosition;

            var far = 100;
            var near = 10;

            for (int i = 0; i < lodRenderers.Count; i++)
            {
                var lodRenderer = lodRenderers[i];

                float distance = lodRenderer.Distance(cameraPosition);
                int min = lodRenderer.MinLODLevel;
                int max = lodRenderer.MaxLODLevel;
                float bias = LODBias;
                int level = ComputeLODLevel(distance, min, max, bias, near, far);

                if (lodRenderer.LODLevel == level)
                {
                    continue;
                }

                lodRenderer.SetLODLevel(level);
            }
        }

        private static int ComputeLODLevel(float distance, int minLODLevel, int maxLODLevel, float bias, float minDistance, float maxDistance)
        {
            float factor = (maxLODLevel - minLODLevel) / (maxDistance - minDistance);
            int lodLevel = (int)(factor * (distance - minDistance) + minLODLevel + bias);
            return Math.Clamp(lodLevel, minLODLevel, maxLODLevel);
        }

        public void Unload()
        {
            lodRenderers.Dispose();
        }
    }
}