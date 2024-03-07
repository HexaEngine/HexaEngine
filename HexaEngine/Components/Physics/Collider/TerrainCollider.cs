namespace HexaEngine.Components.Physics.Collider
{
    using HexaEngine.Components.Renderer;
    using HexaEngine.Core.Assets;
    using HexaEngine.Core.Debugging;
    using HexaEngine.Core.IO;
    using HexaEngine.Core.IO.Binary.Terrains;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Editor.Attributes;
    using MagicPhysX;
    using System.Numerics;

    [EditorCategory("Collider", "Physics")]
    [EditorComponent<TerrainCollider>("Terrain Collider", false, true)]
    public unsafe class TerrainCollider : ColliderShape
    {
        private readonly List<Pointer<PxHeightField>> pxHeightFields = [];
        private AssetRef terrainAsset;

        [EditorProperty("Terrain File", AssetType.Terrain)]
        public AssetRef TerrainAsset
        {
            get => terrainAsset;
            set
            {
                terrainAsset = value;
            }
        }

        public override unsafe void AddShapes(PxPhysics* physics, PxScene* scene, PxRigidActor* actor, PxTransform localPose, Vector3 scale)
        {
            TerrainFile terrain;
            ReusableFileStream? stream = null;

            try
            {
                stream = terrainAsset.OpenReadReusable();
                if (stream != null)
                {
                    terrain = TerrainFile.Load(stream, TerrainLoadMode.Streaming);
                }
                else
                {
                    Logger.Error($"Couldn't load terrain {terrainAsset}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex);
                Logger.Error($"Couldn't load terrain {terrainAsset}");
                stream?.Dispose();
                return;
            }

            for (int i = 0; i < terrain.Cells.Count; i++)
            {
                byte* pData;
                uint dataSize;

                var cell = terrain.Cells[i];

                cell.LoadHeightMapData(stream);

                float terrainWidth = 32;
                float terrainHeight = 32;

                var heightMap = cell.HeightMap;

                float minHeight = float.MaxValue;
                float maxHeight = float.MinValue;
                for (uint j = 0; j < heightMap.Width * heightMap.Height; j++)
                {
                    float height = heightMap[j];
                    minHeight = MathF.Min(height, minHeight);
                    maxHeight = MathF.Max(height, maxHeight);
                }

                float quantization = 0x7fff;

                float deltaHeight = maxHeight - minHeight;

                float heightScale = MathF.Max(deltaHeight / quantization, 0.0001f / 0xFFFF);

                PxHeightFieldSample* samples;
                ConvertHeightMap(heightMap, minHeight, deltaHeight, &samples);

                PxHeightFieldDesc desc = default;
                desc.format = PxHeightFieldFormat.S16Tm;
                desc.nbColumns = heightMap.Height;
                desc.nbRows = heightMap.Width;
                desc.samples.stride = (uint)sizeof(PxHeightFieldSample);
                desc.samples.data = samples;
                desc.flags = 0;

                PxOutputStream* outputStream = (PxOutputStream*)NativeMethods.PxDefaultMemoryOutputStream_new_alloc((PxAllocatorCallback*)NativeMethods.get_default_allocator());

                bool success = NativeMethods.phys_PxCookHeightField(&desc, outputStream);

                Free(samples);

                if (!success)
                {
                    Logger.Error($"Failed to cook height field, cell {cell.Position}");
                    continue;
                }

                pData = NativeMethods.PxDefaultMemoryOutputStream_getData((PxDefaultMemoryOutputStream*)outputStream);
                dataSize = NativeMethods.PxDefaultMemoryOutputStream_getSize((PxDefaultMemoryOutputStream*)outputStream);

                PxDefaultMemoryInputData* read = NativeMethods.PxDefaultMemoryInputData_new_alloc(pData, dataSize);

                PxHeightField* heightField = physics->CreateHeightFieldMut((PxInputStream*)read);

                if (outputStream != null)
                {
                    outputStream->Delete();
                }

                NativeMethods.PxInputData_delete((PxInputData*)read);

                if (heightField == null)
                {
                    Logger.Error($"PxHeightField* is null, cell {cell.Position}");
                    continue;
                }

                pxHeightFields.Add(heightField);

                PxHeightFieldGeometry geometry = NativeMethods.PxHeightFieldGeometry_new(heightField, 0, 0, 0, 0);
                geometry.columnScale = terrainHeight / (heightMap.Height - 1);
                geometry.rowScale = terrainWidth / (heightMap.Width - 1);
                geometry.heightScale = deltaHeight != 0 ? heightScale : 1;

                var shape = physics->CreateShapeMut((PxGeometry*)&geometry, material, true, PxShapeFlags.Visualization | PxShapeFlags.SimulationShape | PxShapeFlags.SceneQueryShape);

                PxTransform localPoseCell = new()
                {
                    q = localPose.q,
                    p = new Vector3(cell.Position.X * terrainWidth, minHeight, cell.Position.Y * terrainHeight) + localPose.p
                };

                AttachShape(actor, shape, localPoseCell);
            }

            stream.Close();
        }

        public override void DestroyShapes()
        {
            base.DestroyShapes();
            for (int i = 0; i < pxHeightFields.Count; i++)
            {
                var mesh = pxHeightFields[i];
                mesh.Data->ReleaseMut();
            }
            pxHeightFields.Clear();
        }

        public void ConvertHeightMap(HeightMap heightMap, float minHeight, float deltaHeight, PxHeightFieldSample** output)
        {
            bool userFlipEdge = true;
            PxHeightFieldSample* outputPtr = AllocT<PxHeightFieldSample>(heightMap.Width * heightMap.Height);
            float quantization = 0x7fff;
            for (uint col = 0; col < heightMap.Height; col++)
            {
                for (uint row = 0; row < heightMap.Width; row++)
                {
                    short height = (short)(quantization * ((heightMap[(col * heightMap.Width) + row] - minHeight) / deltaHeight));

                    PxHeightFieldSample* smp = &outputPtr[(row * heightMap.Width) + col];
                    smp->height = height;
                    smp->materialIndex0.structgen_pad0[0] = 0;
                    smp->materialIndex1.structgen_pad0[0] = 1;
                    if (userFlipEdge)
                        smp->SetTessFlagMut();
                }
            }

            *output = outputPtr;
        }
    }
}