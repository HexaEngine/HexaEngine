namespace HexaEngine.Editor.Widgets
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Weather;

    [EditorWindowCategory("Debug")]
    public class WeatherWidget : EditorWindow
    {
        protected override string Name => "Weather";

        public override unsafe void DrawContent(IGraphicsContext context)
        {
            var manager = WeatherSystem.Current;
            if (manager == null)
                return;

            var skyColor = manager.SkyColor;
            if (ImGui.ColorEdit3("SkyColor", ref skyColor))
                manager.SkyColor = skyColor;
            var sunDir = manager.SunDir;
            ImGui.InputFloat3("Sun Direction", (float*)&sunDir, ImGuiInputTextFlags.ReadOnly);
            var ambientColor = manager.AmbientColor;
            if (ImGui.ColorEdit3("AmbientColor", ref ambientColor))
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
            var coverage = manager.Coverage;
            if (ImGui.InputFloat("Coverage", ref coverage))
                manager.Coverage = coverage;
            var overcast = manager.Overcast;
            if (ImGui.InputFloat("Overcast", ref overcast))
                manager.Overcast = overcast;
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
            var phaseFunctionG = manager.PhaseFunctionG;
            if (ImGui.InputFloat("PhaseFunctionG", ref phaseFunctionG))
                manager.PhaseFunctionG = phaseFunctionG;
        }
    }
}