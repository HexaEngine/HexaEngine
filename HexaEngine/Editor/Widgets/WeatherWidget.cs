namespace HexaEngine.Editor.Widgets
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.ImGuiNET;
    using HexaEngine.Weather;

    public class WeatherWidget : EditorWindow
    {
        protected override string Name => "Weather";

        public override void DrawContent(IGraphicsContext context)
        {
            var manager = WeatherManager.Current;
            if (manager == null)
                return;

            var skyColor = manager.SkyColor;
            if (ImGui.InputFloat3("SkyColor", ref skyColor))
                manager.SkyColor = skyColor;
            var ambientColor = manager.AmbientColor;
            if (ImGui.InputFloat3("AmbientColor", ref ambientColor))
                manager.AmbientColor = ambientColor;
            var windDirection = manager.WindDirection;
            if (ImGui.InputFloat2("WindDirection", ref windDirection))
                manager.WindDirection = windDirection;
            var windSpeed = manager.WindSpeed;
            if (ImGui.InputFloat("WindSpeed", ref windSpeed))
                manager.WindSpeed = windSpeed;
            var crispiness = manager.Crispiness;
            if (ImGui.InputFloat("Crispiness", ref crispiness))
                manager.Crispiness = crispiness;
            var curliness = manager.Curliness;
            if (ImGui.InputFloat("Curliness", ref curliness))
                manager.Curliness = curliness;
            var lightAbsorption = manager.LightAbsorption;
            if (ImGui.InputFloat("LightAbsorption", ref lightAbsorption))
                manager.LightAbsorption = lightAbsorption;
            var cloudsBottomHeight = manager.CloudsBottomHeight;
            if (ImGui.InputFloat("CloudsBottomHeight", ref cloudsBottomHeight))
                manager.CloudsBottomHeight = cloudsBottomHeight;
            var cloudsTopHeight = manager.CloudsTopHeight;
            if (ImGui.InputFloat("CloudsTopHeight", ref cloudsTopHeight))
                manager.CloudsTopHeight = cloudsTopHeight;
            var densityFactor = manager.DensityFactor;
            if (ImGui.InputFloat("DensityFactor", ref densityFactor))
                manager.DensityFactor = densityFactor;
            var cloudType = manager.CloudType;
            if (ImGui.InputFloat("CloudType", ref cloudType))
                manager.CloudType = cloudType;
            var turbidity = manager.Turbidity;
            if (ImGui.InputFloat("Turbidity", ref turbidity))
                manager.Turbidity = turbidity;
            var groundAlbedo = manager.GroundAlbedo;
            if (ImGui.InputFloat("GroundAlbedo", ref groundAlbedo))
                manager.GroundAlbedo = groundAlbedo;
        }
    }
}