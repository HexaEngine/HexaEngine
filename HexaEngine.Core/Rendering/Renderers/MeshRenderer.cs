namespace HexaEngine.Rendering
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Meshes;
    using HexaEngine.Core.Resources;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using System.Numerics;

    public class MeshRenderer : IDisposable
    {
        private readonly ResourceRef<IBuffer> camera;

        private bool initialized;

        private Matrix4x4[] globals;
        private Matrix4x4[] locals;
        private PlainNode[] plainNodes;
        private readonly ConstantBuffer<UPoint4> offsetBuffer;
        private readonly StructuredBuffer<Matrix4x4> transformBuffer;
        private readonly StructuredBuffer<uint> transformOffsetBuffer;
        private int[][] drawables;
        private uint bufferOffset;
        private Mesh[] meshes;
        private Material[] materials;

        private readonly bool sharedBuffers;
        private bool disposedValue;

        public MeshRenderer(IGraphicsDevice device)
        {
            transformBuffer = new(device, CpuAccessFlags.Write);
            transformOffsetBuffer = new(device, CpuAccessFlags.Write);
            offsetBuffer = new(device, CpuAccessFlags.Write);

            camera = ResourceManager2.Shared.GetBuffer("CBCamera");
        }

        public void Initialize(Model model)
        {
            globals = model.Globals;
            locals = model.Locals;
            plainNodes = model.PlainNodes;
            drawables = model.Drawables;
            meshes = model.Meshes;
            materials = model.Materials;

            initialized = true;
        }

        public void Uninitialize()
        {
            initialized = false;

            globals = null;
            locals = null;
            plainNodes = null;
            drawables = null;
            bufferOffset = 0;
            meshes = null;
            materials = null;
        }

        public void Update(IGraphicsContext context, Matrix4x4 transform)
        {
            if (!initialized)
                return;

            globals[0] = locals[0] * transform;

            for (int i = 1; i < plainNodes.Length; i++)
            {
                var node = plainNodes[i];
                globals[i] = locals[node.Id] * globals[node.ParentId];
            }

            if (!sharedBuffers)
            {
                transformOffsetBuffer.ResetCounter();
                transformBuffer.ResetCounter();
            }

            bufferOffset = transformOffsetBuffer.Count;

            for (int i = 0; i < drawables.Length; i++)
            {
                transformOffsetBuffer.Add(transformBuffer.Count);
                var drawable = drawables[i];
                if (drawable == null)
                    continue;

                BoundingBox mesh = meshes[i].BoundingBox;
                for (int j = 0; j < drawable.Length; j++)
                {
                    var id = drawable[j];
                    var global = globals[id];
                    var boundingBox = BoundingBox.Transform(mesh, global);
                    transformBuffer.Add(Matrix4x4.Transpose(global));
                }
            }

            if (!sharedBuffers)
            {
                transformOffsetBuffer.Update(context);
                transformBuffer.Update(context);
            }
        }

        public void VisibilityTest(IGraphicsContext context, Camera camera)
        {
            if (!initialized)
                return;
        }

        public void Draw(IGraphicsContext context)
        {
            if (!initialized)
                return;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

            for (uint i = 0; i < drawables.Length; i++)
            {
                offsetBuffer.Update(context, new(bufferOffset + i));

                int[] drawable = drawables[i];
                Mesh mesh = meshes[i];
                Material material = materials[i];

                if (mesh == null || material == null)
                    continue;

                mesh.BeginDraw(context);
                material.Draw(context, camera.Value, mesh.IndexCount, (uint)drawable.Length);
                mesh.EndDraw(context);
            }

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, (IShaderResourceView?)null);
            context.VSSetShaderResource(1, (IShaderResourceView?)null);
        }

        public void DrawDepth(IGraphicsContext context)
        {
            if (!initialized)
                return;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

            for (uint i = 0; i < drawables.Length; i++)
            {
                offsetBuffer.Update(context, new(bufferOffset + i));

                int[] drawable = drawables[i];
                Mesh mesh = meshes[i];
                Material material = materials[i];

                if (mesh == null || material == null)
                    continue;

                mesh.BeginDraw(context);
                material.DrawDepth(context, camera.Value, mesh.IndexCount, (uint)drawable.Length);
                mesh.EndDraw(context);
            }

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, (IShaderResourceView?)null);
            context.VSSetShaderResource(1, (IShaderResourceView?)null);
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (!initialized)
                return;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);

            for (uint i = 0; i < drawables.Length; i++)
            {
                offsetBuffer.Update(context, new(bufferOffset + i));

                int[] drawable = drawables[i];
                Mesh mesh = meshes[i];
                Material material = materials[i];

                if (mesh == null || material == null)
                    continue;

                mesh.BeginDraw(context);
                material.DrawShadow(context, light, type, mesh.IndexCount, (uint)drawable.Length);
                mesh.EndDraw(context);
            }

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, (IShaderResourceView?)null);
            context.VSSetShaderResource(1, (IShaderResourceView?)null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (!sharedBuffers)
                {
                    transformBuffer.Dispose();
                    transformOffsetBuffer.Dispose();
                }
                offsetBuffer.Dispose();
                Uninitialize();
                disposedValue = true;
            }
        }

        ~MeshRenderer()
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