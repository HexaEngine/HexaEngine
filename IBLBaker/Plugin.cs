namespace IBLBaker
{
    using HexaEngine.Editor;
    using HexaEngine.Editor.Widgets;
    using HexaEngine.Plugins;
    using System;

    public class Plugin : IPlugin
    {
        public string Name => "IBL Baker Plugin";

        public string Version => "1.0.0.0";

        public string Description => "Allows to bake irradiance from cube maps";

        public void OnEnable()
        {
            WidgetManager.Register<IrradianceWidget>();
            WidgetManager.Register<PrefilterWidget>();
        }

        public void OnDisable()
        {
            WidgetManager.Unregister<IrradianceWidget>();
            WidgetManager.Unregister<PrefilterWidget>();
        }

        public void OnInitialize(IServiceProvider provider)
        {
        }

        public void OnUninitialize()
        {
        }
    }
}