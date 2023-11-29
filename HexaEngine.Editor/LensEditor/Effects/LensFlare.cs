namespace HexaEngine.Editor.LensEditor.Effects
{
    using Hexa.NET.ImGui;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Editor.MaterialEditor.Generator;
    using System;
    using System.Numerics;
    using System.Threading.Channels;

    public class LensFlare : LensEffectBase
    {
        private Vector2 flareScale = new(0.5f);
        private float flarePow = 0.7f;
        private float flarePow2 = 1.1f;
        private float flareFill = 10.0f;
        private float flareOffset = -0.5f;

        private float flareMin = 0.0f;
        private float flareMax = 1.0f;

        private Vector2 flareScaleSubtract = new(0.5f);
        private float flarePowSubtract = 1.0f;
        private float flarePow2Subtract = 0.9f;
        private float flareFillSubtract = 2.0f;
        private float flareOffsetSubtract = -0.65f;

        private float flareMinSubtract = 0.0f;
        private float flareMaxSubtract = 1.0f;

        private float finalFlareMin = 0.0f;
        private float finalFlareMax = 10.0f;

        private Vector3 flareChannelMask = new(1);

        private bool changed = false;
        private bool editing = false;

        public override string Name { get; } = "Flare";

        public Vector2 FlareScale { get => flareScale; set => flareScale = value; }

        public float FlarePow { get => flarePow; set => flarePow = value; }

        public float FlarePow2 { get => flarePow2; set => flarePow2 = value; }

        public float FlareFill { get => flareFill; set => flareFill = value; }

        public float FlareOffset { get => flareOffset; set => flareOffset = value; }

        public float FlareMin { get => flareMin; set => flareMin = value; }

        public float FlareMax { get => flareMax; set => flareMax = value; }

        public Vector2 FlareScaleSubtract { get => flareScaleSubtract; set => flareScaleSubtract = value; }

        public float FlarePowSubtract { get => flarePowSubtract; set => flarePowSubtract = value; }

        public float FlarePow2Subtract { get => flarePow2Subtract; set => flarePow2Subtract = value; }

        public float FlareFillSubtract { get => flareFillSubtract; set => flareFillSubtract = value; }

        public float FlareOffsetSubtract { get => flareOffsetSubtract; set => flareOffsetSubtract = value; }

        public float FlareMinSubtract { get => flareMinSubtract; set => flareMinSubtract = value; }

        public float FlareMaxSubtract { get => flareMaxSubtract; set => flareMaxSubtract = value; }

        public float FinalFlareMin { get => finalFlareMin; set => finalFlareMin = value; }

        public float FinalFlareMax { get => finalFlareMax; set => finalFlareMax = value; }

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

            changed = ImGui.InputFloat2($"Subtract Scale##{Id}", ref flareScaleSubtract);
            changed = ImGui.InputFloat($"Subtract Pow##{Id}", ref flarePowSubtract);
            changed = ImGui.InputFloat($"Subtract Pow2##{Id}", ref flarePow2Subtract);
            changed = ImGui.InputFloat($"Subtract Fill##{Id}", ref flareFillSubtract);
            changed = ImGui.InputFloat($"Subtract Offset##{Id}", ref flareOffsetSubtract);

            changed = ImGui.InputFloat($"Subtract Flare Min##{Id}", ref flareMinSubtract);
            changed = ImGui.InputFloat($"Subtract Flare Max##{Id}", ref flareMaxSubtract);

            changed = ImGui.InputFloat($"Final Flare Min##{Id}", ref finalFlareMin);
            changed = ImGui.InputFloat($"Final Flare Max##{Id}", ref finalFlareMax);

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

            //flare
            writer.WriteLine($"float2 {name}scale = float2(2.0 * {flareScale.X.ToHLSL()}, 2.0 * {flareScale.Y.ToHLSL()});");
            writer.WriteLine($"float {name}pow = {flarePow.ToHLSL()};");
            writer.WriteLine($"float {name}fill = {flareFill.ToHLSL()};");
            writer.WriteLine($"float {name}offset = {flareOffset.ToHLSL()};");
            writer.WriteLine($"float2 {name}pos = float2(((1.0 - pos.x) * ({name}offset + 1.0) - ({name}offset * 0.5)) * aspectRatio * {name}scale.x, ((1.0 - pos.y) * ({name}offset + 1.0) - ({name}offset * 0.5)) * {name}scale.y);");
            writer.WriteLine($"float {name} = distance({name}pos, float2(uv.x * aspectRatio * {name}scale.x, uv.y * {name}scale.y));");
            writer.WriteLine($"{name} = 0.5 - {name};");
            writer.WriteLine($"{name} = clamp({name} * {name}fill, {flareMin.ToHLSL()}, {flareMax.ToHLSL()});");
            writer.WriteLine($"{name} = sin({name} * 1.57075);");
            writer.WriteLine($"{name} *= sunmask;");
            writer.WriteLine($"{name} = pow({name}, {flarePow2.ToHLSL()});");

            writer.WriteLine($"{name} *= {name}pow;");

            //subtract
            writer.WriteLine($"float2 {name}Bscale = float2({flareScaleSubtract.X.ToHLSL()} * flarescale, 1.4 * {flareScaleSubtract.Y.ToHLSL()});");
            writer.WriteLine($"float {name}Bpow = {flarePowSubtract.ToHLSL()};");
            writer.WriteLine($"float {name}Bfill = {flareFillSubtract.ToHLSL()};");
            writer.WriteLine($"float {name}Boffset = {flareOffsetSubtract.ToHLSL()};");
            writer.WriteLine($"float2 {name}Bpos = float2(((1.0 - pos.x) * ({name}Boffset + 1.0) - ({name}Boffset * 0.5)) * aspectRatio * {name}Bscale.x, ((1.0 - pos.y) * ({name}Boffset + 1.0) - ({name}Boffset * 0.5)) * {name}Bscale.y);");
            writer.WriteLine($"float {name}B = distance({name}Bpos, float2(uv.x * aspectRatio * {name}Bscale.x, uv.y * {name}Bscale.y));");
            writer.WriteLine($"{name}B = 0.5 - {name}B;");
            writer.WriteLine($"{name}B = clamp({name}B * {name}Bfill, {flareMinSubtract.ToHLSL()}, {flareMaxSubtract.ToHLSL()});");
            writer.WriteLine($"{name}B = sin({name}B * 1.57075);");
            writer.WriteLine($"{name}B *= sunmask;");
            writer.WriteLine($"{name}B = pow({name}B, {flarePow2Subtract.ToHLSL()});");

            writer.WriteLine($"{name}B *= {name}Bpow;");

            writer.WriteLine($"{name} = clamp({name} - {name}B, {finalFlareMin.ToHLSL()}, {finalFlareMax.ToHLSL()});");

            writer.WriteLine($"color.r += {name} * {flareChannelMask.X.ToHLSL()} * flaremultR;");
            writer.WriteLine($"color.g += {name} * {flareChannelMask.Y.ToHLSL()} * flaremultG;");
            writer.WriteLine($"color.b += {name} * {flareChannelMask.Z.ToHLSL()} * flaremultB;");
        }
    }
}