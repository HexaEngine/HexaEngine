namespace HexaEngine.Plugins
{
    public class Plugin
    {
        private readonly IServiceProvider provider;
        private readonly IPlugin plugin;
        private bool isEnabled;
        private bool isInitialized;

        public Plugin(IServiceProvider provider, IPlugin plugin)
        {
            this.provider = provider;
            this.plugin = plugin;
        }

        public string GetName() => plugin.Name;

        public string Version => plugin.Version;

        public string Description => plugin.Description;

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled == value)
                {
                    return;
                }

                isEnabled = value;

                if (value)
                {
                    plugin.OnEnable();
                }
                else
                {
                    plugin.OnDisable();
                }
            }
        }

        internal bool IsInitialized
        {
            get => isInitialized;
            set
            {
                if (isInitialized == value)
                {
                    return;
                }

                isInitialized = value;

                if (!isEnabled)
                {
                    return;
                }

                if (value)
                {
                    plugin.OnInitialize(provider);
                }
                else
                {
                    plugin.OnUninitialize();
                }
            }
        }
    }
}