﻿namespace HexaEngine.Scenes.Components.Renderer
{
    using HexaEngine.Core;
    using HexaEngine.Core.Editor.Attributes;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Materials;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Renderers;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Core.Scenes.Managers;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering;
    using System.Numerics;
    using System.Threading.Tasks;

    [EditorComponent(typeof(SkinnedMeshRendererComponent), "Skinned Mesh Renderer")]
    public class SkinnedMeshRendererComponent : IRendererComponent
    {
        private string modelPath = string.Empty;

        private GameObject? gameObject;
        private ModelManager modelManager;
        private MaterialManager materialManager;
        private SkinnedMeshRenderer renderer;
        private SkinnedModel? model;

        static SkinnedMeshRendererComponent()
        {
        }

        [EditorProperty("Model", null, "*.mesh")]
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
        public uint QueueIndex { get; } = (uint)RenderQueueIndex.Geometry;

        [JsonIgnore]
        public RendererFlags Flags { get; } = RendererFlags.All;

        [JsonIgnore]
        public BoundingBox BoundingBox { get => BoundingBox.Transform(model?.BoundingBox ?? BoundingBox.Empty, gameObject.Transform); }

        public void SetLocal(Matrix4x4 local, uint nodeId)
        {
            model?.SetLocal(local, nodeId);
            gameObject.SendUpdateTransformed();
        }

        public Matrix4x4 GetLocal(uint nodeId)
        {
            if (model == null)
                return Matrix4x4.Identity;
            return model.Locals[nodeId];
        }

        public void SetBoneLocal(Matrix4x4 local, uint boneId)
        {
            model?.SetBoneLocal(local, boneId);
            gameObject.SendUpdateTransformed();
        }

        public Matrix4x4 GetBoneLocal(uint boneId)
        {
            if (model == null)
                return Matrix4x4.Identity;
            return model.BoneLocals[boneId];
        }

        public int GetNodeIdByName(string name)
        {
            if (model == null)
                return -1;
            return model.GetNodeIdByName(name);
        }

        public int GetBoneIdByName(string name)
        {
            if (model == null)
                return -1;
            return model.GetBoneIdByName(name);
        }

        public void Awake(IGraphicsDevice device, GameObject gameObject)
        {
            this.gameObject = gameObject;

            renderer = new(device);

            modelManager = gameObject.GetScene().ModelManager;
            materialManager = gameObject.GetScene().MaterialManager;

            UpdateModel();
        }

        public void Destory()
        {
            renderer.Dispose();
            model?.Dispose();
        }

        public void Update(IGraphicsContext context)
        {
            renderer.Update(context, gameObject.Transform.Global);
        }

        public void DrawDepth(IGraphicsContext context)
        {
            renderer.DrawDepth(context);
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            renderer.DrawShadowMap(context, light, type);
        }

        public void VisibilityTest(IGraphicsContext context, Camera camera)
        {
        }

        public void Draw(IGraphicsContext context)
        {
            renderer.Draw(context);
        }

        private void UpdateModel()
        {
            Task.Factory.StartNew(async state =>
            {
                if (state is not SkinnedMeshRendererComponent component)
                {
                    return;
                }

                if (component.gameObject == null)
                {
                    return;
                }

                if (component.modelManager == null)
                {
                    return;
                }

                var tmpModel = component.model;
                component.model = null;
                tmpModel?.Dispose();

                var path = Paths.CurrentAssetsPath + component.modelPath;
                if (FileSystem.Exists(path))
                {
                    ModelFile source = component.modelManager.Load(path);
                    MaterialLibrary library = component.materialManager.Load(Paths.CurrentMaterialsPath + source.MaterialLibrary + ".matlib");

                    component.model = new(source, library);
                    await component.model.LoadAsync();
                    component.renderer.Initialize(component.model);

                    component.gameObject.SendUpdateTransformed();
                }
            }, this);
        }
    }
}