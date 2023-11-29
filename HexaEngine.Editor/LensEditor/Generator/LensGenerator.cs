namespace HexaEngine.Editor.LensEditor.Generator
{
    using HexaEngine.Editor.MaterialEditor.Generator;
    using System.Collections.Generic;
    using System.Text;

    public class LensGenerator
    {
        public string Generate(IList<ILensEffect> effects)
        {
            StringBuilder sb = new();

            CodeWriter writer = new(sb);

            WriteHeader(writer);
            WriteCode(writer, effects);
            WriteMain(writer);

            return sb.ToString();
        }

        private void WriteMain(CodeWriter writer)
        {
            using (writer.PushBlock("float4 main(VSOut pin) : SV_Target"))
            {
                writer.WriteLine("float2 uv = pin.Tex;");
                writer.WriteLine("float2 sunPos = sunPosition.xy; ");

                writer.WriteLine("float referenceDepth = saturate(sunPosition.z); ");

                // determine the flare opacity:
                // These values work well for me, but should be tweakable
                writer.WriteLine("const float2 step = 1.0f / screenDim; ");
                writer.WriteLine("const float2 range = 10.5f * step; ");
                writer.WriteLine("float samples = 0.0f; ");
                writer.WriteLine("float accdepth = 0.0f; ");

                using (writer.PushBlock("for (float y = -range.y; y <= range.y; y += step.y)"))
                {
                    using (writer.PushBlock(" for (float x = -range.x; x <= range.x; x += step.x)"))
                    {
                        writer.WriteLine("samples += 1.0f; ");
                        writer.WriteLine("accdepth += depthTex.SampleLevel(linearClampSampler, sunPosition.xy + float2(x, y), 0).r >= referenceDepth - 0.001 ? 1 : 0; ");
                    }
                }

                writer.WriteLine("accdepth /= samples; ");

                using (writer.PushBlock("if (accdepth <= 0)"))
                {
                    writer.WriteLine("discard; ");
                }

                writer.WriteLine("float3 color = 0; ");
                writer.WriteLine("color = lensflare(uv, sunPos.xy) * accdepth; ");
                writer.WriteLine("return float4(color, 1.0); ");
            }
        }

        private void WriteCode(CodeWriter writer, IList<ILensEffect> effects)
        {
            using (writer.PushBlock("float3 lensflare(float2 uv, float2 pos)"))
            {
                writer.WriteLine("float aspectRatio = screenDim.x / screenDim.y;");

                writer.WriteLine("float3 color = 0;");

                writer.WriteLine("float flaremultR = 0.04 * tint.r;");
                writer.WriteLine("float flaremultG = 0.05 * tint.g;");
                writer.WriteLine("float flaremultB = 0.075 * tint.b;");
                writer.WriteLine("float flarescale = 1.0;");

                writer.WriteLine("float sunmask = 1;");
                writer.WriteLine("float sunVisibility = 1;");

                writer.WriteLine("float cosTheta = light_dir.y;");
                using (writer.PushBlock("if (cosTheta < 0.0f)"))
                {
                    writer.WriteLine("float a = clamp(1.0f + cosTheta * 50.0f, 0, 1);");
                    writer.WriteLine("sunmask *= a;");
                }

                for (int i = 0; i < effects.Count; i++)
                {
                    effects[i].Generate(writer, i);
                }

                writer.WriteLine("return color;");
            }
        }

        private void WriteHeader(CodeWriter writer)
        {
            writer.WriteLine("#include \"../camera.hlsl\"");
            writer.WriteLine("#include \"../weather.hlsl\"");

            using (writer.PushBlock("struct VSOut"))
            {
                writer.WriteLine("float4 Pos : SV_Position;");
                writer.WriteLine("float2 Tex : TEXCOORD;");
            }
            writer.WriteLine(";");

            using (writer.PushBlock("cbuffer LensParams : register(b0)"))
            {
                writer.WriteLine("float4 sunPosition;");
                writer.WriteLine("float4 tint;");
            }
            writer.WriteLine(";");

            writer.WriteLine("Texture2D<float> depthTex : register(t0);");
            writer.WriteLine("SamplerState linearClampSampler : register(s0);");
        }
    }
}