namespace HexaEngine.Volumes
{
    using HexaEngine.PostFx;

    public class PostFxSettingsContainer
    {
        private readonly List<PostFxProxy> proxies = [];
        private readonly Dictionary<IPostFx, PostFxProxy> proxiesDictionary = [];

        public PostFxSettingsContainer()
        {
        }

        [JsonConstructor]
        public PostFxSettingsContainer(IEnumerable<PostFxProxy> proxies)
        {
            this.proxies = new(proxies);
        }

        public PostFxProxy this[IPostFx index]
        {
            get => proxiesDictionary[index];
        }

        public IReadOnlyList<PostFxProxy> Proxies => proxies;

        [JsonIgnore]
        public int Count => proxies.Count;

        public void Build(IReadOnlyList<IPostFx> effects)
        {
            // rebuilds the lookup table and adds unknown effects.
            proxiesDictionary.Clear();
            foreach (var effect in effects)
            {
                bool found = false;
                for (var i = 0; i < proxies.Count; i++)
                {
                    var proxy = proxies[i];
                    if (proxy.TypeName == effect.GetType().FullName)
                    {
                        proxy.UpdateType(effect);
                        proxiesDictionary.Add(effect, proxy);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    PostFxProxy proxy = new(effect);
                    proxies.Add(proxy);
                    proxiesDictionary.Add(effect, proxy);
                }
            }
        }

        public void Apply(IReadOnlyList<IPostFx> effects)
        {
            foreach (var effect in effects)
            {
                proxiesDictionary[effect].Apply(effect);
            }
        }

        public void Apply(IReadOnlyList<IPostFx> effects, PostFxSettingsContainer baseContainer, float blend, VolumeTransitionMode mode)
        {
            foreach (var effect in effects)
            {
                proxiesDictionary[effect].Apply(effect, baseContainer.proxiesDictionary[effect], blend, mode);
            }
        }

        public void Clear()
        {
            proxies.Clear();
            proxiesDictionary.Clear();
        }
    }
}