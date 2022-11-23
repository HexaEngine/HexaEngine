namespace HexaEngine.Objects
{
    using HexaEngine.Scenes;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public class Material
    {
        private Scene? scene;
        private string normalTextureMap = string.Empty;
        private string displacementTextureMap = string.Empty;
        private string baseColorTextureMap = string.Empty;
        private string specularTextureMap = string.Empty;
        private string specularColorTextureMap = string.Empty;
        private string roughnessTextureMap = string.Empty;
        private string metalnessTextureMap = string.Empty;
        private string roughnessmetalnessTextureMap = string.Empty;
        private string aoTextureMap = string.Empty;
        private string cleancoatTextureMap = string.Empty;
        private string cleancoatGlossTextureMap = string.Empty;
        private string sheenTextureMap = string.Empty;
        private string sheenTintTextureMap = string.Empty;
        private string anisotropicTextureMap = string.Empty;
        private string subsurfaceTextureMap = string.Empty;
        private string subsurfaceColorTextureMap = string.Empty;
        private string emissivenessTextureMap = string.Empty;

        private Vector3 baseColor;
        private float opacity;
        private float specular;
        private float specularTint;
        private Vector3 specularColor;
        private float ao;
        private float metalness;
        private float roughness;
        private float cleancoat;
        private float cleancoatGloss;
        private float sheen;
        private float sheenTint;
        private float anisotropic;
        private float subsurface;
        private Vector3 subsurfaceColor;
        private Vector3 emissivness;

        public string Name { get; set; } = string.Empty;

        public Vector3 BaseColor
        {
            get => baseColor; set
            {
                baseColor = value; if (scene == null) return;
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

        public float Specular
        {
            get => specular; set
            {
                specular = value; if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public float SpecularTint
        {
            get => specularTint; set
            {
                specularTint = value; if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public Vector3 SpecularColor
        {
            get => specularColor; set
            {
                specularColor = value; if (scene == null) return;
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

        public float Cleancoat
        {
            get => cleancoat; set
            {
                cleancoat = value; if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public float CleancoatGloss
        {
            get => cleancoatGloss; set
            {
                cleancoatGloss = value; if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public float Sheen
        {
            get => sheen; set
            {
                cleancoat = value; if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public float SheenTint
        {
            get => sheenTint; set
            {
                sheenTint = value; if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public float Anisotropic
        {
            get => anisotropic; set
            {
                anisotropic = value; if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public float Subsurface
        {
            get => subsurface; set
            {
                subsurface = value; if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public Vector3 SubsurfaceColor
        {
            get => subsurfaceColor; set
            {
                subsurfaceColor = value; if (scene == null) return;
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

        public string BaseColorTextureMap
        {
            get => baseColorTextureMap;
            set
            {
                baseColorTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string SpecularTextureMap
        {
            get => specularTextureMap;
            set
            {
                specularTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string SpecularColorTextureMap
        {
            get => specularColorTextureMap;
            set
            {
                specularColorTextureMap = value;
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

        public string CleancoatTextureMap
        {
            get => cleancoatTextureMap;
            set
            {
                cleancoatTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string CleancoatGlossTextureMap
        {
            get => cleancoatGlossTextureMap;
            set
            {
                cleancoatGlossTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string SheenTextureMap
        {
            get => sheenTextureMap;
            set
            {
                cleancoatTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string SheenTintTextureMap
        {
            get => sheenTintTextureMap;
            set
            {
                cleancoatGlossTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string AnisotropicTextureMap
        {
            get => anisotropicTextureMap;
            set
            {
                anisotropicTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string SubsurfaceTextureMap
        {
            get => subsurfaceTextureMap;
            set
            {
                subsurfaceTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string SubsurfaceColorTextureMap
        {
            get => subsurfaceColorTextureMap;
            set
            {
                subsurfaceColorTextureMap = value;
                if (scene == null) return;
                scene.CommandQueue.Enqueue(new(CommandType.Update, this));
            }
        }

        public string EmissiveTextureMap
        {
            get => emissivenessTextureMap;
            set
            {
                emissivenessTextureMap = value;
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
    public struct CBMaterial
    {
        public Vector4 Color;
        public Vector4 RMAo;
        public Vector4 Emissive;
        public Vector4 Anisotropic;

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
            Color = new(material.BaseColor, 1);
            Emissive = new(material.Emissivness, 1);
            RMAo = new(material.Roughness, material.Metalness, material.Ao, 1);
            Anisotropic = new(material.Anisotropic, 0, 0, 0);

            HasAlbedoMap = string.IsNullOrEmpty(material.BaseColorTextureMap) ? 0 : 1;
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