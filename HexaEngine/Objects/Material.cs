namespace HexaEngine.Objects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Graphics;
    using Newtonsoft.Json;
    using System;
    using System.Numerics;
    using System.Runtime.InteropServices;

    public class Material : IDisposable
    {
        [JsonIgnore]
        public IBuffer CB;

        private bool disposedValue;
        private string albedoTextureMap = string.Empty;
        private string normalTextureMap = string.Empty;
        private string displacementTextureMap = string.Empty;
        private string roughnessTextureMap = string.Empty;
        private string metalnessTextureMap = string.Empty;
        private string emissiveTextureMap = string.Empty;
        private string aoTextureMap = string.Empty;
        private ISamplerState samplerState;

        public string Name { get; set; } = string.Empty;

        public Vector3 Color { get; set; }

        public float Opacity { get; set; }

        public float Roughness { get; set; }

        public float Metalness { get; set; }

        public float Ao { get; set; }

        public Vector3 Emissivness { get; set; }

        [JsonIgnore]
        public Texture Albedo { get; private set; }

        [JsonIgnore]
        public Texture Normal { get; private set; }

        [JsonIgnore]
        public Texture Displacement { get; private set; }

        [JsonIgnore]
        public Texture RoughnessTexture { get; private set; }

        [JsonIgnore]
        public Texture MetalnessTexture { get; private set; }

        [JsonIgnore]
        public Texture EmissiveTexture { get; private set; }

        [JsonIgnore]
        public Texture AoTexture { get; private set; }

        public string AlbedoTextureMap
        {
            get => albedoTextureMap;
            set
            {
                albedoTextureMap = value;
            }
        }

        public string NormalTextureMap
        {
            get => normalTextureMap;
            set
            {
                normalTextureMap = value;
            }
        }

        public string DisplacementTextureMap
        {
            get => displacementTextureMap;
            set
            {
                displacementTextureMap = value;
            }
        }

        public string RoughnessTextureMap
        {
            get => roughnessTextureMap;
            set
            {
                roughnessTextureMap = value;
            }
        }

        public string MetalnessTextureMap
        {
            get => metalnessTextureMap;
            set
            {
                metalnessTextureMap = value;
            }
        }

        public string EmissiveTextureMap
        {
            get => emissiveTextureMap;
            set
            {
                emissiveTextureMap = value;
            }
        }

        public string AoTextureMap
        {
            get => aoTextureMap;
            set
            {
                aoTextureMap = value;
            }
        }

        public void Bind(IGraphicsContext context)
        {
            context.Write(CB, new CBMaterial(this));
            context.SetConstantBuffer(CB, ShaderStage.Domain, 2);
            context.SetConstantBuffer(CB, ShaderStage.Pixel, 2);
            context.SetSampler(samplerState, ShaderStage.Pixel, 0);
            context.SetSampler(samplerState, ShaderStage.Domain, 0);
            Albedo?.Bind(context, 0);
            Normal?.Bind(context, 1);
            RoughnessTexture?.Bind(context, 2);
            MetalnessTexture?.Bind(context, 3);
            EmissiveTexture?.Bind(context, 4);
            AoTexture?.Bind(context, 5);
            Displacement?.Bind(context, 0, ShaderStage.Domain);
        }

        public void Initialize(IGraphicsDevice device)
        {
            CB = device.CreateBuffer(new CBMaterial(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            samplerState = device.CreateSamplerState(SamplerDescription.AnisotropicClamp);
            if (!string.IsNullOrEmpty(AlbedoTextureMap) && Albedo == null)
            {
                Albedo = new Texture(device, new TextureFileDescription(Paths.CurrentTexturePath + AlbedoTextureMap));
            }
            if (!string.IsNullOrEmpty(NormalTextureMap) && Normal == null)
            {
                Normal = new Texture(device, new TextureFileDescription(Paths.CurrentTexturePath + NormalTextureMap));
            }
            if (!string.IsNullOrEmpty(DisplacementTextureMap) && Displacement == null)
            {
                Displacement = new Texture(device, new TextureFileDescription(Paths.CurrentTexturePath + DisplacementTextureMap));
            }
            if (!string.IsNullOrEmpty(RoughnessTextureMap) && RoughnessTexture == null)
            {
                RoughnessTexture = new Texture(device, new TextureFileDescription(Paths.CurrentTexturePath + RoughnessTextureMap));
            }
            if (!string.IsNullOrEmpty(MetalnessTextureMap) && MetalnessTexture == null)
            {
                MetalnessTexture = new Texture(device, new TextureFileDescription(Paths.CurrentTexturePath + MetalnessTextureMap));
            }
            if (!string.IsNullOrEmpty(EmissiveTextureMap) && EmissiveTexture == null)
            {
                EmissiveTexture = new Texture(device, new TextureFileDescription(Paths.CurrentTexturePath + EmissiveTextureMap));
            }
            if (!string.IsNullOrEmpty(AoTextureMap) && AoTexture == null)
            {
                AoTexture = new Texture(device, new TextureFileDescription(Paths.CurrentTexturePath + AoTextureMap));
            }
        }

        public static implicit operator CBMaterial(Material material)
        {
            return new()
            {
                Color = material.Color,
                Emissive = material.Emissivness,
                Metalness = material.Metalness,
                Roughness = material.Roughness,
                Ao = material.Ao,

                HasAlbedoMap = material.Albedo is not null ? 1 : 0,
                HasNormalMap = material.Normal is not null ? 1 : 0,
                HasDisplacementMap = material.Displacement is not null ? 1 : 0,

                HasMetalnessMap = material.MetalnessTexture is not null ? 1 : 0,
                HasRoughnessMap = material.RoughnessTexture is not null ? 1 : 0,
                HasEmissiveMap = material.EmissiveTexture is not null ? 1 : 0,
                HasAoMap = material.AoTexture is not null ? 1 : 0,
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                CB.Dispose();
                samplerState?.Dispose();
                Albedo?.Dispose();
                Normal?.Dispose();
                Displacement?.Dispose();
                MetalnessTexture?.Dispose();
                RoughnessTexture?.Dispose();
                EmissiveTexture?.Dispose();
                AoTexture?.Dispose();
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
    public struct CBMaterial
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

        public CBMaterial(Material material)
        {
            Color = material.Color;
            Emissive = material.Emissivness;
            Metalness = material.Metalness;
            Roughness = material.Roughness;
            Ao = material.Ao;

            HasAlbedoMap = material.Albedo is not null ? 1 : 0;
            HasNormalMap = material.Normal is not null ? 1 : 0;
            HasDisplacementMap = material.Displacement is not null ? 1 : 0;

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
}