namespace HexaEngine.Components.Collider
{
    using HexaEngine.Core;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Meshes;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Physics;
    using MagicPhysX;
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    [EditorCategory("Collider")]
    [EditorComponent(typeof(MeshCollider), "Mesh Collider")]
    public unsafe class MeshCollider : BaseCollider
    {
        private string model = string.Empty;

        private readonly List<Pointer<PxTriangleMesh>> pxTriangleMeshes = new();
        private Node[]? nodes;

        [EditorProperty("Model", null, ".model")]
        public string Model
        { get => model; set { model = value; update = true; } }

        [EditorButton("Cook")]
        public void CookButton()
        {
            CookShape(true);
        }

        private void CookShape(bool bypassCache = false)
        {
            for (int i = 0; i < pxTriangleMeshes.Count; i++)
            {
                var mesh = pxTriangleMeshes[i];
                mesh.Data->ReleaseMut();
            }

            string path = Paths.CurrentAssetsPath + this.model;

            if (!FileSystem.Exists(path))
            {
                Logger.Error("Failed to load model, model file not found!");
                return;
            }

            ModelFile model;

            try
            {
                model = scene.ModelManager.Load(path);
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to load model, {ex}");
                return;
            }

            int nodesCount = 0;
            model.Root.CountNodes(ref nodesCount);
            nodes = new Node[nodesCount];
            int nodesIndex = 0;
            model.Root.FillNodes(nodes, ref nodesIndex);

            pxTriangleMeshes.Clear();

            for (uint i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[(int)i];
                var key = $"{path}+{mesh.Name}";
                CookMesh(key, mesh, bypassCache);
            }
        }

        public void CookMesh(string key, MeshData data, bool bypassCache)
        {
            bool ownsBuffer = true;

            byte* pData;
            uint dataSize;

            PxOutputStream* outputStream = null;
            if (bypassCache || !PhysicsSystem.CookingCache.TryGet(key, &pData, &dataSize))
            {
                ownsBuffer = false;
                PxCookingParams cookingParams = new();

                cookingParams.meshPreprocessParams = PxMeshPreprocessingFlags.WeldVertices;
                cookingParams.scale = new() { length = 100, speed = 981 };
                cookingParams.meshWeldTolerance = 1;

                PxTriangleMeshDesc desc = new();

                Vector3* points = AllocCopyT(data.Positions);
                uint* indices = AllocCopyT(data.Indices);

                desc.points.count = (uint)data.Positions.Length;
                desc.points.data = points;
                desc.points.stride = (uint)sizeof(Vector3);

                desc.triangles.count = (uint)data.Indices.Length / 3;
                desc.triangles.data = indices;
                desc.triangles.stride = (uint)sizeof(int) * 3;

                outputStream = (PxOutputStream*)NativeMethods.PxDefaultMemoryOutputStream_new_alloc((PxAllocatorCallback*)NativeMethods.get_default_allocator());

                PxTriangleMeshCookingResult cookingResult;

                bool success = NativeMethods.phys_PxCookTriangleMesh(&cookingParams, &desc, outputStream, &cookingResult);

                Free(points);
                Free(indices);

                if (!success)
                {
                    Logger.Error("Failed to cook mesh");
                    return;
                }

                pData = NativeMethods.PxDefaultMemoryOutputStream_getData((PxDefaultMemoryOutputStream*)outputStream);
                dataSize = NativeMethods.PxDefaultMemoryOutputStream_getSize((PxDefaultMemoryOutputStream*)outputStream);

                PhysicsSystem.CookingCache.Set(key, pData, dataSize);
            }

            PxDefaultMemoryInputData* read = NativeMethods.PxDefaultMemoryInputData_new_alloc(pData, dataSize);

            PxTriangleMesh* pxTriangleMesh = pxPhysics->CreateTriangleMeshMut((PxInputStream*)read);

            if (ownsBuffer)
            {
                Free(pData);
            }

            if (outputStream != null)
            {
                outputStream->Delete();
            }

            NativeMethods.PxInputData_delete((PxInputData*)read);

            if (pxTriangleMesh == null)
            {
                Logger.Error("PxTriangleMesh* is null");
                return;
            }

            pxTriangleMeshes.Add(pxTriangleMesh);
        }

        public override void CreateShape()
        {
        }

        public override unsafe void AddShapes(PxRigidActor* actor)
        {
            CookShape();

            if (nodes == null)
            {
                return;
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                var transform = node.GetGlobalTransform();
                Matrix4x4.Decompose(transform, out var scale, out var rotation, out var translation);
                for (int j = 0; j < node.Meshes.Count; j++)
                {
                    var meshIdx = node.Meshes[j];
                    var triangleMesh = pxTriangleMeshes[(int)meshIdx];

                    PxMeshScale pxMeshScale = new();
                    pxMeshScale.scale = scale;

                    var geometry = NativeMethods.PxTriangleMeshGeometry_new(triangleMesh, &pxMeshScale, 0);

                    var shape = pxPhysics->CreateShapeMut((PxGeometry*)&geometry, material, true, PxShapeFlags.Visualization | PxShapeFlags.SimulationShape | PxShapeFlags.SceneQueryShape);

                    PxTransform localPose = new();
                    localPose.q = rotation;
                    localPose.p = translation;

                    shape->SetLocalPoseMut(&localPose);

                    actor->AttachShapeMut(shape);
                    shape->ReleaseMut();
                }
            }
        }
    }
}