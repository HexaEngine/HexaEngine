namespace HexaEngine.Graphics
{
    using HexaEngine.Core.UI;
    using System.Diagnostics.CodeAnalysis;

    public class DrawLayerCollection : List<DrawLayer>
    {
    }

    public class DrawLayer
    {
        private ImGuiName displayName;
        private string name;

        public DrawLayer(string name, bool isDefault)
        {
            IsDefault = isDefault;
            this.name = name;
            displayName = new ImGuiName(name);
        }

        public bool IsDefault { get; private set; }

        [JsonIgnore]
        public ImGuiName DisplayName => displayName;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                displayName.SetName(value);
            }
        }
    }

    public class DrawLayerManager
    {
        private readonly List<DrawLayer> drawLayers = [];
        private readonly object _lock = new();

        public DrawLayerManager()
        {
            /*
            drawLayers.Add(new("Default", true));
            drawLayers.Add(new("Background", true));
            drawLayers.Add(new("Terrain", true));
            drawLayers.Add(new("Entity", true));
            drawLayers.Add(new("Effect", true));
            drawLayers.Add(new("UI", true));
            drawLayers.Add(new("Skybox", true));
            */
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return drawLayers.Count;
                }
            }
        }

        public IReadOnlyList<DrawLayer> DrawLayers => drawLayers;

        public object SyncObject => _lock;

        public DrawLayer AddLayer(string name)
        {
            DrawLayer layer = new(name, false);
            lock (_lock)
            {
                drawLayers.Add(layer);
            }
            return layer;
        }

        public bool RemoveLayer(DrawLayer drawLayer)
        {
            if (drawLayer.IsDefault)
            {
                return false;
            }

            lock (_lock)
            {
                return drawLayers.Remove(drawLayer);
            }
        }

        public DrawLayer? FindLayer(string name)
        {
            lock (_lock)
            {
                for (int i = 0; i < drawLayers.Count; i++)
                {
                    var drawLayer = drawLayers[i];
                    if (drawLayer.Name == name)
                    {
                        return drawLayer;
                    }
                }
            }

            return null;
        }

        public bool TryFindLayer(string name, [NotNullWhen(true)] out DrawLayer? layer)
        {
            layer = FindLayer(name);
            return layer != null;
        }
    }
}