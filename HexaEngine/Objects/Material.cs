namespace HexaEngine.Objects
{
    using HexaEngine.Scenes;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public class Material
    {
        private Scene? scene;
        private string albedoTextureMap = string.Empty;
        private string normalTextureMap = string.Empty;
        private string displacementTextureMap = string.Empty;
        private string roughnessTextureMap = string.Empty;
        private string metalnessTextureMap = string.Empty;
        private string roughnessmetalnessTextureMap = string.Empty;
        private string emissiveTextureMap = string.Empty;
        private string aoTextureMap = string.Empty;
        private Vector3 emissivness;
        private float ao;
        private float metalness;
        private float roughness;
        private float opacity;
        private Vector3 albedo;

        public string Name { get; set; } = string.Empty;

        public Vector3 Albedo
        {
            get => albedo; set
            {
                albedo = value; if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public float Opacity
        {
            get => opacity; set
            {
                opacity = value; if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public float Roughness
        {
            get => roughness; set
            {
                roughness = value; if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public float Metalness
        {
            get => metalness; set
            {
                metalness = value; if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public float Ao
        {
            get => ao; set
            {
                ao = value; if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public Vector3 Emissivness
        {
            get => emissivness; set
            {
                emissivness = value; if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string AlbedoTextureMap
        {
            get => albedoTextureMap;
            set
            {
                albedoTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string NormalTextureMap
        {
            get => normalTextureMap;
            set
            {
                normalTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string DisplacementTextureMap
        {
            get => displacementTextureMap;
            set
            {
                displacementTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string RoughnessTextureMap
        {
            get => roughnessTextureMap;
            set
            {
                roughnessTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string MetalnessTextureMap
        {
            get => metalnessTextureMap;
            set
            {
                metalnessTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string RoughnessMetalnessTextureMap
        {
            get => roughnessmetalnessTextureMap;
            set
            {
                roughnessmetalnessTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string EmissiveTextureMap
        {
            get => emissiveTextureMap;
            set
            {
                emissiveTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string AoTextureMap
        {
            get => aoTextureMap;
            set
            {
                aoTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public void Initialize(Scene scene)
        {
            this.scene = scene;
        }

        public static implicit operator CBMaterial(Material material)
        {
            return new(material);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CBMaterialOld
    {
        public Vector3 Color;
        public float reserved1;
        public float Roughness;
        public Vector3 reserved2;
        public float Metalness;
        public Vector3 reserved3;
        public Vector3 Emissive;
        public float reserved4;
        public float Ao;
        public Vector3 reserved5;

        public int HasDisplacementMap;
        public int HasAlbedoMap;
        public int HasNormalMap;
        public int HasRoughnessMap;
        public int HasMetalnessMap;
        public int HasEmissiveMap;
        public int HasAoMap;
        public float reserved;

        public CBMaterialOld(Material material)
        {
            Color = material.Albedo;
            Emissive = material.Emissivness;
            Metalness = material.Metalness;
            Roughness = material.Roughness;
            Ao = material.Ao;

            HasAlbedoMap = string.IsNullOrEmpty(material.AlbedoTextureMap) ? 0 : 1;
            HasNormalMap = string.IsNullOrEmpty(material.NormalTextureMap) ? 0 : 1;
            HasDisplacementMap = string.IsNullOrEmpty(material.DisplacementTextureMap) ? 0 : 1;

            HasMetalnessMap = string.IsNullOrEmpty(material.MetalnessTextureMap) ? 0 : 1;
            HasRoughnessMap = string.IsNullOrEmpty(material.RoughnessTextureMap) ? 0 : 1;
            HasEmissiveMap = string.IsNullOrEmpty(material.EmissiveTextureMap) ? 0 : 1;
            HasAoMap = string.IsNullOrEmpty(material.AoTextureMap) ? 0 : 1;

            reserved = default;
            reserved1 = default;
            reserved2 = default;
            reserved3 = default;
            reserved4 = default;
            reserved5 = default;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CBMaterial
    {
        public Vector4 Color;
        public Vector4 RMAo;
        public Vector4 Emissive;

        public int HasDisplacementMap;
        public int HasAlbedoMap;
        public int HasNormalMap;
        public int HasRoughnessMap;
        public int HasMetalnessMap;
        public int HasEmissiveMap;
        public int HasAoMap;
        public int HasRoughnessMetalnessMap;

        public CBMaterial(Material material)
        {
            Color = new(material.Albedo, 1);
            Emissive = new(material.Emissivness, 1);
            RMAo = new(material.Roughness, material.Metalness, material.Ao, 1);

            HasAlbedoMap = string.IsNullOrEmpty(material.AlbedoTextureMap) ? 0 : 1;
            HasNormalMap = string.IsNullOrEmpty(material.NormalTextureMap) ? 0 : 1;
            HasDisplacementMap = string.IsNullOrEmpty(material.DisplacementTextureMap) ? 0 : 1;

            HasMetalnessMap = string.IsNullOrEmpty(material.MetalnessTextureMap) ? 0 : 1;
            HasRoughnessMap = string.IsNullOrEmpty(material.RoughnessTextureMap) ? 0 : 1;
            HasEmissiveMap = string.IsNullOrEmpty(material.EmissiveTextureMap) ? 0 : 1;
            HasAoMap = string.IsNullOrEmpty(material.AoTextureMap) ? 0 : 1;

            HasRoughnessMetalnessMap = string.IsNullOrEmpty(material.RoughnessMetalnessTextureMap) ? 0 : 1;
        }
    }
}