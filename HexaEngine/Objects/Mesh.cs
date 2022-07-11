namespace HexaEngine.Objects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Objects.Components;
    using HexaEngine.Rendering;
    using HexaEngine.Scenes;
    using HexaEngine.Shaders;
    using System.Linq;
    using System.Numerics;

    [EditorNode("Mesh")]
    public unsafe class Mesh : SceneNode
    {
        private IBuffer CB;
        private IBuffer VB;
        private IBuffer IB;
        private IBuffer ISB;
        private int vertexCount;
        private int indexCount;
        private int instanceCount;
        private Material material;
        private string meshPath = string.Empty;
        private string materialName;
        private bool drawable;

        public Mesh()
        {
            AddComponent(new MeshComponent());
        }

        [EditorProperty("Path")]
        public string MeshPath
        {
            get => meshPath;
            set
            {
                meshPath = value;
                InitializeMesh();
            }
        }

        [EditorProperty("Material name")]
        public string MaterialName
        {
            get => materialName;
            set
            {
                materialName = value;
                InitializeMaterial();
            }
        }

        public bool Drawable => drawable;

        public Material Material => material;

        public override void Initialize(IGraphicsDevice device)
        {
            CB = device.CreateBuffer(new CBWorld(), BindFlags.ConstantBuffer, Usage.Dynamic, CpuAccessFlags.Write);
            base.Initialize(device);
            InitializeMesh();
            InitializeMaterial();
        }

        public override void Uninitialize()
        {
            CB.Dispose();
            VB?.Dispose();
            IB?.Dispose();

            base.Uninitialize();
        }

        private void InitializeMesh()
        {
            if (!Initialized) return;
            if (FileSystem.Exists(Paths.CurrentModelPath + MeshPath))
            {
                VB?.Dispose();
                IB?.Dispose();
                var result = MeshFactory.Instance.Load(Paths.CurrentModelPath + MeshPath);
                var vertices = result.Vertices;
                var indices = result.Groups[0].Indices;
                VB = Device.CreateBuffer(vertices, BindFlags.VertexBuffer);
                IB = Device.CreateBuffer(indices, BindFlags.IndexBuffer);
                vertexCount = vertices.Length;
                indexCount = indices.Length;
            }

            drawable = VB != null && IB != null && material != null;
        }

        private void InitializeMaterial()
        {
            if (!Initialized) return;
            Scene scene = GetScene();
            material = scene.Materials.FirstOrDefault(x => x.Name == materialName);

            drawable = VB != null && IB != null && material != null;
        }

        public void DrawAuto(IGraphicsContext context, Pipeline pipeline, Viewport viewport, IView view)
        {
            if (!drawable) return;
            context.Write(CB, new CBWorld(this));
            context.SetConstantBuffer(CB, ShaderStage.Domain, 0);
            context.SetVertexBuffer(VB, sizeof(MeshVertex));
            if (IB != null)
            {
                context.SetIndexBuffer(IB, Format.R32UInt, 0);
                if (ISB != null)
                {
                    context.SetVertexBuffer(1, ISB, sizeof(MeshVertex));
                    pipeline.DrawIndexedInstanced(context, viewport, view, Transform, indexCount, instanceCount, 0, 0, 0);
                }
                else
                {
                    pipeline.DrawIndexed(context, viewport, view, Transform, indexCount, 0, 0);
                }
            }
            else
            {
                if (ISB != null)
                {
                    context.SetVertexBuffer(1, ISB, sizeof(MeshVertex));
                    pipeline.DrawInstanced(context, viewport, view, Transform, vertexCount, instanceCount, 0, 0);
                }
                else
                {
                    pipeline.Draw(context, viewport, view, Transform, vertexCount, 0);
                }
            }
        }

        [EditorComponent(typeof(MeshComponent), "MeshComponentInternal", true, true)]
        private unsafe class MeshComponent : IDeferredRendererComponent, IDepthRendererComponent
        {
            private Mesh mesh;
            private MTLShader shader;
            private MTLDepthShaderBack depthShaderBack;
            private MTLDepthShaderFront depthShaderFront;

            public void Initialize(IGraphicsDevice device, SceneNode node)
            {
                mesh = (Mesh)node;
                shader = new(device);
                depthShaderBack = new(device);
                depthShaderFront = new(device);
            }

            public void Render(IGraphicsContext context, Viewport viewport, IView view)
            {
                if (!mesh.drawable) return;
                context.SetVertexBuffer(mesh.VB, sizeof(MeshVertex));
                context.SetIndexBuffer(mesh.IB, Format.R32UInt, 0);
                mesh.material.Bind(context);
                shader.DrawIndexed(context, viewport, view, mesh.Transform, mesh.indexCount, 0, 0);
            }

            public void RenderDepthBackface(IGraphicsContext context, Viewport viewport, IView view)
            {
                if (!mesh.drawable) return;
                context.SetVertexBuffer(mesh.VB, sizeof(MeshVertex));
                context.SetIndexBuffer(mesh.IB, Format.R32UInt, 0);
                mesh.material.Bind(context);
                depthShaderBack.DrawIndexed(context, viewport, view, mesh.Transform, mesh.indexCount, 0, 0);
            }

            public void RenderDepthFrontface(IGraphicsContext context, Viewport viewport, IView view)
            {
                if (!mesh.drawable) return;
                context.SetVertexBuffer(mesh.VB, sizeof(MeshVertex));
                context.SetIndexBuffer(mesh.IB, Format.R32UInt, 0);
                mesh.material.Bind(context);
                depthShaderFront.DrawIndexed(context, viewport, view, mesh.Transform, mesh.indexCount, 0, 0);
            }

            public void Uninitialize()
            {
                shader.Dispose();
                depthShaderBack.Dispose();
                depthShaderFront.Dispose();
            }
        }
    }
}