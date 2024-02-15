namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO.Binary.Materials;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Renderers;
    using HexaEngine.Jobs;
    using HexaEngine.Lights;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Resources;
    using HexaEngine.Scenes.Managers;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Numerics;

    public class MaterialAssetMappingCollection : ICollection<MaterialAssetMapping>
    {
        private readonly List<MaterialAssetMapping> mappings = [];

        public int Count => mappings.Count;

        public bool IsReadOnly => false;

        public event Action<MaterialAssetMapping>? OnChanged;

        public MaterialAssetMapping this[int index]
        {
            get => mappings[index];
            set
            {
                mappings[index] = value;
                OnChanged?.Invoke(value);
            }
        }

        public MaterialAssetMapping Find(MeshData mesh)
        {
            for (int i = 0; i < mappings.Count; i++)
            {
                var mapping = mappings[i];
                if (mapping.Mesh == mesh.Name)
                    return mapping;
            }

            return default;
        }

        public MaterialAssetMapping Find(AssetRef material)
        {
            for (int i = 0; i < mappings.Count; i++)
            {
                var mapping = mappings[i];
                if (mapping.Material == material)
                    return mapping;
            }

            return default;
        }

        public MaterialData GetMaterial(MeshData data)
        {
            var mapping = Find(data);
            if (mapping.Material == AssetRef.Empty)
            {
                return MaterialData.Empty;
            }
            else
            {
                Artifact? artifact = ArtifactDatabase.GetArtifact(mapping.Material);
                if (artifact == null)
                {
                    Logger.Warn($"Failed to load material {mapping.Material}");
                    return MaterialData.Empty;
                }
                if (artifact.Type != AssetType.Material)
                {
                    Logger.Warn($"Failed to load material {mapping.Material}, asset was {artifact.Type} but needs to be {AssetType.Material}");
                    return MaterialData.Empty;
                }

                Stream? stream = null;

                try
                {
                    stream = artifact.OpenRead();
                    MaterialFile materialFile = MaterialFile.Read(stream);
                    return materialFile;
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                    Logger.Warn($"Failed to load material {mapping.Material}");
                    return MaterialData.Empty;
                }
                finally
                {
                    stream?.Dispose();
                }
            }
        }

        public void Add(MaterialAssetMapping item)
        {
            mappings.Add(item);
            OnChanged?.Invoke(item);
        }

        public void Clear()
        {
            mappings.Clear();
        }

        public bool Contains(MaterialAssetMapping item)
        {
            return mappings.Contains(item);
        }

        public void CopyTo(MaterialAssetMapping[] array, int arrayIndex)
        {
            mappings.CopyTo(array, arrayIndex);
        }

        public IEnumerator<MaterialAssetMapping> GetEnumerator()
        {
            return mappings.GetEnumerator();
        }

        public bool Remove(MaterialAssetMapping item)
        {
            OnChanged?.Invoke(item);
            return mappings.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mappings.GetEnumerator();
        }

        public void Update(ModelFile modelFile)
        {
            for (int i = 0; i < mappings.Count; i++)
            {
                var mapping = mappings[i];
                if (!ModelContainsMesh(modelFile, mapping.Mesh))
                {
                    mappings.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < modelFile.Meshes.Count; i++)
            {
                var mesh = modelFile.Meshes[i];
                if (!ContainsMesh(mappings, mesh.Name))
                {
                    mappings.Add(new(mesh, mesh.MaterialId));
                }
            }
        }

        private static bool ContainsMesh(List<MaterialAssetMapping> assetMappings, string meshName)
        {
            for (int i = 0; i < assetMappings.Count; i++)
            {
                var mapping = assetMappings[i];
                if (mapping.Mesh == meshName)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool ModelContainsMesh(ModelFile modelFile, string meshName)
        {
            for (int i = 0; i < modelFile.Meshes.Count; i++)
            {
                var mesh = modelFile.Meshes[i];
                if (mesh.Name == meshName)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public struct MaterialAssetMapping
    {
        public string Mesh;
        public AssetRef Material;

        public MaterialAssetMapping(MeshData mesh, AssetRef material)
        {
            Mesh = mesh.Name;
            Material = material;
        }
    }

    [EditorCategory("Renderer")]
    [EditorComponent(typeof(MeshRendererComponent), "Mesh Renderer")]
    public class MeshRendererComponent : BaseRendererComponent, ISelectableRayTest
    {
        private ModelManager modelManager;
        private MaterialManager materialManager;
        private MeshRenderer renderer;
        private Model? model;
        private AssetRef modelAsset;

        static MeshRendererComponent()
        {
        }

        [EditorProperty("Model", AssetType.Model)]
        public AssetRef Model
        {
            get => modelAsset;
            set
            {
                modelAsset = value;
                UpdateModel();
            }
        }

        [JsonIgnore]
        public override string DebugName { get; protected set; } = nameof(MeshRenderer);

        [JsonIgnore]
        public override RendererFlags Flags { get; } = RendererFlags.All | RendererFlags.Clustered | RendererFlags.Deferred | RendererFlags.Forward;

        [JsonIgnore]
        public override BoundingBox BoundingBox { get => BoundingBox.Transform(model?.BoundingBox ?? BoundingBox.Empty, GameObject?.Transform ?? Matrix4x4.Identity); }

        [JsonIgnore]
        public Matrix4x4 Transform => GameObject?.Transform ?? Matrix4x4.Identity;

        [JsonIgnore]
        public Model? ModelInstance => model;

        [EditorCategory("Materials")]
        [EditorProperty("")]
        public MaterialAssetMappingCollection Materials { get; } = new();

        public override void Load(IGraphicsDevice device)
        {
            modelManager = GameObject.GetScene().ModelManager;
            materialManager = GameObject.GetScene().MaterialManager;

            renderer = new(device);

            UpdateModel();
        }

        public override void Unload()
        {
            renderer.Dispose();
            model?.Dispose();
        }

        public override void Update(IGraphicsContext context)
        {
            renderer.Update(context, GameObject.Transform.Global);
        }

        public void DrawDepth(IGraphicsContext context, IBuffer cam)
        {
            renderer.DrawDepth(context, cam);
        }

        public override void DrawDepth(IGraphicsContext context)
        {
            renderer.DrawDepth(context);
        }

        public override void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            renderer.DrawShadowMap(context, light, type);
        }

        public override void VisibilityTest(CullingContext context)
        {
        }

        public override void Draw(IGraphicsContext context, RenderPath path)
        {
            if (path == RenderPath.Deferred)
            {
                renderer.DrawDeferred(context);
            }
            else
            {
                renderer.DrawForward(context);
            }
        }

        public override void Bake(IGraphicsContext context)
        {
            renderer.Bake(context);
        }

        public bool SelectRayTest(Ray ray, ref float depth)
        {
            if (model == null)
            {
                return false;
            }

            if (!BoundingBox.Intersects(ray).HasValue)
            {
                return false;
            }

            Ray objectSpaceRay = Ray.Transform(ray, GameObject.Transform.GlobalInverse);

            for (int i = 0; i < model.Meshes.Length; i++)
            {
                var mesh = model.Meshes[i];
                var result = mesh.Data.Intersect(objectSpaceRay);
                if (result != null)
                {
                    depth = result.Value;
                    return true;
                }
            }

            return false;
        }

        private void OnChanged(MaterialAssetMapping obj)
        {
            UpdateModel();
        }

        private Job UpdateModel()
        {
            Loaded = false;
            renderer?.Uninitialize();
            var tmpModel = model;
            model = null;
            tmpModel?.Dispose();

            Materials.OnChanged -= OnChanged;

            return Job.Run("Model Load Job", this, state =>
            {
                if (state is not MeshRendererComponent component)
                {
                    return;
                }

                if (component.GameObject == null)
                {
                    return;
                }

                if (component.modelManager == null)
                {
                    return;
                }

                var stream = modelAsset.OpenRead();
                if (stream != null)
                {
                    ModelFile modelFile;
                    try
                    {
                        modelFile = ModelFile.Load(stream);
                    }
                    finally
                    {
                        stream.Close();
                    }

                    component.Materials.Update(modelFile);

                    component.Materials.OnChanged += component.OnChanged;

                    component.model = new(modelFile, component.Materials);
                    component.model.LoadAsync().Wait();

                    var flags = component.model.ShaderFlags;
                    component.QueueIndex = (uint)RenderQueueIndex.Geometry;

                    if (flags.HasFlag(MaterialShaderFlags.AlphaTest))
                    {
                        component.QueueIndex = (uint)RenderQueueIndex.AlphaTest;
                    }

                    if (flags.HasFlag(MaterialShaderFlags.Transparent))
                    {
                        component.QueueIndex = (uint)RenderQueueIndex.Transparency;
                    }

                    component.renderer.Initialize(component.model);
                    component.Loaded = true;
                    component.GameObject.SendUpdateTransformed();
                }
                else
                {
                    Logger.Error($"Couldn't load model {modelAsset}");
                }
            }, JobPriority.Normal, JobFlags.BlockOnSceneLoad);
        }
    }
}