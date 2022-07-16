namespace HexaEngine.Objects
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.IO;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Rendering.ConstantBuffers;
    using HexaEngine.Scenes;
    using Newtonsoft.Json;
    using System.Linq;

    [EditorNode("Mesh")]
    public unsafe class Mesh : SceneNode
    {
#nullable disable

        /// <summary>
        /// Nullabillity is tested though <see cref="drawable"/>
        /// </summary>
        private IBuffer CB;

        /// <summary>
        /// Nullabillity is tested though <see cref="drawable"/>
        /// </summary>
        private IBuffer VB;

        /// <summary>
        /// Nullabillity is tested though <see cref="drawable"/>
        /// </summary>
        private IBuffer IB;

        /// <summary>
        /// Nullabillity is tested though <see cref="drawable"/>
        /// </summary>
        private Material material;

        public MeshFile MeshFile;
        public MeshVertex[] Vertices;
        public int[] Indices;

#nullable enable
        private int vertexCount;
        private int indexCount;

        private string meshPath = string.Empty;
        private string materialName = string.Empty;
        private bool drawable;

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

        [JsonIgnore]
        public bool Drawable => drawable;

        [JsonIgnore]
        public Material Material => material;

        public override void Initialize(IGraphicsDevice device)
        {
            GetScene().CommandQueue.Enqueue(new() { Type = CommandType.Load, Sender = this });
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
            GetScene().CommandQueue.Enqueue(new() { Type = CommandType.Unload, Sender = this });
            base.Uninitialize();
        }

        private void InitializeMesh()
        {
            if (!Initialized) return;
            if (FileSystem.Exists(Paths.CurrentModelPath + MeshPath))
            {
                VB?.Dispose();
                IB?.Dispose();
                var result = MeshFile = MeshFactory.Instance.Load(Paths.CurrentModelPath + MeshPath);
                Vertices = result.Vertices;
                Indices = result.Groups[0].Indices;
                VB = Device.CreateBuffer(Vertices, BindFlags.VertexBuffer);
                IB = Device.CreateBuffer(Indices, BindFlags.IndexBuffer);
                vertexCount = Vertices.Length;
                indexCount = Indices.Length;
            }
            GetScene().CommandQueue.Enqueue(new() { Type = CommandType.Update, Sender = this });
            drawable = CB != null && VB != null && IB != null && material != null;
        }

        private void InitializeMaterial()
        {
            if (!Initialized) return;
            Scene scene = GetScene();
            material = scene.Materials.FirstOrDefault(x => x.Name == materialName);
            GetScene().CommandQueue.Enqueue(new() { Type = CommandType.Update, Sender = this });
            drawable = CB != null && VB != null && IB != null && material != null;
        }

        public void DrawAuto(IGraphicsContext context, Pipeline pipeline, Viewport viewport)
        {
            if (!drawable) return;

            context.Write(CB, new CBWorld(this));
            context.SetConstantBuffer(CB, ShaderStage.Domain, 0);
            context.SetVertexBuffer(VB, sizeof(MeshVertex));
            if (IB != null)
            {
                context.SetIndexBuffer(IB, Format.R32UInt, 0);
                pipeline.DrawIndexed(context, viewport, indexCount, 0, 0);
            }
            else
            {
                pipeline.Draw(context, viewport, vertexCount, 0);
            }
        }
    }
}