namespace HexaEngine.Resources.Factories
{
    using Hexa.NET.Logging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Metadata;
    using HexaEngine.Materials;
    using System.Text;
    using HexaEngine.Core.IO;

    public static class MaterialShaderResourceFactoryExtensions
    {
        public static Resources.MaterialShader LoadMaterialShader<T>(this ResourceManager manager, MaterialShaderDesc desc)
        {
            return manager.CreateInstance<Resources.MaterialShader, MaterialShaderDesc>(ResourceTypeRegistry.GetGuid<T>(desc.MaterialId), desc) ?? throw new NotSupportedException();
        }

        public static Resources.MaterialShader LoadMaterialShader(this ResourceManager manager, int type, MaterialShaderDesc desc)
        {
            return manager.CreateInstance<Resources.MaterialShader, MaterialShaderDesc>(new(desc.MaterialId, type), desc) ?? throw new NotSupportedException();
        }

        public static async Task<Resources.MaterialShader> LoadMaterialShaderAsync<T>(this ResourceManager manager, MaterialShaderDesc desc)
        {
            return await manager.CreateInstanceAsync<Resources.MaterialShader, MaterialShaderDesc>(ResourceTypeRegistry.GetGuid<T>(desc.MaterialId), desc) ?? throw new NotSupportedException();
        }

        public static async Task<Resources.MaterialShader> LoadMaterialShaderAsync(this ResourceManager manager, int type, MaterialShaderDesc desc)
        {
            return await manager.CreateInstanceAsync<Resources.MaterialShader, MaterialShaderDesc>(new(desc.MaterialId, type), desc) ?? throw new NotSupportedException();
        }

        public static void UpdateMaterialShader(this ResourceManager manager, Resources.MaterialShader? shader, MaterialShaderDesc desc)
        {
            if (shader == null)
            {
                return;
            }

            shader.Update(desc);
            shader.Reload();
        }

        public static void UpdateMaterialShader(this ResourceManager manager, Resources.MaterialShader? shader, MaterialData desc)
        {
            if (shader == null)
            {
                return;
            }

            var shaderDesc = shader.Description;

            if (desc.Metadata.TryGet<MetadataStringEntry>(MaterialNodeConverter.MetadataSurfaceVersionKey, out var versionKey) && desc.Metadata.TryGet<MetadataStringEntry>(MaterialNodeConverter.MetadataSurfaceKey, out var shaderKey))
            {
                for (var i = 0; i < shaderDesc.Passes.Length; i++)
                {
                    var pass = shaderDesc.Passes[i];
                    if (pass.SurfaceShader && pass.BaseShader != null)
                    {
                        var pipeline = pass.Pipeline;
                        BuildSurfaceShader(versionKey, shaderKey, ref pipeline, pass.BaseShader);
                        pass.Pipeline = pipeline;
                        shaderDesc.Passes[i] = pass;
                    }
                }
            }

            shaderDesc.Macros = desc.GetShaderMacros();

            shader.Update(shaderDesc);
            shader.Reload();
        }

        public static void BuildSurfaceShader(MetadataStringEntry version, MetadataStringEntry shader, ref GraphicsPipelineDescEx pipelineDesc, string baseShader)
        {
            if (version.Value == MaterialNodeConverter.SurfaceVersion)
            {
                StringBuilder sb = new();
                sb.AppendLine($"#include \"../../material.hlsl\"");
                sb.AppendLine($"#include \"../../geometry.hlsl\"");
                sb.AppendLine(shader.Value);
                sb.AppendLine(FileSystem.ReadAllText(Paths.CurrentShaderPath + baseShader));
                pipelineDesc.PixelShader = ShaderSource.FromCode(baseShader, sb.ToString());
            }
        }

        public static async Task<Resources.MaterialShader?> UpdateMaterialShaderAsync(this ResourceManager manager, Resources.MaterialShader? shader, MaterialShaderDesc desc)
        {
            if (shader == null)
            {
                return null;
            }

            shader.Update(desc);
            await shader.ReloadAsync();
            return shader;
        }

        public static async Task<Resources.MaterialShader?> UpdateMaterialShaderAsync(this ResourceManager manager, Resources.MaterialShader? shader, MaterialData material)
        {
            if (shader == null)
            {
                return null;
            }

            var shaderDesc = shader.Description;

            if (material.Metadata.TryGet<MetadataStringEntry>(MaterialNodeConverter.MetadataSurfaceVersionKey, out var versionKey) && material.Metadata.TryGet<MetadataStringEntry>(MaterialNodeConverter.MetadataSurfaceKey, out var shaderKey))
            {
                for (var i = 0; i < shaderDesc.Passes.Length; i++)
                {
                    var pass = shaderDesc.Passes[i];
                    if (pass.SurfaceShader && pass.BaseShader != null)
                    {
                        var pipeline = pass.Pipeline;
                        BuildSurfaceShader(versionKey, shaderKey, ref pipeline, pass.BaseShader);
                        pass.Pipeline = pipeline;
                        shaderDesc.Passes[i] = pass;
                    }
                }
            }

            shaderDesc.Macros = material.GetShaderMacros();

            shader.Update(shaderDesc);
            await shader.ReloadAsync();
            return shader;
        }

        public static void RecompileShaders(this ResourceManager manager)
        {
            var factory = manager.GetFactoryByResourceType<Resources.MaterialShader, MaterialShaderDesc>();
            if (factory == null)
            {
                return;
            }

            LoggerFactory.General.Info("recompiling material shaders ...");
            foreach (var shader in factory.Instances)
            {
                shader.Value?.Reload();
            }
            LoggerFactory.General.Info("recompiling material shaders ...  done!");
        }
    }
}