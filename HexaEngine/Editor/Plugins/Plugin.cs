namespace HexaEngine.Editor.Plugins
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.UI;

    public class Plugin
    {
        private readonly IPlugin plugin;
        private bool isEnabled;
        private bool isInitialized;

        public Plugin(IPlugin plugin)
        {
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

                try
                {
                    if (value)
                    {
                        plugin.OnEnable();
                    }
                    else
                    {
                        plugin.OnDisable();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    MessageBox.Show("Failed to enable plugin", ex.Message);
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

                try
                {
                    if (value)
                    {
                        plugin.OnInitialize();
                    }
                    else
                    {
                        plugin.OnUninitialize();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    MessageBox.Show("Failed to initialize plugin", ex.Message);
                }
            }
        }
    }
}