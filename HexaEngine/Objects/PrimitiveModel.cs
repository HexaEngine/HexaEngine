namespace HexaEngine.Objects
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core;
    using HexaEngine.Core.Extensions;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Primitives;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Meshes;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using System.Numerics;

    public class PrimitiveModel : DisposableBase
    {
        private IPrimitive primitive;
        private Material material = null!;
        private MaterialData materialData;

        private BoundingBox boundingBox;
        private Matrix4x4[] instanceTransforms;
        private readonly DrawType drawType;

        public PrimitiveModel(IPrimitive primitive, MaterialData material, BoundingBox boundingBox, int instanceCount = 1)
        {
            this.primitive = primitive;
            materialData = material;
            this.boundingBox = boundingBox;

            instanceTransforms = new Matrix4x4[instanceCount];
            List<DrawInstance> drawInstances = new();
            for (int i = 0; i < instanceCount; i++)
            {
                drawInstances.Add(new(i));
                instanceTransforms[i] = Matrix4x4.Identity;
            }
            drawType = new(0, 0, drawInstances);

            LoadMaterial();
        }

        public IPrimitive Prim { get => primitive; set => primitive = value; }

        public DrawType DrawType => drawType;

        public int InstanceCount => drawType.Instances.Count;

        public Material Material => material;

        public MaterialData MaterialData
        {
            get => materialData; set
            {
                materialData = value;
                ReloadMaterial();
            }
        }

        public BoundingBox BoundingBox { get => boundingBox; set => boundingBox = value; }

        public Matrix4x4[] InstanceTransforms => instanceTransforms;

        public void SetInstanceCount(int count)
        {
            var current = InstanceCount;
            if (count < current)
            {
                for (int i = current - 1; i > count; i--)
                {
                    drawType.Instances.RemoveAt(i);
                }
                Array.Resize(ref instanceTransforms, count);
            }
            else
            {
                for (int i = current; i < count; i++)
                {
                    drawType.Instances.Add(new(i));
                }
                Array.Resize(ref instanceTransforms, count);
                Array.Fill(instanceTransforms, Matrix4x4.Identity, current, count - current);
            }
        }

        public void SetInstanceTransform(int instance, Matrix4x4 transform)
        {
            instanceTransforms[instance] = transform;
        }

        public Matrix4x4 GetInstanceTransform(int instance)
        {
            return instanceTransforms[instance];
        }

        public void ReloadMaterial()
        {
            material.Dispose();
            LoadMaterial();
        }

        private void LoadMaterial()
        {
            const VertexFlags vertexFlags = VertexFlags.Positions | VertexFlags.UVs | VertexFlags.Normals | VertexFlags.Tangents;
            UVChannelInfo channelInfo = new() { Channel0 = UVType.UV2D };
            materialData.Properties.Add(new MaterialProperty("TwoSided", MaterialPropertyType.TwoSided, MaterialValueType.Bool, Endianness.LittleEndian, 1, [1]));
            var shaderDesc = Model.GetMaterialShaderDesc(vertexFlags, GetShaderMacros(vertexFlags, channelInfo), GetInputElements(vertexFlags, channelInfo), materialData, false, out var _);
            material = ResourceManager.Shared.LoadMaterial<IPrimitive>(shaderDesc, materialData);
        }

        private unsafe InputElementDescription[] GetInputElements(VertexFlags flags, UVChannelInfo channelInfo)
        {
            List<InputElementDescription> inputElements = [];
            int offset = 0;
            if ((flags & VertexFlags.Colors) != 0)
            {
                inputElements.Add(new("COLOR", 0, Format.R32G32B32A32Float, offset, 0));
                offset += 16;
            }
            if ((flags & VertexFlags.Positions) != 0)
            {
                inputElements.Add(new("POSITION", 0, Format.R32G32B32Float, offset, 0));
                offset += 12;
            }
            if ((flags & VertexFlags.UVs) != 0)
            {
                var info = channelInfo;
                UVType* pType = (UVType*)&info;
                for (int i = 0; i < UVChannelInfo.MaxChannels; i++)
                {
                    switch (pType[i])
                    {
                        case UVType.Empty:
                            continue;

                        case UVType.UV2D:
                            inputElements.Add(new("TEXCOORD", 0, Format.R32G32Float, offset, i));
                            offset += 8;
                            break;

                        case UVType.UV3D:
                            inputElements.Add(new("TEXCOORD", 0, Format.R32G32B32Float, offset, i));
                            offset += 12;
                            break;

                        case UVType.UV4D:
                            inputElements.Add(new("TEXCOORD", 0, Format.R32G32B32A32Float, offset, i));
                            offset += 16;
                            break;
                    }
                }
            }
            if ((flags & VertexFlags.Normals) != 0)
            {
                inputElements.Add(new("NORMAL", 0, Format.R32G32B32Float, offset, 0));
                offset += 12;
            }
            if ((flags & VertexFlags.Tangents) != 0)
            {
                inputElements.Add(new("TANGENT", 0, Format.R32G32B32Float, offset, 0));
                offset += 12;
            }
            if ((flags & VertexFlags.Skinned) != 0)
            {
                inputElements.Add(new("BLENDINDICES", 0, Format.R32G32B32A32UInt, offset, 0));
                offset += 16;
                inputElements.Add(new("BLENDWEIGHT", 0, Format.R32G32B32A32Float, offset, 0));
                // offset += 16; // Commented out for potential future use
            }

            return [.. inputElements];
        }

        private unsafe ShaderMacro[] GetShaderMacros(VertexFlags flags, UVChannelInfo channelInfo)
        {
            List<ShaderMacro> macros = [];
            if ((flags & VertexFlags.Colors) != 0)
            {
                macros.Add(new("VtxColors", "1"));
            }
            if ((flags & VertexFlags.Positions) != 0)
            {
                macros.Add(new("VtxPos", "1"));
            }
            if ((flags & VertexFlags.UVs) != 0)
            {
                macros.Add(new("VtxUVs", "1"));  // TODO: UV channels, not supported yet by the file format.
                var info = channelInfo;
                UVType* pType = (UVType*)&info;
                for (int i = 0; i < UVChannelInfo.MaxChannels; i++)
                {
                    var type = pType[i];

                    if (type == UVType.Empty)
                        continue;

                    macros.Add(new($"VtxUVs{i}", "1"));
                    macros.Add(new($"VtxUV{i}Type", type.ToHLSL()));
                }
            }
            if ((flags & VertexFlags.Normals) != 0)
            {
                macros.Add(new("VtxNormals", "1"));
            }
            if ((flags & VertexFlags.Tangents) != 0)
            {
                macros.Add(new("VtxTangents", "1"));
            }
            if ((flags & VertexFlags.Skinned) != 0)
            {
                macros.Add(new("VtxSkinned", "1"));
            }
            return [.. macros];
        }

        protected override void DisposeCore()
        {
            primitive.Dispose();
            material.Dispose();
            materialData.Dispose();
        }
    }
}