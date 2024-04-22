using HexaEngine.Core.Assets;
using HexaEngine.Core.Debugging;
using HexaEngine.Core.Graphics;
using HexaEngine.Editor.Attributes;

namespace HexaEngine.Lights.Types
{
    [EditorCategory("Lights")]
    [EditorGameObject<IBLLight>("IBL Light")]
    public class IBLLight : LightSource
    {
        private AssetRef diffuseMapAsset;
        private AssetRef specularMapAsset;
        private Texture2D? diffuseMap;
        private Texture2D? specularMap;

        public override LightType LightType { get; } = LightType.IBL;

        [EditorProperty("Diffuse Map", AssetType.TextureCube)]
        public AssetRef DiffuseMapAsset { get => diffuseMapAsset; set => diffuseMapAsset = value; }

        [EditorProperty("Specular Map", AssetType.TextureCube)]
        public AssetRef SpecularMapAsset { get => specularMapAsset; set => specularMapAsset = value; }

        public Texture2D? DiffuseMap => diffuseMap;

        public Texture2D? SpecularMap => specularMap;

        public override void Initialize()
        {
            base.Initialize();
            UpdateTextures();
        }

        public override void Uninitialize()
        {
            diffuseMap?.Dispose();
            specularMap?.Dispose();
            base.Uninitialize();
        }

        private void UpdateTextures()
        {
            diffuseMap?.Dispose();
            specularMap?.Dispose();
            diffuseMap = null;
            specularMap = null;

            if (diffuseMapAsset == Guid.Empty)
            {
                return;
            }

            if (specularMapAsset == Guid.Empty)
            {
                return;
            }

            try
            {
                diffuseMap = Texture2D.LoadFromAssets(diffuseMapAsset, TextureDimension.TextureCube);
                specularMap = Texture2D.LoadFromAssets(specularMapAsset, TextureDimension.TextureCube);
            }
            catch (Exception ex)
            {
                LoggerFactory.General.Error("Failed to load");
                LoggerFactory.General.Log(ex);
            }
        }
    }
}