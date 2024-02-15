﻿namespace HexaEngine.Resources.Factories
{
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Meshes;
    using System.Threading.Tasks;

    public class MaterialResourceFactory : ResourceFactory<Material, (MeshData, MaterialData, bool)>
    {
        public MaterialResourceFactory(ResourceManager resourceManager) : base(resourceManager)
        {
        }

        protected override Material CreateInstance(ResourceManager manager, Guid id, (MeshData, MaterialData, bool) instanceData)
        {
            return new(this, instanceData.Item2);
        }

        protected override void LoadInstance(ResourceManager manager, Material instance, (MeshData, MaterialData, bool) instanceData)
        {
            (MeshData mesh, MaterialData desc, bool debone) = instanceData;
            instance.Shader = manager.LoadMaterialShader(mesh, desc, debone);
            for (int i = 0; i < desc.Textures.Count; i++)
            {
                var tex = desc.Textures[i];

                if (FilterTextureForPBRCookTorrance(tex.Type, desc.Textures))
                {
                    instance.TextureList.Add(manager.LoadTexture(desc.Textures[i]));
                }
            }

            instance.EndUpdate();
        }

        protected override async Task LoadInstanceAsync(ResourceManager manager, Material instance, (MeshData, MaterialData, bool) instanceData)
        {
            (MeshData mesh, MaterialData desc, bool debone) = instanceData;
            instance.Shader = await manager.LoadMaterialShaderAsync(mesh, desc, debone);
            for (int i = 0; i < desc.Textures.Count; i++)
            {
                var tex = desc.Textures[i];

                if (FilterTextureForPBRCookTorrance(tex.Type, desc.Textures))
                {
                    instance.TextureList.Add(await manager.LoadTextureAsync(desc.Textures[i]));
                }
            }

            instance.EndUpdate();
        }

        private static bool FilterTextureForPBRCookTorrance(MaterialTextureType type, List<MaterialTexture> textures)
        {
            if (!(type switch
            {
                MaterialTextureType.BaseColor => true,
                MaterialTextureType.Normal => true,
                MaterialTextureType.Metallic => true,
                MaterialTextureType.Roughness => true,
                MaterialTextureType.AmbientOcclusion => true,
                MaterialTextureType.RoughnessMetallic => true,
                MaterialTextureType.AmbientOcclusionRoughnessMetallic => true,
                _ => false,
            }))
            {
                return false;
            }

            for (int i = 0; i < textures.Count; i++)
            {
                var texture = textures[i];
                if (IsCombinationTextureOf(texture.Type, type))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsCombinationTextureOf(MaterialTextureType type, MaterialTextureType other)
        {
            if (type == MaterialTextureType.AmbientOcclusionRoughnessMetallic)
            {
                return other switch
                {
                    MaterialTextureType.Metallic => true,
                    MaterialTextureType.Roughness => true,
                    MaterialTextureType.AmbientOcclusion => true,
                    MaterialTextureType.RoughnessMetallic => true,
                    _ => false,
                };
            }

            if (type == MaterialTextureType.RoughnessMetallic)
            {
                return other switch
                {
                    MaterialTextureType.Metallic => true,
                    MaterialTextureType.Roughness => true,
                    _ => false,
                };
            }

            return false;
        }

        protected override void UnloadInstance(ResourceManager manager, Material instance)
        {
            for (int i = 0; i < instance.TextureList.Count; i++)
            {
                instance.TextureList[i]?.Dispose();
            }
            instance.Shader?.Dispose();
        }
    }
}