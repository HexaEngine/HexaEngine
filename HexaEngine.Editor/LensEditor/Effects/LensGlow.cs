namespace HexaEngine.Editor.LensEditor.Effects
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Editor.MaterialEditor.Generator;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

    public class LensGlow : LensEffectBase
    {
        private Vector2 flareScale = new(0.5f);
        private float flarePow = 0.7f;
        private float flarePow2 = 1.1f;
        private float flareFill = 10.0f;
        private float flareOffset = -0.5f;

        private float flareMin = 0.0f;
        private float flareMax = 1.0f;

        private Vector3 flareChannelMask = new(1);

        private bool changed = false;
        private bool editing = false;

        public override string Name { get; } = "Glow";

        public Vector2 FlareScale { get => flareScale; set => flareScale = value; }

        public float FlarePow { get => flarePow; set => flarePow = value; }

        public float FlarePow2 { get => flarePow2; set => flarePow2 = value; }

        public float FlareFill { get => flareFill; set => flareFill = value; }

        public float FlareOffset { get => flareOffset; set => flareOffset = value; }

        public float FlareMin { get => flareMin; set => flareMin = value; }

        public float FlareMax { get => flareMax; set => flareMax = value; }

        public Vector3 FlareChannelMask { get => flareChannelMask; set => flareChannelMask = value; }

        public override bool DrawContent()
        {
            changed = ImGui.InputFloat2($"Scale##{Id}", ref flareScale);
            changed = ImGui.InputFloat($"Pow##{Id}", ref flarePow);
            changed = ImGui.InputFloat($"Pow2##{Id}", ref flarePow2);
            changed = ImGui.InputFloat($"Fill##{Id}", ref flareFill);
            changed = ImGui.InputFloat($"Offset##{Id}", ref flareOffset);

            changed = ImGui.InputFloat($"Flare Min##{Id}", ref flareMin);
            changed = ImGui.InputFloat($"Flare Max##{Id}", ref flareMax);

            changed = ImGui.ColorEdit3($"Color Mask##{Id}", ref flareChannelMask);

            editing = ImGui.IsAnyItemActive();
            if (changed && !editing)
            {
                changed = false;
                return true;
            }

            return false;
        }

        public override void Generate(CodeWriter writer, int index)
        {
            string name = $"flare{index}";
            writer.WriteLine($"float2 {name}scale = float2({flareScale.X.ToHLSL()} * flarescale, {flareScale.Y.ToHLSL()} * flarescale);");
            writer.WriteLine($"float {name}pow = {flarePow.ToHLSL()};");
            writer.WriteLine($"float {name}fill = {flareFill.ToHLSL()};");
            writer.WriteLine($"float {name}offset = {flareOffset.ToHLSL()};");
            writer.WriteLine($"float2 {name}pos = float2(((1.0 - pos.x) * ({name}offset + 1.0) - ({name}offset * 0.5)) * aspectRatio * {name}scale.x, ((1.0 - pos.y) * ({name}offset + 1.0) - ({name}offset * 0.5)) * {name}scale.y);");

            writer.WriteLine($"float {name} = distance({name}pos, float2(uv.x * aspectRatio * {name}scale.x, uv.y * {name}scale.y));");
            writer.WriteLine($"{name} = 0.5 - {name};");
            writer.WriteLine($"{name} = clamp({name} * {name}fill, {flareMin.ToHLSL()}, {flareMax.ToHLSL()});");
            writer.WriteLine($"{name} *= sunmask;");
            writer.WriteLine($"{name} = pow({name}, {flarePow2.ToHLSL()});");

            writer.WriteLine($"{name} *= {name}pow;");

            writer.WriteLine($"color.r += {name} * {flareChannelMask.X.ToHLSL()} * flaremultR;");
            writer.WriteLine($"color.g += {name} * {flareChannelMask.Y.ToHLSL()} * flaremultG;");
            writer.WriteLine($"color.b += {name} * {flareChannelMask.Z.ToHLSL()} * flaremultB;");
        }
    }
}