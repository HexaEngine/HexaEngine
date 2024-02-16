namespace HexaEngine.Meshes
{
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Mathematics;
    using HexaEngine.Resources;
    using HexaEngine.Resources.Factories;
    using System.Numerics;
    using System.Threading.Tasks;

    public class Model : IDisposable
    {
        private readonly ModelFile modelFile;
        private readonly MaterialAssetMappingCollection materialAssets;

        private readonly Node[] nodes;
        private readonly Mesh[] meshes;
        private readonly Material[] materials;
        private readonly int[][] drawables;
        private readonly Matrix4x4[] locals;
        private readonly Matrix4x4[] globals;
        private readonly PlainNode[] plainNodes;

        private BoundingBox boundingBox;

        private ModelMaterialShaderFlags shaderFlags;

        private bool loaded;

        private bool disposedValue;

        public Model(ModelFile modelFile, MaterialAssetMappingCollection materialAssets)
        {
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
            drawables = new int[modelFile.Header.MeshCount][];

            List<int> meshInstances = new();
            for (uint i = 0; i < modelFile.Header.MeshCount; i++)
            {
                for (int j = 0; j < nodeCount; j++)
                {
                    var node = nodes[j];
                    if (node.Meshes.Contains(i))
                    {
                        meshInstances.Add(j);
                    }
                }

                drawables[i] = meshInstances.ToArray();
                meshInstances.Clear();
            }

            this.modelFile = modelFile;
            this.materialAssets = materialAssets;
        }

        public ModelFile ModelFile => modelFile;

        public Node[] Nodes => nodes;

        public PlainNode[] PlainNodes => plainNodes;

        public Mesh[] Meshes => meshes;

        public Material[] Materials => materials;

        public int[][] Drawables => drawables;

        public Matrix4x4[] Locals => locals;

        public Matrix4x4[] Globals => globals;

        public BoundingBox BoundingBox => boundingBox;

        public ModelMaterialShaderFlags ShaderFlags => shaderFlags;

        public bool Loaded => loaded;

        public void Load()
        {
            for (int i = 0; i < modelFile.Meshes.Count; i++)
            {
                var data = modelFile.GetMesh(i);

                Mesh mesh = ResourceManager.Shared.LoadMesh(data, true);
                MaterialData materialDesc = materialAssets.GetMaterial(data);
                MaterialShaderDesc shaderDesc = GetMaterialShaderDesc(mesh, materialDesc, true, out var flags);
                Material material = ResourceManager.Shared.LoadMaterial(shaderDesc, materialAssets.GetMaterial(data));

                materials[i] = material;
                meshes[i] = mesh;
                boundingBox = BoundingBox.CreateMerged(boundingBox, mesh.BoundingBox);
                shaderFlags |= flags;
            }

            loaded = true;
        }

        public async Task LoadAsync()
        {
            for (int i = 0; i < modelFile.Meshes.Count; i++)
            {
                var data = modelFile.GetMesh(i);

                Mesh mesh = await ResourceManager.Shared.LoadMeshAsync(data, true);
                MaterialData materialDesc = materialAssets.GetMaterial(data);
                MaterialShaderDesc shaderDesc = GetMaterialShaderDesc(mesh, materialDesc, true, out var flags);
                Material material = await ResourceManager.Shared.LoadMaterialAsync(shaderDesc, materialDesc);

                materials[i] = material;
                meshes[i] = mesh;
                boundingBox = BoundingBox.CreateMerged(boundingBox, mesh.BoundingBox);
                shaderFlags |= flags;
            }

            loaded = true;
        }

        private static void Setup(MeshData mesh, MaterialData material, bool debone, out ModelMaterialShaderFlags flags, out ShaderMacro[] macros, out ShaderMacro[] shadowMacros, out MaterialFlags matflags, out bool custom, out bool twoSided, out bool alphaTest, out bool blendFunc)
        {
            flags = ModelMaterialShaderFlags.DepthTest | ModelMaterialShaderFlags.Deferred | ModelMaterialShaderFlags.Shadow | ModelMaterialShaderFlags.Bake;
            macros = [];
            shadowMacros = [];

            if (debone! && (mesh.Flags & VertexFlags.Skinned) != 0)
            {
                flags &= ~ModelMaterialShaderFlags.Bake;
                macros = [.. macros, new ShaderMacro("VtxSkinned", "1")];
                shadowMacros = [.. shadowMacros, new ShaderMacro("VtxSkinned", "1")];
            }

            matflags = material.Flags;
            custom = material.HasShader(MaterialShaderType.VertexShaderFile) && material.HasShader(MaterialShaderType.PixelShaderFile);
            twoSided = false;
            if (material.TryGetProperty(MaterialPropertyType.TwoSided, out var twosidedProp))
            {
                twoSided = twosidedProp.AsBool();
                if (twoSided)
                {
                    flags |= ModelMaterialShaderFlags.TwoSided;
                }
            }

            alphaTest = false;
            if ((material.Flags & MaterialFlags.AlphaTest) != 0)
            {
                flags |= ModelMaterialShaderFlags.AlphaTest;
                alphaTest = true;
            }

            blendFunc = false;
            if (material.TryGetProperty(MaterialPropertyType.BlendFunc, out var blendFuncProp))
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
            Setup(mesh, material, debone, out flags, out ShaderMacro[] macros, out ShaderMacro[] shadowMacros, out MaterialFlags matflags, out bool custom, out bool twoSided, out bool alphaTest, out bool blendFunc);

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
                Macros = [.. macros, new("CLUSTERED_FORWARD", 1)]
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

            GraphicsPipelineDesc pipelineDescBake = new()
            {
                VertexShader = $"forward/geometry/vs.hlsl",
                PixelShader = $"forward/geometry/ps.hlsl",
                Macros = [.. macros, new("CLUSTERED_FORWARD", 1), new("BAKE_PASS", 1)]
            };

            GraphicsPipelineStateDesc pipelineStateDescBake = new()
            {
                DepthStencil = DepthStencilDescription.DepthReadEquals,
                Rasterizer = rasterizer,
                Blend = blend,
                Topology = PrimitiveTopology.TriangleList,
            };

            if ((matflags & MaterialFlags.Tessellation) != 0)
            {
                flags |= ModelMaterialShaderFlags.Tessellation;
                Array.Resize(ref macros, macros.Length + 1);
                macros[^1] = new("Tessellation", "1");
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
                    Macros = shadowMacros
                };

                GraphicsPipelineStateDesc csmPipelineStateDesc = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = rasterizer,
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
                    Macros = shadowMacros
                };

                GraphicsPipelineStateDesc psmPipelineStateDesc = new()
                {
                    DepthStencil = DepthStencilDescription.Default,
                    Rasterizer = rasterizer,
                    Blend = BlendDescription.Opaque,
                    Topology = PrimitiveTopology.TriangleList,
                };

                if ((matflags & MaterialFlags.Tessellation) != 0)
                {
                    csmPipelineDesc.HullShader = "forward/geometry/csm/hs.hlsl";
                    csmPipelineDesc.DomainShader = "forward/geometry/csm/ds.hlsl";
                    csmPipelineStateDesc.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                    osmPipelineDesc.HullShader = "forward/geometry/dpsm/hs.hlsl";
                    osmPipelineDesc.DomainShader = "forward/geometry/dpsm/ds.hlsl";
                    osmPipelineStateDesc.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                    psmPipelineDesc.HullShader = "forward/geometry/psm/hs.hlsl";
                    psmPipelineDesc.DomainShader = "forward/geometry/psm/ds.hlsl";
                    psmPipelineStateDesc.Topology = PrimitiveTopology.PatchListWith3ControlPoints;
                }

                passes.Add(new("Directional", csmPipelineDesc, csmPipelineStateDesc));
                passes.Add(new("Omnidirectional", osmPipelineDesc, osmPipelineStateDesc));
                passes.Add(new("Perspective", psmPipelineDesc, psmPipelineStateDesc));
            }

            return passes;
        }

        public static MaterialShaderDesc GetMaterialShaderDesc(Mesh mesh, MaterialData material, bool debone, out ModelMaterialShaderFlags flags)
        {
            var passes = GetMaterialShaderPasses(mesh.Data, material, debone, out flags);
            return new(material, mesh.InputElements, [.. passes]);
        }

        public void Unload()
        {
            shaderFlags = ModelMaterialShaderFlags.None;
            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i].Dispose();
                materials[i].Dispose();
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
                var node = plainNodes[i];
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