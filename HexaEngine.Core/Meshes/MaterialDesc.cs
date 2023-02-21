namespace HexaEngine.Core.Meshes
{
    using System.Numerics;

    public class MaterialDesc
    {
        public string NormalTextureMap = string.Empty;
        public string DisplacementTextureMap = string.Empty;
        public string BaseColorTextureMap = string.Empty;
        public string SpecularTextureMap = string.Empty;
        public string SpecularColorTextureMap = string.Empty;
        public string RoughnessTextureMap = string.Empty;
        public string MetalnessTextureMap = string.Empty;
        public string RoughnessMetalnessTextureMap = string.Empty;
        public string AoTextureMap = string.Empty;
        public string CleancoatTextureMap = string.Empty;
        public string CleancoatGlossTextureMap = string.Empty;
        public string SheenTextureMap = string.Empty;
        public string SheenTintTextureMap = string.Empty;
        public string AnisotropicTextureMap = string.Empty;
        public string SubsurfaceTextureMap = string.Empty;
        public string SubsurfaceColorTextureMap = string.Empty;
        public string EmissiveTextureMap = string.Empty;

        public Vector3 BaseColor;
        public float Opacity;
        public float Specular;
        public float SpecularTint;
        public Vector3 SpecularColor;
        public float Ao;
        public float Metalness;
        public float Roughness;
        public float Cleancoat;
        public float CleancoatGloss;
        public float Sheen;
        public float SheenTint;
        public float Anisotropic;
        public float Subsurface;
        public Vector3 SubsurfaceColor;
        public Vector3 Emissive;
        public string Name = string.Empty;

        public static MaterialDesc Default => new() { Name = "Default", Opacity = 1 };

        public MaterialDesc()
        {
        }
    }
}