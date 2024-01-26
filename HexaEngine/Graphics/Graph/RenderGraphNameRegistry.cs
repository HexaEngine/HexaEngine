namespace HexaEngine.Graphics.Graph
{
    using System.Diagnostics.CodeAnalysis;

    public class RenderGraphNameRegistry
    {
        private readonly List<RenderPass> passes = [];
        private readonly Dictionary<string, RenderPass> nameToPass = [];
        private readonly object _lock = new();

        public string? GetNameBy<T>() where T : RenderPass
        {
            lock (_lock)
            {
                for (int i = 0; i < passes.Count; i++)
                {
                    RenderPass effect = passes[i];
                    if (effect is T)
                    {
                        return effect.Name;
                    }
                }
                return null;
            }
        }

        public bool TryGetNameBy<T>([NotNullWhen(true)] out string? name) where T : RenderPass
        {
            lock (_lock)
            {
                for (int i = 0; i < passes.Count; i++)
                {
                    RenderPass pass = passes[i];
                    if (pass is T)
                    {
                        name = pass.Name;
                        return true;
                    }
                }
                name = null;
                return false;
            }
        }

        public RenderPass? GetPostFxByName(string name)
        {
            lock (_lock)
            {
                if (!nameToPass.TryGetValue(name, out var effect))
                {
                    return null;
                }
                return effect;
            }
        }

        public bool TryGetPostFxByName(string name, [NotNullWhen(true)] out RenderPass? effect)
        {
            lock (_lock)
            {
                return nameToPass.TryGetValue(name, out effect);
            }
        }

        public void Add(RenderPass effect)
        {
            lock (_lock)
            {
                passes.Add(effect);
                nameToPass.Add(effect.Name, effect);
            }
        }

        public void Remove(RenderPass effect)
        {
            lock (_lock)
            {
                nameToPass.Remove(effect.Name);
                passes.Remove(effect);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                passes.Clear();
                nameToPass.Clear();
            }
        }
    }
}