namespace HexaEngine.Editor.Plugins
{
    using HexaEngine.Core;
    using HexaEngine.Core.Configuration;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.UI;
    using System;
    using System.Collections.Generic;

    public static class PluginManager
    {
        internal static readonly ILogger Logger = LoggerFactory.GetLogger(nameof(PluginManager));
        private static PluginLoader? loader;
        private static readonly List<Plugin> plugins = new();
        private static readonly ConfigKey config;

        static PluginManager()
        {
            config = Config.Global.GetOrCreateKey("Plugins");
            config.TryGetOrAddKeyValue("Reload", null, DataType.Button, false, out var button);
            button.ValueChanged += ReloadButton;
        }

        public static string PluginsPath { get; set; } = "plugins/";

        public static IReadOnlyList<Plugin> Plugins => plugins;

        public static void Load()
        {
            loader ??= new(PluginsPath);

            loader.Load();

            List<IPlugin> instances = [];

            foreach (var type in loader.GetAssignableTypes<IPlugin>())
            {
                try
                {
                    IPlugin? instance = (IPlugin?)Activator.CreateInstance(type);
                    if (instance is null)
                    {
                        continue;
                    }

                    instances.Add(instance);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    MessageBox.Show("Failed to create plugin instance", ex.Message);
                }
            }

            for (int i = 0; i < instances.Count; i++)
            {
                IPlugin? instance = instances[i];
                try
                {
                    Plugin plugin = new(instance);
                    plugins.Add(plugin);
                    config.GenerateSubKeyAuto(plugin, plugin.Name);
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    MessageBox.Show("Failed to create plugin config", ex.Message);
                }
            }
        }

        private static void ReloadButton(ConfigValue arg1, string? arg2)
        {
            Unload();
            Load();
        }

        public static void Unload()
        {
            for (int i = 0; i < plugins.Count; i++)
            {
                Plugin? plugin = plugins[i];
                try
                {
                    plugin.IsInitialized = false;
                    plugin.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex);
                    MessageBox.Show("Failed to unload plugin", ex.Message);
                }
            }
            plugins.Clear();
            loader?.Unload();

            Config.Global.Save();
        }
    }
}