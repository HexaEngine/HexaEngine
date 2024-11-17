namespace HexaEngine.Graphics.Renderers
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Reflection;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Core.Utilities;
    using HexaEngine.Graphics.Culling;
    using HexaEngine.Graphics.Graph;
    using HexaEngine.Lights;
    using HexaEngine.Meshes;
    using HexaEngine.Resources;
    using Silk.NET.OpenAL;
    using System;
    using System.Numerics;

    public class SkinnedMeshRenderer : IDisposable
    {
        private ulong dirtyFrame;

        private bool initialized;

        private Matrix4x4[] globals = null!;
        private Matrix4x4[] locals = null!;
        private Matrix4x4[] boneGlobals = null!;
        private Matrix4x4[] boneLocals = null!;
        private PlainNode[] plainNodes = null!;
        private PlainNode[] bones = null!;
        private readonly ConstantBuffer<UPoint4> offsetBuffer = null!;
        private readonly StructuredBuffer<Matrix4x4> transformBuffer = null!;
        private readonly StructuredBuffer<uint> transformOffsetBuffer = null!;
        private readonly StructuredBuffer<Matrix4x4> boneTransformBuffer = null!;
        private readonly StructuredBuffer<uint> boneTransformOffsetBuffer = null!;
        private int[][] drawables = null!;
        private uint bufferOffset;
        private uint boneBufferOffset;
        private Mesh[] meshes = null!;
        private Material[] materials = null!;

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

            CullingManager.Current.BuffersResized += TransformBufferResize;

            initialized = true;
        }

        private void TransformBufferResize(object? sender, CapacityChangedEventArgs e)
        {
            dirtyFrame = Time.Frame;
        }

        public void Uninitialize()
        {
            initialized = false;

            globals = null!;
            locals = null!;
            boneGlobals = null!;
            boneLocals = null!;
            bones = null!;
            plainNodes = null!;
            drawables = null!;
            bufferOffset = 0;
            meshes = null!;
            materials = null!;
        }

        public void Prepare(SkinnedModel instance)
        {
            if (dirtyFrame == 0 || dirtyFrame == Time.Frame - 1)
            {
                foreach (Material material in instance.Materials)
                {
                    Prepare(material);
                }
            }
        }

        public void Prepare(Material material)
        {
            foreach (var pass in material.Shader.Passes)
            {
                var bindings = pass.Bindings;
                bindings.SetCBV("offsetBuffer", offsetBuffer);
                if (pass.Name == "Deferred" || pass.Name == "Forward")
                {
                    bindings.SetSRV("worldMatrices", transformBuffer.SRV!);
                    bindings.SetSRV("worldMatrixOffsets", transformOffsetBuffer.SRV!);
                }
                else
                {
                    bindings.SetSRV("worldMatrices", transformBuffer.SRV!);
                    bindings.SetSRV("worldMatrixOffsets", transformOffsetBuffer.SRV!);
                    //bindings.SetSRV("worldMatrices", transformNoBuffer.SRV);
                    //bindings.SetSRV("worldMatrixOffsets", transformNoOffsetBuffer.SRV);
                }

                bindings.SetSRV("boneMatrices", boneTransformBuffer.SRV);
                bindings.SetSRV("boneMatrixOffsets", boneTransformOffsetBuffer.SRV);
            }
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

        public void Update(IGraphicsContext context, Matrix4x4 transform, SkinnedModel model)
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
                    var bone = ((MeshData)mesh.Data).Bones![j];

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

        public void DrawForward(IGraphicsContext context, SkinnedModel model)
        {
            if (!initialized)
            {
                return;
            }

            uint boneOffset = 0;

            Prepare(model);

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
        }

        public void DrawDeferred(IGraphicsContext context, SkinnedModel model)
        {
            if (!initialized)
            {
                return;
            }

            uint boneOffset = 0;

            Prepare(model);

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
        }

        public void DrawDepth(IGraphicsContext context, SkinnedModel model)
        {
            if (!initialized)
            {
                return;
            }

            uint boneOffset = 0;

            Prepare(model);

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
        }

        public void DrawShadowMap(IGraphicsContext context, SkinnedModel model, IBuffer light, ShadowType type)
        {
            if (!initialized)
            {
                return;
            }

            uint boneOffset = 0;

            Prepare(model);

            string name = EnumHelper<ShadowType>.GetName(type);

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

                if (!material.BeginDraw(name, out var pass))
                {
                    continue;
                }

                pass.Bindings.SetCBV("lightBuffer", light);

                mesh.BeginDraw(context);
                if (pass.BeginDraw(context))
                {
                    context.DrawIndexedInstanced(mesh.IndexCount, (uint)drawable.Length, 0, 0, 0);
                }
                material.EndDraw(context);
                mesh.EndDraw(context);

                if (((MeshData)mesh.Data).BoneCount > 0)
                {
                    boneOffset++;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                transformBuffer.Resize -= TransformBufferResize;
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