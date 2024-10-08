using Hexa.NET.Logging;
using HexaEngine.Core.Assets;
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
        public AssetRef DiffuseMapAsset { get => diffuseMapAsset; set => SetAndNotifyWithEqualsTest(ref diffuseMapAsset, value); }

        [EditorProperty("Specular Map", AssetType.TextureCube)]
        public AssetRef SpecularMapAsset { get => specularMapAsset; set => SetAndNotifyWithEqualsTest(ref specularMapAsset, value); }

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
            UpdateDiffuseMap();
            UpdateSpecularMap();
        }

        private void UpdateSpecularMap()
        {
            var tmp = specularMap;
            specularMap = null;
            tmp?.Dispose();

            if (specularMapAsset == Guid.Empty)
            {
                return;
            }

            try
            {
                specularMap = Texture2D.LoadFromAssets(specularMapAsset, TextureDimension.TextureCube);
            }
            catch (Exception ex)
            {
                LoggerFactory.General.Error("Failed to load");
                LoggerFactory.General.Log(ex);
            }
        }

        private void UpdateDiffuseMap()
        {
            var tmp = diffuseMap;
            diffuseMap = null;
            tmp?.Dispose();

            if (diffuseMapAsset == Guid.Empty)
            {
                return;
            }

            try
            {
                diffuseMap = Texture2D.LoadFromAssets(diffuseMapAsset, TextureDimension.TextureCube);
            }
            catch (Exception ex)
            {
                LoggerFactory.General.Error("Failed to load");
                LoggerFactory.General.Log(ex);
            }
        }
    }
}