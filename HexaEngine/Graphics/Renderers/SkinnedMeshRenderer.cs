namespace HexaEngine.Graphics.Renderers
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Lights;
    using Hexa.NET.Mathematics;
    using HexaEngine.Meshes;
    using HexaEngine.Resources;
    using System;
    using System.Numerics;

    public class SkinnedMeshRenderer : IDisposable
    {
        private bool initialized;

        private Matrix4x4[] globals;
        private Matrix4x4[] locals;
        private Matrix4x4[] boneGlobals;
        private Matrix4x4[] boneLocals;
        private PlainNode[] plainNodes;
        private PlainNode[] bones;
        private readonly ConstantBuffer<UPoint4> offsetBuffer;
        private readonly StructuredBuffer<Matrix4x4> transformBuffer;
        private readonly StructuredBuffer<uint> transformOffsetBuffer;
        private readonly StructuredBuffer<Matrix4x4> boneTransformBuffer;
        private readonly StructuredBuffer<uint> boneTransformOffsetBuffer;
        private int[][] drawables;
        private uint bufferOffset;
        private uint boneBufferOffset;
        private Mesh[] meshes;
        private Material[] materials;

        private readonly bool sharedBuffers;
        private bool disposedValue;

        public SkinnedMeshRenderer()
        {
            transformBuffer = new(CpuAccessFlags.Write);
            transformOffsetBuffer = new(CpuAccessFlags.Write);
            boneTransformBuffer = new(CpuAccessFlags.Write);
            boneTransformOffsetBuffer = new(CpuAccessFlags.Write);
            offsetBuffer = new(CpuAccessFlags.Write);
        }

        public SkinnedMeshRenderer(StructuredBuffer<Matrix4x4> transformBuffer, StructuredBuffer<uint> transformOffsetBuffer, StructuredBuffer<Matrix4x4> boneTransformBuffer, StructuredBuffer<uint> boneTransformOffsetBuffer)
        {
            this.transformBuffer = transformBuffer;
            this.transformOffsetBuffer = transformOffsetBuffer;
            this.boneTransformBuffer = boneTransformBuffer;
            this.boneTransformOffsetBuffer = boneTransformOffsetBuffer;
            offsetBuffer = new(CpuAccessFlags.Write);
            sharedBuffers = true;
        }

        public void Initialize(SkinnedModel model)
        {
            globals = model.Globals;
            locals = model.Locals;
            boneGlobals = model.BoneGlobals;
            boneLocals = model.BoneLocals;
            bones = model.Bones;
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
            boneGlobals = null;
            boneLocals = null;
            bones = null;
            plainNodes = null;
            drawables = null;
            bufferOffset = 0;
            meshes = null;
            materials = null;
        }

        private int GetBoneIdByName(string name)
        {
            if (bones == null)
            {
                return -1;
            }

            for (int i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];
                if (bone.Name == name)
                {
                    return bone.Id;
                }
            }
            return -1;
        }

        public void Update(IGraphicsContext context, Matrix4x4 transform)
        {
            if (!initialized)
            {
                return;
            }

            globals[0] = locals[0] * transform;

            for (int i = 1; i < plainNodes.Length; i++)
            {
                var node = plainNodes[i];
                globals[i] = locals[node.Id] * globals[node.ParentId];
            }

            if (!sharedBuffers)
            {
                transformBuffer.ResetCounter();
                transformOffsetBuffer.ResetCounter();
            }

            bufferOffset = transformOffsetBuffer.Count;

            for (int i = 0; i < drawables.Length; i++)
            {
                transformOffsetBuffer.Add(transformBuffer.Count);
                var drawable = drawables[i];
                if (drawable == null)
                {
                    continue;
                }

                for (int j = 0; j < drawable.Length; j++)
                {
                    var id = drawable[j];
                    transformBuffer.Add(Matrix4x4.Transpose(globals[id]));
                }
            }

            if (!sharedBuffers)
            {
                transformBuffer.Update(context);
                transformOffsetBuffer.Update(context);
            }

            if (bones == null)
            {
                return;
            }

            for (int i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];

                if (bone.ParentId == -1)
                {
                    boneGlobals[i] = boneLocals[bone.Id];
                }
                else
                {
                    boneGlobals[i] = boneLocals[bone.Id] * boneGlobals[bone.ParentId];
                }
            }

            if (!sharedBuffers)
            {
                boneTransformBuffer.ResetCounter();
                boneTransformOffsetBuffer.ResetCounter();
            }

            boneBufferOffset = boneTransformOffsetBuffer.Count;

            for (int i = 0; i < meshes.Length; i++)
            {
                boneTransformOffsetBuffer.Add(boneTransformBuffer.Count);
                var mesh = meshes[i];
                if (mesh == null)
                {
                    continue;
                }

                for (int j = 0; j < ((MeshData)mesh.Data).BoneCount; j++)
                {
                    var bone = ((MeshData)mesh.Data).Bones[j];

                    var id = GetBoneIdByName(bone.Name);
                    boneTransformBuffer.Add(Matrix4x4.Transpose(bone.Offset * boneGlobals[id]));
                }
            }

            if (!sharedBuffers)
            {
                boneTransformBuffer.Update(context);
                boneTransformOffsetBuffer.Update(context);
            }
        }

        public void DrawForward(IGraphicsContext context)
        {
            if (!initialized)
            {
                return;
            }

            uint boneOffset = 0;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);
            context.VSSetShaderResource(2, boneTransformBuffer.SRV);
            context.VSSetShaderResource(3, boneTransformOffsetBuffer.SRV);

            for (uint i = 0; i < drawables.Length; i++)
            {
                offsetBuffer.Update(context, new(bufferOffset + i, boneBufferOffset + boneOffset, 0, 0));

                var drawable = drawables[i];
                var mesh = meshes[i];
                var material = materials[i];

                if (mesh == null || material == null)
                {
                    continue;
                }

                mesh.BeginDraw(context);
                material.DrawIndexedInstanced(context, "Forward", mesh.IndexCount, (uint)drawable.Length);
                mesh.EndDraw(context);

                if (((MeshData)mesh.Data).BoneCount > 0)
                {
                    boneOffset++;
                }
            }

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
            context.VSSetShaderResource(2, null);
            context.VSSetShaderResource(3, null);
        }

        public void DrawDeferred(IGraphicsContext context)
        {
            if (!initialized)
            {
                return;
            }

            uint boneOffset = 0;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);
            context.VSSetShaderResource(2, boneTransformBuffer.SRV);
            context.VSSetShaderResource(3, boneTransformOffsetBuffer.SRV);

            for (uint i = 0; i < drawables.Length; i++)
            {
                offsetBuffer.Update(context, new(bufferOffset + i, boneBufferOffset + boneOffset, 0, 0));

                var drawable = drawables[i];
                var mesh = meshes[i];
                var material = materials[i];

                if (mesh == null || material == null)
                {
                    continue;
                }

                mesh.BeginDraw(context);
                material.DrawIndexedInstanced(context, "Deferred", mesh.IndexCount, (uint)drawable.Length);
                mesh.EndDraw(context);

                if (((MeshData)mesh.Data).BoneCount > 0)
                {
                    boneOffset++;
                }
            }

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
            context.VSSetShaderResource(2, null);
            context.VSSetShaderResource(3, null);
        }

        public void DrawDepth(IGraphicsContext context)
        {
            if (!initialized)
            {
                return;
            }

            uint boneOffset = 0;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);
            context.VSSetShaderResource(2, boneTransformBuffer.SRV);
            context.VSSetShaderResource(3, boneTransformOffsetBuffer.SRV);

            for (uint i = 0; i < drawables.Length; i++)
            {
                offsetBuffer.Update(context, new(bufferOffset + i, boneBufferOffset + boneOffset, 0, 0));

                var drawable = drawables[i];
                var mesh = meshes[i];
                var material = materials[i];

                if (mesh == null || material == null)
                {
                    continue;
                }

                mesh.BeginDraw(context);
                material.DrawIndexedInstanced(context, "DepthOnly", mesh.IndexCount, (uint)drawable.Length);
                mesh.EndDraw(context);

                if (((MeshData)mesh.Data).BoneCount > 0)
                {
                    boneOffset++;
                }
            }

            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
            context.VSSetShaderResource(2, null);
            context.VSSetShaderResource(3, null);
        }

        public void DrawShadowMap(IGraphicsContext context, IBuffer light, ShadowType type)
        {
            if (!initialized)
            {
                return;
            }

            uint boneOffset = 0;

            context.VSSetConstantBuffer(0, offsetBuffer);
            context.VSSetShaderResource(0, transformBuffer.SRV);
            context.VSSetShaderResource(1, transformOffsetBuffer.SRV);
            context.VSSetShaderResource(2, boneTransformBuffer.SRV);
            context.VSSetShaderResource(3, boneTransformOffsetBuffer.SRV);
            context.VSSetConstantBuffer(1, light);
            context.GSSetConstantBuffer(0, light);
            context.PSSetConstantBuffer(0, light);

            for (uint i = 0; i < drawables.Length; i++)
            {
                offsetBuffer.Update(context, new(bufferOffset + i, boneBufferOffset + boneOffset, 0, 0));

                var drawable = drawables[i];
                var mesh = meshes[i];
                var material = materials[i];

                if (mesh == null || material == null)
                {
                    continue;
                }

                mesh.BeginDraw(context);
                material.DrawIndexedInstanced(context, type.ToString(), mesh.IndexCount, (uint)drawable.Length);
                mesh.EndDraw(context);

                if (((MeshData)mesh.Data).BoneCount > 0)
                {
                    boneOffset++;
                }
            }

            context.PSSetConstantBuffer(0, null);
            context.VSSetConstantBuffer(1, null);
            context.GSSetConstantBuffer(0, null);
            context.VSSetConstantBuffer(0, null);
            context.VSSetShaderResource(0, null);
            context.VSSetShaderResource(1, null);
            context.VSSetShaderResource(2, null);
            context.VSSetShaderResource(3, null);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (!sharedBuffers)
                {
                    transformBuffer.Dispose();
                    transformOffsetBuffer.Dispose();
                    boneTransformBuffer.Dispose();
                    boneTransformOffsetBuffer.Dispose();
                }
                offsetBuffer.Dispose();
                Uninitialize();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}