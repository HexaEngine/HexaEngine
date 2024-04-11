namespace HexaEngine.Editor.Plugins
{
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.UI;

    /// <summary>
    /// Represents a wrapper for an <see cref="IPlugin"/> instance, providing additional functionality and state management.
    /// </summary>
    public class Plugin
    {
        private readonly IPlugin plugin;
        private bool isEnabled;
        private bool isInitialized;

        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class with the specified IPlugin instance.
        /// </summary>
        /// <param name="plugin">The IPlugin instance to wrap.</param>
        public Plugin(IPlugin plugin)
        {
            this.plugin = plugin;
        }

        /// <summary>
        /// Gets the name of the wrapped plugin.
        /// </summary>
        public string Name => plugin.Name;

        /// <summary>
        /// Gets the version of the wrapped plugin.
        /// </summary>
        public string Version => plugin.Version;

        /// <summary>
        /// Gets the description of the wrapped plugin.
        /// </summary>
        public string Description => plugin.Description;

        /// <summary>
        /// Gets or sets a value indicating whether the plugin is currently enabled.
        /// </summary>
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
                    PluginManager.Logger.Log(ex);
                    MessageBox.Show("Failed to enable plugin", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the plugin is currently initialized.
        /// </summary>
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
                    PluginManager.Logger.Log(ex);
                    MessageBox.Show("Failed to initialize plugin", ex.Message);
                }
            }
        }
    }
}