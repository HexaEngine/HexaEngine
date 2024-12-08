﻿namespace HexaEngine.Components.Physics.Collider
{
    using Hexa.NET.Logging;
    using Hexa.NET.Utilities;
    using HexaEngine.Core;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Meshes;
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Physics;
    using HexaEngine.Scenes.Serialization;
    using MagicPhysX;
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    [OldName("HexaEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "HexaEngine.Components.Physics.Collider.ConvexMeshCollider")]
    [EditorCategory("Collider", "Physics")]
    [EditorComponent<ConvexMeshCollider>("Convex Mesh Collider")]
    public unsafe class ConvexMeshCollider : ColliderShape
    {
        private AssetRef model = default;

        private readonly List<Pointer<PxConvexMesh>> pxConvexMeshes = new();
        private Node[]? nodes;

        [EditorProperty("Model", AssetType.Model)]
        public AssetRef Model
        {
            get => model; set
            {
                model = value;
            }
        }

        [EditorButton("Cook")]
        public void CookButton()
        {
            CookShape(PhysicsSystem.PxPhysics, true);
        }

        private void CookShape(PxPhysics* physics, bool bypassCache = false)
        {
            string? path = this.model.GetPath();

            if (path == null)
            {
                return;
            }

            if (!File.Exists(path))
            {
                LoggerFactory.General.Error("Failed to load model, model file not found!");
                return;
            }

            ModelFile model;

            using Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                model = ModelFile.Load(stream, MeshLoadMode.Streaming);
            }
            catch (Exception ex)
            {
                LoggerFactory.General.Error($"Failed to load model, {ex}");
                return;
            }

            int nodesCount = 0;
            model.Root.CountNodes(ref nodesCount);
            nodes = new Node[nodesCount];
            int nodesIndex = 0;
            model.Root.FillNodes(nodes, ref nodesIndex);

            for (uint i = 0; i < model.Meshes.Count; i++)
            {
                var mesh = model.Meshes[(int)i];
                var key = $"{path}+{mesh.Name}+ConvexCook";
                var lod = mesh.LoadLODData(0, stream);
                CookMesh(physics, key, lod, bypassCache);
            }
        }

        public void CookMesh(PxPhysics* physics, string key, MeshLODData data, bool bypassCache)
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

                PxConvexMeshDesc desc = new();

                Vector3* points = AllocCopyT(data.Positions);
                uint* indices = AllocCopyT(data.Indices);

                desc.points.count = (uint)data.Positions.Length;
                desc.points.data = points;
                desc.points.stride = (uint)sizeof(Vector3);

                desc.indices.count = (uint)data.Indices.Length;
                desc.indices.data = indices;
                desc.indices.stride = sizeof(int);

                outputStream = (PxOutputStream*)NativeMethods.PxDefaultMemoryOutputStream_new_alloc((PxAllocatorCallback*)NativeMethods.get_default_allocator());

                PxConvexMeshCookingResult cookingResult;

                bool success = NativeMethods.phys_PxCookConvexMesh(&cookingParams, &desc, outputStream, &cookingResult);

                Free(points);
                Free(indices);

                if (!success)
                {
                    PhysicsSystem.Logger.Error("Failed to cook mesh");
                    return;
                }

                pData = NativeMethods.PxDefaultMemoryOutputStream_getData((PxDefaultMemoryOutputStream*)outputStream);
                dataSize = NativeMethods.PxDefaultMemoryOutputStream_getSize((PxDefaultMemoryOutputStream*)outputStream);

                PhysicsSystem.CookingCache.Set(key, pData, dataSize);
            }

            PxDefaultMemoryInputData* read = NativeMethods.PxDefaultMemoryInputData_new_alloc(pData, dataSize);

            PxConvexMesh* pxConvexMesh = physics->CreateConvexMeshMut((PxInputStream*)read);

            if (ownsBuffer)
            {
                Free(pData);
            }

            if (outputStream != null)
            {
                outputStream->Delete();
            }

            NativeMethods.PxInputData_delete((PxInputData*)read);

            if (pxConvexMesh == null)
            {
                PhysicsSystem.Logger.Error("PxTriangleMesh* is null");
                return;
            }

            pxConvexMeshes.Add(pxConvexMesh);
        }

        public override unsafe void AddShapes(PxPhysics* physics, PxScene* scene, PxRigidActor* actor, PxTransform localPose, Vector3 scale)
        {
            CookShape(physics);

            if (nodes == null)
            {
                return;
            }

            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                var nodeTransform = node.GetGlobalTransform();
                Matrix4x4.Decompose(nodeTransform, out var nodeScale, out var nodeRotation, out var nodeTranslation);
                for (int j = 0; j < node.Meshes.Count; j++)
                {
                    var meshIdx = node.Meshes[j];
                    var convexMesh = pxConvexMeshes[(int)meshIdx];

                    PxMeshScale pxMeshScale = new();
                    pxMeshScale.scale = nodeScale * scale;

                    var geometry = NativeMethods.PxConvexMeshGeometry_new(convexMesh, &pxMeshScale, 0);

                    var shape = physics->CreateShapeMut((PxGeometry*)&geometry, material, true, PxShapeFlags.Visualization | PxShapeFlags.SimulationShape | PxShapeFlags.SceneQueryShape);

                    PxTransform localPoseNode = new();
                    localPoseNode.q = nodeRotation * localPose.q;
                    localPoseNode.p = nodeTranslation * localPose.p;

                    AttachShape(actor, shape, localPoseNode);
                }
            }
        }

        public override void DestroyShapes()
        {
            base.DestroyShapes();
            for (int i = 0; i < pxConvexMeshes.Count; i++)
            {
                var mesh = pxConvexMeshes[i];
                mesh.Data->ReleaseMut();
            }
            pxConvexMeshes.Clear();
        }
    }
}