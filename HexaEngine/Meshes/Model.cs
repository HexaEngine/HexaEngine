namespace HexaEngine.Meshes
{
    using HexaEngine.Components.Renderer;
    using HexaEngine.Configuration;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Jobs;
    using HexaEngine.Mathematics;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using System.Numerics;
    using System.Threading.Tasks;

    public class Model : IDisposable
    {
        private readonly ReusableFileStream stream;
        private readonly ModelFile modelFile;
        private readonly MaterialAssetMappingCollection materialAssets;

        private readonly Node[] nodes;
        private readonly Mesh[] meshes;
        private readonly Material[] materials;
        private readonly DrawType[] drawTypes;
        private readonly Matrix4x4[] locals;
        private readonly Matrix4x4[] globals;
        private readonly PlainNode[] plainNodes;
        private readonly int lodLevels;
        private BoundingBox boundingBox;

        private ModelMaterialShaderFlags shaderFlags;

        private bool loaded;

        private bool disposedValue;

        public Model(ReusableFileStream stream, MaterialAssetMappingCollection materialAssets)
        {
            this.stream = stream;
            this.materialAssets = materialAssets;

            modelFile = ModelFile.Load(stream, MeshLoadMode.Streaming);
            materialAssets.Update(modelFile);

            lodLevels = 5;

            int nodeCount = 0;
            modelFile.Root.CountNodes(ref nodeCount);
            nodes = new Node[nodeCount];
            int index = 0;
            modelFile.Root.FillNodes(nodes, ref index);

            globals = new Matrix4x4[nodeCount];
            locals = new Matrix4x4[nodeCount];
            index = 0;
            modelFile.Root.FillTransforms(locals, ref index);

            plainNodes = new PlainNode[nodeCount];
            index = 0;
            modelFile.Root.TraverseTree(plainNodes, ref index);

            materials = new Material[modelFile.Header.MeshCount];
            meshes = new Mesh[modelFile.Header.MeshCount];
            drawTypes = new DrawType[modelFile.Header.MeshCount];

            List<DrawInstance> meshDrawInstances = new();
            for (uint i = 0; i < modelFile.Header.MeshCount; i++)
            {
                MeshData mesh = modelFile.Meshes[(int)i];
                for (int j = 0; j < nodeCount; j++)
                {
                    Node node = nodes[j];
                    if (node.Meshes.Contains(i))
                    {
                        meshDrawInstances.Add(new(j));
                    }
                }

                drawTypes[i] = new(i, i, [.. meshDrawInstances]);
                meshDrawInstances.Clear();
            }
        }

        public ModelFile ModelFile => modelFile;

        public Node[] Nodes => nodes;

        public PlainNode[] PlainNodes => plainNodes;

        public Mesh[] Meshes => meshes;

        public Material[] Materials => materials;

        public DrawType[] DrawTypes => drawTypes;

        public Matrix4x4[] Locals => locals;

        public Matrix4x4[] Globals => globals;

        public BoundingBox BoundingBox => boundingBox;

        public ModelMaterialShaderFlags ShaderFlags => shaderFlags;

        public int LODLevels => lodLevels;

        public bool Loaded => loaded;

        public event Action? OnLoaded;

        public void ReloadMaterials()
        {
            if (meshes == null)
            {
                return;
            }
            shaderFlags = 0;
            for (int i = 0; i < meshes.Length; i++)
            {
                Mesh mesh = meshes[i];

                if (mesh == null)
                {
                    continue;
                }

                Material material = materials[i];
                var tmp = material;
                MaterialData materialDesc = materialAssets.GetMaterial(mesh.Data.Name);
                MaterialShaderDesc shaderDesc = GetMaterialShaderDesc(mesh, materialDesc, true, out ModelMaterialShaderFlags flags);
                material = ResourceManager.Shared.LoadMaterial<Model>(shaderDesc, materialDesc);
                materials[i] = material;
                tmp.Dispose();
                shaderFlags |= flags;
            }
            OnLoaded?.Invoke();
        }

        public void ReloadMaterial(MaterialAssetMapping mapping)
        {
            if (meshes == null)
            {
                return;
            }

            for (int i = 0; i < meshes.Length; i++)
            {
                Mesh mesh = meshes[i];

                if (mesh == null || mesh.Data.Name != mapping.Mesh)
                {
                    continue;
                }

                Material material = materials[i];

                if (material.Data.Guid == mapping.Material)
                {
                    continue;
                }

                var tmp = material;
                MaterialData materialDesc = materialAssets.GetMaterial(mesh.Data.Name);
                MaterialShaderDesc shaderDesc = GetMaterialShaderDesc(mesh, materialDesc, true, out ModelMaterialShaderFlags flags);
                material = ResourceManager.Shared.LoadMaterial<Model>(shaderDesc, materialDesc);
                materials[i] = material;
                tmp.Dispose();
                shaderFlags |= flags;
            }
            OnLoaded?.Invoke();
        }

        public void SetLOD(int level)
        {
            if (!stream.CanReOpen)
            {
                return;
            }

            Job.Run("LOD-Streaming Job", this, state =>
            {
                if (state is not Model model)
                {
                    return;
                }

                var stream = model.stream;
                lock (stream)
                {
                    stream.ReOpen();
                    BoundingBox boundingBox = BoundingBox.Empty;
                    for (int i = 0; i < model.modelFile.Meshes.Count; i++)
                    {
                        Mesh mesh = model.meshes[i];
                        var tmp = mesh;
                        MeshData data = model.modelFile.GetMesh(i);
                        MeshLODData lod = data.LoadLODData(level, stream);

                        mesh = ResourceManager.Shared.LoadMesh(data, lod);
                        model.meshes[i] = mesh;
                        tmp.Dispose();
                        model.boundingBox = BoundingBox.CreateMerged(boundingBox, mesh.BoundingBox);
                    }
                    model.boundingBox = boundingBox;
                    stream.Close();
                    OnLoaded?.Invoke();
                }
            }, ComputeLODJobPriority(level));
        }

        public JobPriority ComputeLODJobPriority(int level)
        {
            float s = level / (float)lodLevels;
            s = MathUtil.Clamp01(1 - s);
            return (JobPriority)(int)MathUtil.Lerp((float)JobPriority.Low, (float)JobPriority.Highest, s);
        }

        public void Load()
        {
            if (!stream.CanReOpen)
            {
                return;
            }

            lock (stream)
            {
                stream.ReOpen();
                for (int i = 0; i < modelFile.Meshes.Count; i++)
                {
                    MeshData data = modelFile.GetMesh(i);
                    MeshLODData lod = data.LoadLODData(0, stream);

                    Mesh mesh = ResourceManager.Shared.LoadMesh(data, lod);
                    MaterialData materialDesc = materialAssets.GetMaterial(data.Name);
                    MaterialShaderDesc shaderDesc = GetMaterialShaderDesc(mesh, materialDesc, true, out ModelMaterialShaderFlags flags);
                    Material material = ResourceManager.Shared.LoadMaterial<Model>(shaderDesc, materialDesc);

                    materials[i] = material;
                    meshes[i] = mesh;
                    boundingBox = BoundingBox.CreateMerged(boundingBox, mesh.BoundingBox);
                    shaderFlags |= flags;
                }
                stream.Close();
                OnLoaded?.Invoke();
            }

            loaded = true;
        }

        public async Task LoadAsync()
        {
            if (!stream.CanReOpen)
            {
                return;
            }

            stream.ReOpen();
            for (int i = 0; i < modelFile.Meshes.Count; i++)
            {
                MeshData data = modelFile.GetMesh(i);
                MeshLODData lod = data.LoadLODData(0, stream);

                Mesh mesh = await ResourceManager.Shared.LoadMeshAsync(data, lod);
                MaterialData materialDesc = materialAssets.GetMaterial(data.Name);
                MaterialShaderDesc shaderDesc = GetMaterialShaderDesc(mesh, materialDesc, true, out ModelMaterialShaderFlags flags);
                Material material = await ResourceManager.Shared.LoadMaterialAsync<Model>(shaderDesc, materialDesc);

                materials[i] = material;
                meshes[i] = mesh;
                boundingBox = BoundingBox.CreateMerged(boundingBox, mesh.BoundingBox);
                shaderFlags |= flags;
            }
            stream.Close();
            OnLoaded?.Invoke();

            loaded = true;
        }

        private static void Setup(MeshData mesh, MaterialData material, bool debone, out ModelMaterialShaderFlags flags, out ShaderMacro[] macros, out ShaderMacro[] shadowMacros, out MaterialFlags matflags, out bool custom, out bool twoSided, out bool alphaTest, out bool blendFunc, out bool tessellation)
        {
            flags = ModelMaterialShaderFlags.DepthTest | ModelMaterialShaderFlags.Deferred | ModelMaterialShaderFlags.Shadow | ModelMaterialShaderFlags.Bake;
            macros = [];
            shadowMacros = [new("SOFT_SHADOWS", (int)GraphicsSettings.SoftShadowMode)];

            if (debone! && (mesh.Flags & VertexFlags.Skinned) != 0)
            {
                flags &= ~ModelMaterialShaderFlags.Bake;
                macros = [.. macros, new ShaderMacro("VtxSkinned", "1")];
                shadowMacros = [.. shadowMacros, new ShaderMacro("VtxSkinned", "1")];
            }

            tessellation = false;
            if (material.HasTexture(MaterialTextureType.Displacement))
            {
                flags |= ModelMaterialShaderFlags.Tessellation;
                macros = [.. macros, new("Tessellation", "1")];
                tessellation = true;
            }

            matflags = material.Flags;
            custom = material.HasShader(MaterialShaderType.VertexShaderFile) && material.HasShader(MaterialShaderType.PixelShaderFile);
            twoSided = false;
            if (material.TryGetProperty(MaterialPropertyType.TwoSided, out MaterialProperty twosidedProp))
            {
                twoSided = twosidedProp.AsBool();
                if (twoSided)
                {
                    flags |= ModelMaterialShaderFlags.TwoSided;
                }
            }

            material.TryGetTexture(MaterialTextureType.Diffuse, out Core.IO.Binary.Materials.MaterialTexture textureDiffuse);
            material.TryGetTexture(MaterialTextureType.BaseColor, out Core.IO.Binary.Materials.MaterialTexture textureBaseColor);

            alphaTest = false;
            if ((textureDiffuse.Flags & TextureFlags.UseAlpha) != 0 || (textureBaseColor.Flags & TextureFlags.UseAlpha) != 0 || true)
            {
                flags |= ModelMaterialShaderFlags.AlphaTest;
                alphaTest = true;
            }

            blendFunc = false;
            if (material.TryGetProperty(MaterialPropertyType.BlendFunc, out MaterialProperty blendFuncProp))
            {
                blendFunc = blendFuncProp.AsBool();
                if (blendFunc)
                {
                    flags |= ModelMaterialShaderFlags.AlphaTest;
                    flags |= ModelMaterialShaderFlags.Transparent;
                    flags |= ModelMaterialShaderFlags.Forward;
                    flags &= ~ModelMaterialShaderFlags.Deferred;
                    flags &= ~ModelMaterialShaderFlags.DepthTest;
                    flags &= ~ModelMaterialShaderFlags.Shadow;
                    alphaTest = true;
                }
            }

            if ((material.Flags & MaterialFlags.Transparent) != 0)
            {
                flags |= ModelMaterialShaderFlags.AlphaTest;
                flags |= ModelMaterialShaderFlags.Transparent;
                flags |= ModelMaterialShaderFlags.Forward;
                flags &= ~ModelMaterialShaderFlags.Deferred;
                flags &= ~ModelMaterialShaderFlags.DepthTest;
                flags &= ~ModelMaterialShaderFlags.Shadow;
                blendFunc = true;
                alphaTest = true;
            }
        }

        private static List<MaterialShaderPassDesc> GetMaterialShaderPasses(MeshData mesh, MaterialData material, bool debone, out ModelMaterialShaderFlags flags)
        {
            Setup(mesh, material, debone, out flags, out ShaderMacro[] macros, out ShaderMacro[] shadowMacros, out MaterialFlags matflags, out bool custom, out bool twoSided, out bool alphaTest, out bool blendFunc, out bool tessellation);

            List<MaterialShaderPassDesc> passes = new();

            RasterizerDescription rasterizer = RasterizerDescription.CullBack;
            if (twoSided)
            {
                rasterizer = RasterizerDescription.CullNone;
            }

            BlendDescription blend = BlendDescription.Opaque;
            if (blendFunc)
            {
                blend = BlendDescription.AlphaBlend;
            }

            GraphicsPipelineDesc pipelineDescForward = new()
            {
                VertexShader = $"forward/geometry/vs.hlsl",
                PixelShader = $"forward/geometry/ps.hlsl",
                Macros = macros
            };

            GraphicsPipelineStateDesc pipelineStateDescForward = new()
            {
                DepthStencil = DepthStencilDescription.DepthReadEquals,
                Rasterizer = rasterizer,
                Blend = blend,
                Topology = PrimitiveTopology.TriangleList,
            };

            GraphicsPipelineDesc pipelineDescDeferred = new()
            {
                VertexShader = $"deferred/geometry/vs.hlsl",
                PixelShader = $"deferred/geometry/ps.hlsl",
                Macros = macros
            };

            GraphicsPipelineStateDesc pipelineStateDescDeferred = new()
            {
                DepthStencil = DepthStencilDescription.DepthReadEquals,
                Rasterizer = rasterizer,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            GraphicsPipelineDesc pipelineDescDepthOnly = new()
            {
                VertexShader = $"deferred/geometry/vs.hlsl",
                Macros = macros
            };

            GraphicsPipelineStateDesc pipelineStateDescDepthOnly = new()
            {
                DepthStencil = DepthStencilDescription.Default,
                Rasterizer = rasterizer,
                Blend = BlendDescription.Opaque,
                Topology = PrimitiveTopology.TriangleList,
            };

            if (tessellation)
            {
                pipelineDescForward.HullShader = $"forward/geometry/hs.hlsl";
                pipelineDescForward.DomainShader = $"forward/geometry/ds.hlsl";

                pipelineDescDeferred.HullShader = $"deferred/geometry/hs.hlsl";
                pipelineDescDeferred.DomainShader = $"deferred/geometry/ds.hlsl";

                pipelineDescDepthOnly.HullShader = $"deferred/geometry/hs.hlsl";
                pipelineDescDepthOnly.DomainShader = $"deferred/geometry/ds.hlsl";

                pipelineStateDescForward.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                pipelineStateDescDeferred.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                pipelineStateDescDepthOnly.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
            }

            if (alphaTest)
            {
                pipelineDescDepthOnly.PixelShader = $"deferred/geometry/ps.hlsl";
                pipelineDescDepthOnly.Macros = [.. macros, new("DEPTH_TEST_ONLY")];
            }

            if ((flags & ModelMaterialShaderFlags.Forward) != 0)
            {
                passes.Add(new("Forward", pipelineDescForward, pipelineStateDescForward));
            }

            if ((flags & ModelMaterialShaderFlags.Deferred) != 0)
            {
                passes.Add(new("Deferred", pipelineDescDeferred, pipelineStateDescDeferred));
            }

            if ((flags & ModelMaterialShaderFlags.DepthTest) != 0)
            {
                passes.Add(new("DepthOnly", pipelineDescDepthOnly, pipelineStateDescDepthOnly));
            }

            if ((flags & ModelMaterialShaderFlags.Shadow) != 0)
            {
                RasterizerDescription rasterizerShadow = RasterizerDescription.CullFront;
                if (twoSided)
                {
                    rasterizerShadow = RasterizerDescription.CullNone;
                }

                GraphicsPipelineDesc csmPipelineDesc = new()
                {
                    VertexShader = "forward/geometry/csm/vs.hlsl",
                    GeometryShader = "forward/geometry/csm/gs.hlsl",
                    PixelShader = "forward/geometry/csm/ps.hlsl",
                    Macros = shadowMacros
                };

                GraphicsPipelineStateDesc csmPipelineStateDesc = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullNone,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                };

                GraphicsPipelineDesc osmPipelineDesc = new()
                {
                    VertexShader = "forward/geometry/dpsm/vs.hlsl",
                    PixelShader = "forward/geometry/dpsm/ps.hlsl",
                    Macros = shadowMacros
                };

                GraphicsPipelineStateDesc osmPipelineStateDesc = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullNone,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                };

                GraphicsPipelineDesc psmPipelineDesc = new()
                {
                    VertexShader = "forward/geometry/psm/vs.hlsl",
                    PixelShader = "forward/geometry/psm/ps.hlsl",
                    Macros = shadowMacros
                };

                GraphicsPipelineStateDesc psmPipelineStateDesc = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = RasterizerDescription.CullNone,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                };

                passes.Add(new("Directional", csmPipelineDesc, csmPipelineStateDesc));
                passes.Add(new("Omnidirectional", osmPipelineDesc, osmPipelineStateDesc));
                passes.Add(new("Perspective", psmPipelineDesc, psmPipelineStateDesc));
            }

            return passes;
        }

        public static MaterialShaderDesc GetMaterialShaderDesc(Mesh mesh, MaterialData material, bool debone, out ModelMaterialShaderFlags flags)
        {
            List<MaterialShaderPassDesc> passes = GetMaterialShaderPasses(((MeshData)mesh.Data), material, debone, out flags);
            return new(material, mesh.InputElements, [.. passes]);
        }

        public void Unload()
        {
            shaderFlags = ModelMaterialShaderFlags.None;
            if (meshes != null)
            {
                for (int i = 0; i < meshes.Length; i++)
                {
                    meshes[i]?.Dispose();
                }
            }
            if (materials != null)
            {
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i]?.Dispose();
                }
            }

            loaded = false;
        }

        public void SetLocal(Matrix4x4 local, uint nodeId)
        {
            if (locals != null)
                locals[nodeId] = local;
        }

        public Matrix4x4 GetLocal(uint nodeId)
        {
            if (locals == null)
                return Matrix4x4.Identity;
            return locals[nodeId];
        }

        public int GetNodeIdByName(string name)
        {
            if (plainNodes == null)
                return -1;
            for (int i = 0; i < plainNodes.Length; i++)
            {
                PlainNode node = plainNodes[i];
                if (node.Name == name)
                    return node.Id;
            }
            return -1;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Unload();
                stream.Dispose();
                disposedValue = true;
            }
        }

        ~Model()
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
}