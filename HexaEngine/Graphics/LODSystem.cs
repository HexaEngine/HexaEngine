namespace HexaEngine.Graphics
{
    using HexaEngine.Queries.Generic;
    using HexaEngine.Scenes;
    using HexaEngine.Scenes.Managers;
    using System.Globalization;
    using System.Numerics;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public struct LODLevel : IXmlSerializable
    {
        public int LODIndex;
        public float MaxDistance;

        public readonly XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            reader.ReadStartElement("LODLevel");

            string? lodIndex = reader.GetAttribute("LODIndex");
            if (lodIndex != null)
            {
                LODIndex = int.Parse(lodIndex);
            }
            string? maxDistance = reader.GetAttribute("MaxDistance");
            if (maxDistance != null)
            {
                MaxDistance = float.Parse(maxDistance, NumberStyles.Any, CultureInfo.InvariantCulture);
            }

            reader.ReadEndElement();
        }

        public readonly void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("LODLevel");
            writer.WriteAttributeString("LODIndex", LODIndex.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("MaxDistance", MaxDistance.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndAttribute();
        }
    }

    public struct LODGroupDesc : IXmlSerializable
    {
        public Guid Guid { get; set; }

        public string Name { get; set; }

        public LODFadeMode FadeMode { get; set; }

        public readonly XmlSchema? GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            reader.ReadStartElement("LODGroup");
            string? guid = reader.GetAttribute("Guid");
            if (guid != null)
            {
                Guid = new Guid(guid);
            }
            string? name = reader.GetAttribute("Name");
            if (name != null)
            {
                Name = name;
            }
            string? fadeMode = reader.GetAttribute("FadeMode");
            if (fadeMode != null)
            {
                FadeMode = Enum.Parse<LODFadeMode>(fadeMode);
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    public class LODConfig
    {
        public List<LODLevel> Levels { get; } = [];

        public List<LODGroupDesc> Groups { get; } = [];
    }

    public class LODSystem : ISceneSystem
    {
        private readonly ComponentTypeQuery<ILODRendererComponent> lodRenderers = new();
        private readonly List<LODLevel> lodLevels = [];

        public string Name { get; } = "LOD System";

        public SystemFlags Flags { get; } = SystemFlags.Awake | SystemFlags.Update | SystemFlags.Destroy;

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
        private float updateDelta;

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

            far = 100;
            near = 10;

            Parallel.For(0, lodRenderers.Count, UpdateBatch);
        }

        private void UpdateBatch(int index)
        {
            var lodRenderer = lodRenderers[index];

            float distance = lodRenderer.Distance(cameraPosition);
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