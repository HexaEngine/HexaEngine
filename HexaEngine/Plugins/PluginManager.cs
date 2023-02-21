namespace HexaEngine.Plugins
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using System;
    using System.Collections.Generic;

    public static class PluginManager
    {
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

            List<IPlugin> instances = new();

            foreach (var type in loader.GetAssignableTypes<IPlugin>())
            {
                try
                {
                    IPlugin? instance = (IPlugin?)Activator.CreateInstance(type);
                    if (instance is null) continue;
                    instances.Add(instance);
                }
                catch (Exception ex)
                {
                    ImGuiConsole.Log(ex);
                }
            }

            foreach (var instance in instances)
            {
                try
                {
                    Plugin plugin = new(instance);
                    plugins.Add(plugin);
                    config.GenerateSubKeyAuto(plugin, plugin.GetName());
                }
                catch (Exception ex)
                {
                    ImGuiConsole.Log(ex);
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
            foreach (var plugin in plugins)
            {
                try
                {
                    plugin.IsInitialized = false;
                    plugin.IsEnabled = false;
                }
                catch (Exception ex)
                {
                    ImGuiConsole.Log(ex);
                }
            }
            plugins.Clear();
            loader?.Unload();

            Config.Global.Save();
        }
    }
}