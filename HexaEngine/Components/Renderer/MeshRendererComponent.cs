﻿namespace HexaEngine.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Scenes;
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
    using System;
    using System.Collections;
    using System.Numerics;

    public struct MaterialMapping
    {
        public readonly MeshData Mesh;
        private MaterialData? material;

        public MaterialData? Material
        {
            readonly get => material; set
            {
                material = value;

                Mesh.MaterialName = material?.Name ?? string.Empty;
            }
        }

        public MaterialMapping(MeshData mesh, MaterialData material)
        {
            Mesh = mesh;
            this.material = material;
        }
    }

    public class MaterialMappingCollection : ICollection<MaterialMapping>
    {
        private readonly List<MaterialMapping> mappings = [];

        public int Count => mappings.Count;

        public bool IsReadOnly => false;

        public MaterialMapping this[int index]
        {
            get => mappings[index];
            set => mappings[index] = value;
        }

        public void Add(MaterialMapping item)
        {
            mappings.Add(item);
        }

        public void Clear()
        {
            mappings.Clear();
        }

        public bool Contains(MaterialMapping item)
        {
            return mappings.Contains(item);
        }

        public void CopyTo(MaterialMapping[] array, int arrayIndex)
        {
            mappings.CopyTo(array, arrayIndex);
        }

        public IEnumerator<MaterialMapping> GetEnumerator()
        {
            return mappings.GetEnumerator();
        }

        public bool Remove(MaterialMapping item)
        {
            return mappings.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return mappings.GetEnumerator();
        }
    }

    [EditorCategory("Renderer")]
    [EditorComponent(typeof(MeshRendererComponent), "Mesh Renderer")]
    public class MeshRendererComponent : BaseRendererComponent, ISelectableRayTest
    {
        private string modelPath = string.Empty;

        private ModelManager modelManager;
        private MaterialManager materialManager;
        private MeshRenderer renderer;
        private Model? model;

        static MeshRendererComponent()
        {
        }

        [EditorProperty("Model", startingPath: null, ".model")]
        public string Model
        {
            get => modelPath;
            set
            {
                modelPath = value;
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

        [EditorProperty("Materials")]
        [JsonIgnore]
        public MaterialMappingCollection Materials { get; } = new();

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

        private Job UpdateModel()
        {
            Loaded = false;
            renderer?.Uninitialize();
            var tmpModel = model;
            model = null;
            tmpModel?.Dispose();

            return Job.Run("Model Load Job", this, state =>
            {
                if (state is not MeshRendererComponent component)
                {
                    return;
                }

                component.Materials.Clear();

                if (component.GameObject == null)
                {
                    return;
                }

                if (component.modelManager == null)
                {
                    return;
                }

                var path = Paths.CurrentAssetsPath + component.modelPath;
                if (FileSystem.Exists(path))
                {
                    ModelFile source = component.modelManager.Load(path);
                    MaterialLibrary library = component.materialManager.Load(Paths.CurrentMaterialsPath + source.MaterialLibrary);

                    component.model = new(source, library);
                    component.model.LoadAsync().Wait();

                    for (int i = 0; i < component.model.Meshes.Length; i++)
                    {
                        var mesh = component.model.Meshes[i];
                        var material = component.model.Materials.FirstOrDefault(x => x.Name == mesh.Data.MaterialName);
                        MaterialMapping mapping = new(mesh.Data, material?.Data);
                        component.Materials.Add(mapping);
                    }

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
            }, JobPriority.Normal, JobFlags.BlockOnSceneLoad);
        }

        public void Hide(Mesh mesh)
        {
            throw new NotImplementedException();
        }
    }
}