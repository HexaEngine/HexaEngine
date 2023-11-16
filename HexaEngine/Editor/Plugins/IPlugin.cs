namespace HexaEngine.Editor.Plugins
{
    /// <summary>
    /// Represents an interface for a plugin in the HexaEngine editor.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the version of the plugin.
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Gets the description of the plugin.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Called when the plugin is enabled.
        /// </summary>
        void OnEnable();

        /// <summary>
        /// Called when the plugin is initialized.
        /// </summary>
        void OnInitialize();

        /// <summary>
        /// Called when the plugin is uninitialized.
        /// </summary>
        void OnUninitialize();

        /// <summary>
        /// Called when the plugin is disabled.
        /// </summary>
        void OnDisable();
    }
}