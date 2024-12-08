namespace HexaEngine.Graphics
{
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Numerics;

    public class LODSystem : ISceneSystem
    {
        private readonly ComponentTypeQuery<ILODRendererComponent> lodRenderers = new();

        public string Name { get; } = "LOD System";

        public SystemFlags Flags { get; } = SystemFlags.Awake | SystemFlags.CameraUpdate | SystemFlags.Destroy;

        public float LODBias { get; set; }

        public int UpdatesPerSecond
        {
            get => updatesPerSecond;
            set
            {
                updatesPerSecond = value;
                updateDelta = 1 / value;
            }
        }

        public void Awake(Scene scene)
        {
            scene.QueryManager.AddQuery(lodRenderers);
        }

        private float accumulator;
        private int updatesPerSecond = 20;
        private float updateDelta = 1 / 20;

        private Vector3 cameraPosition;
        private float far;
        private float near;

        public void Update(float delta)
        {
            var camera = CameraManager.Current;

            if (camera == null)
            {
                return;
            }

            accumulator += delta;

            if (accumulator < updateDelta)
            {
                return;
            }

            accumulator -= updateDelta;

            cameraPosition = camera.Transform.GlobalPosition;

            far = camera.Far;
            near = camera.Near;

            Parallel.For(0, lodRenderers.Count, UpdateBatch);
        }

        private void UpdateBatch(int index)
        {
            var lodRenderer = lodRenderers[index];

            float distance = lodRenderer.Distance(cameraPosition);
            if (distance > far) return;
            int min = lodRenderer.MinLODLevel;
            int max = lodRenderer.MaxLODLevel;
            float bias = LODBias;
            int level = ComputeLODLevel(distance, min, max, bias, near, far);

            if (lodRenderer.LODLevel == level)
            {
                return;
            }

            lodRenderer.SetLODLevel(level);
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