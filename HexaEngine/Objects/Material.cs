namespace HexaEngine.Objects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Scenes;
    using Newtonsoft.Json;
    using SharpGen.Runtime;
    using System;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public class Material : IDisposable
    {
        private Scene? scene;
        private IGraphicsDevice? device;
        private IBuffer? CB;
        private ISamplerState? samplerState;

        private bool disposedValue;
        private string albedoTextureMap = string.Empty;
        private string normalTextureMap = string.Empty;
        private string displacementTextureMap = string.Empty;
        private string roughnessTextureMap = string.Empty;
        private string metalnessTextureMap = string.Empty;
        private string emissiveTextureMap = string.Empty;
        private string aoTextureMap = string.Empty;

        public string Name { get; set; } = string.Empty;

        public Vector3 Albedo { get; set; }

        public float Opacity { get; set; }

        public float Roughness { get; set; }

        public float Metalness { get; set; }

        public float Ao { get; set; }

        public Vector3 Emissivness { get; set; }

        [JsonIgnore]
        public IShaderResourceView? AlbedoTexture { get; private set; }

        [JsonIgnore]
        public IShaderResourceView? NormalTexture { get; private set; }

        [JsonIgnore]
        public IShaderResourceView? DisplacementTexture { get; private set; }

        [JsonIgnore]
        public IShaderResourceView? RoughnessTexture { get; private set; }

        [JsonIgnore]
        public IShaderResourceView? MetalnessTexture { get; private set; }

        [JsonIgnore]
        public IShaderResourceView? EmissiveTexture { get; private set; }

        [JsonIgnore]
        public IShaderResourceView? AoTexture { get; private set; }

        public string AlbedoTextureMap
        {
            get => albedoTextureMap;
            set
            {
                albedoTextureMap = value;
                if (scene == null) return;
                if (!string.IsNullOrEmpty(AlbedoTextureMap))
                {
                    AlbedoTexture = scene.LoadTexture(Paths.CurrentTexturePath + AlbedoTextureMap);
                }
                else
                {
                    AlbedoTexture = null;
                }
            }
        }

        public string NormalTextureMap
        {
            get => normalTextureMap;
            set
            {
                normalTextureMap = value;
                if (scene == null) return;
                if (!string.IsNullOrEmpty(NormalTextureMap))
                {
                    NormalTexture = scene.LoadTexture(Paths.CurrentTexturePath + NormalTextureMap);
                }
                else
                {
                    NormalTexture = null;
                }
            }
        }

        public string DisplacementTextureMap
        {
            get => displacementTextureMap;
            set
            {
                displacementTextureMap = value;
                if (scene == null) return;
                if (!string.IsNullOrEmpty(DisplacementTextureMap))
                {
                    DisplacementTexture = scene.LoadTexture(Paths.CurrentTexturePath + DisplacementTextureMap);
                }
                else
                {
                    DisplacementTexture = null;
                }
            }
        }

        public string RoughnessTextureMap
        {
            get => roughnessTextureMap;
            set
            {
                roughnessTextureMap = value;
                if (scene == null) return;
                if (!string.IsNullOrEmpty(RoughnessTextureMap))
                {
                    RoughnessTexture = scene.LoadTexture(Paths.CurrentTexturePath + RoughnessTextureMap);
                }
                else
                {
                    RoughnessTexture = null;
                }
            }
        }

        public string MetalnessTextureMap
        {
            get => metalnessTextureMap;
            set
            {
                metalnessTextureMap = value;
                if (scene == null) return;
                if (!string.IsNullOrEmpty(MetalnessTextureMap))
                {
                    MetalnessTexture = scene.LoadTexture(Paths.CurrentTexturePath + MetalnessTextureMap);
                }
                else
                {
                    MetalnessTexture = null;
                }
            }
        }

        public string EmissiveTextureMap
        {
            get => emissiveTextureMap;
            set
            {
                emissiveTextureMap = value;
                if (scene == null) return;
                if (!string.IsNullOrEmpty(EmissiveTextureMap))
                {
                    EmissiveTexture = scene.LoadTexture(Paths.CurrentTexturePath + EmissiveTextureMap);
                }
                else
                {
                    EmissiveTexture = null;
                }
            }
        }

        public string AoTextureMap
        {
            get => aoTextureMap;
            set
            {
                aoTextureMap = value;
                if (scene == null) return;
                if (!string.IsNullOrEmpty(AoTextureMap))
                {
                    AoTexture = scene.LoadTexture(Paths.CurrentTexturePath + AoTextureMap);
                }
                else
                {
                    AoTexture = null;
                }
            }
        }

        public void Bind(IGraphicsContext context)
        {
            if (CB != null)
                context.Write(CB, new CBMaterial(this));
            context.SetConstantBuffer(CB, ShaderStage.Domain, 2);
            context.SetConstantBuffer(CB, ShaderStage.Pixel, 2);
            context.SetSampler(samplerState, ShaderStage.Pixel, 0);
            context.SetSampler(samplerState, ShaderStage.Domain, 0);
            context.SetShaderResource(AlbedoTexture, ShaderStage.Pixel, 0);
            context.SetShaderResource(NormalTexture, ShaderStage.Pixel, 1);
            context.SetShaderResource(RoughnessTexture, ShaderStage.Pixel, 2);
            context.SetShaderResource(MetalnessTexture, ShaderStage.Pixel, 3);
            context.SetShaderResource(EmissiveTexture, ShaderStage.Pixel, 4);
            context.SetShaderResource(AoTexture, ShaderStage.Pixel, 5);
            context.SetShaderResource(DisplacementTexture, ShaderStage.Domain, 0);
        }

        public void Initialize(Scene scene, IGraphicsDevice device)
        {
            this.scene = scene;
            this.device = device;
            CB = device.CreateBuffer(new CBMaterial(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            samplerState = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
            if (!string.IsNullOrEmpty(AlbedoTextureMap) && AlbedoTexture == null)
            {
                AlbedoTexture = scene.LoadTexture(Paths.CurrentTexturePath + AlbedoTextureMap);
            }
            if (!string.IsNullOrEmpty(NormalTextureMap) && NormalTexture == null)
            {
                NormalTexture = scene.LoadTexture(Paths.CurrentTexturePath + NormalTextureMap);
            }
            if (!string.IsNullOrEmpty(DisplacementTextureMap) && DisplacementTexture == null)
            {
                DisplacementTexture = scene.LoadTexture(Paths.CurrentTexturePath + DisplacementTextureMap);
            }
            if (!string.IsNullOrEmpty(RoughnessTextureMap) && RoughnessTexture == null)
            {
                RoughnessTexture = scene.LoadTexture(Paths.CurrentTexturePath + RoughnessTextureMap);
            }
            if (!string.IsNullOrEmpty(MetalnessTextureMap) && MetalnessTexture == null)
            {
                MetalnessTexture = scene.LoadTexture(Paths.CurrentTexturePath + MetalnessTextureMap);
            }
            if (!string.IsNullOrEmpty(EmissiveTextureMap) && EmissiveTexture == null)
            {
                EmissiveTexture = scene.LoadTexture(Paths.CurrentTexturePath + EmissiveTextureMap);
            }
            if (!string.IsNullOrEmpty(AoTextureMap) && AoTexture == null)
            {
                AoTexture = scene.LoadTexture(Paths.CurrentTexturePath + AoTextureMap);
            }
        }

        public static implicit operator CBMaterial(Material material)
        {
            return new(material);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                device = null;
                CB?.Dispose();
                samplerState?.Dispose();
                disposedValue = true;
            }
        }

        ~Material()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
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

            HasAlbedoMap = material.AlbedoTexture is not null ? 1 : 0;
            HasNormalMap = material.NormalTexture is not null ? 1 : 0;
            HasDisplacementMap = material.DisplacementTexture is not null ? 1 : 0;

            HasMetalnessMap = material.MetalnessTexture is not null ? 1 : 0;
            HasRoughnessMap = material.RoughnessTexture is not null ? 1 : 0;
            HasEmissiveMap = material.EmissiveTexture is not null ? 1 : 0;
            HasAoMap = material.AoTexture is not null ? 1 : 0;

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

        public RawBool HasDisplacementMap;
        public int HasAlbedoMap;
        public int HasNormalMap;
        public int HasRoughnessMap;
        public int HasMetalnessMap;
        public int HasEmissiveMap;
        public int HasAoMap;
        public int reserved;

        public CBMaterial(Material material)
        {
            Color = new(material.Albedo, 1);
            Emissive = new(material.Emissivness, 1);
            RMAo = new(material.Roughness, material.Metalness, material.Ao, 1);

            HasAlbedoMap = material.AlbedoTexture is not null ? 1 : 0;
            HasNormalMap = material.NormalTexture is not null ? 1 : 0;
            HasDisplacementMap = material.DisplacementTexture is not null;

            HasMetalnessMap = material.MetalnessTexture is not null ? 1 : 0;
            HasRoughnessMap = material.RoughnessTexture is not null ? 1 : 0;
            HasEmissiveMap = material.EmissiveTexture is not null ? 1 : 0;
            HasAoMap = material.AoTexture is not null ? 1 : 0;

            reserved = default;
        }
    }
}