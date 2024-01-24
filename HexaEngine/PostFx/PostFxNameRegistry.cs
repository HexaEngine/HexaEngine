namespace HexaEngine.PostFx
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class PostFxNameRegistry
    {
        private readonly List<IPostFx> effects = [];
        private readonly Dictionary<string, IPostFx> nameToEffect = [];
        private readonly object _lock = new();

        public string? GetNameBy<T>() where T : IPostFx
        {
            lock (_lock)
            {
                for (int i = 0; i < effects.Count; i++)
                {
                    IPostFx effect = effects[i];
                    if (effect is T)
                    {
                        return effect.Name;
                    }
                }
                return null;
            }
        }

        public bool TryGetNameBy<T>([NotNullWhen(true)] out string? name) where T : IPostFx
        {
            lock (_lock)
            {
                for (int i = 0; i < effects.Count; i++)
                {
                    IPostFx effect = effects[i];
                    if (effect is T)
                    {
                        name = effect.Name;
                        return true;
                    }
                }
                name = null;
                return false;
            }
        }

        public IPostFx? GetPostFxByName(string name)
        {
            lock (_lock)
            {
                if (!nameToEffect.TryGetValue(name, out var effect))
                {
                    return null;
                }
                return effect;
            }
        }

        public bool TryGetPostFxByName(string name, [NotNullWhen(true)] out IPostFx? effect)
        {
            lock (_lock)
            {
                return nameToEffect.TryGetValue(name, out effect);
            }
        }

        public void Add(IPostFx effect)
        {
            lock (_lock)
            {
                effects.Add(effect);
                nameToEffect.Add(effect.Name, effect);
            }
        }

        public void Remove(IPostFx effect)
        {
            lock (_lock)
            {
                nameToEffect.Remove(effect.Name);
                effects.Remove(effect);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                effects.Clear();
                nameToEffect.Clear();
            }
        }
    }
}