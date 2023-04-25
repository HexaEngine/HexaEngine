namespace HexaEngine.Core.Instances
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Core.Graphics.Structs;
    using HexaEngine.Core.Lights;
    using HexaEngine.Core.Scenes;
    using HexaEngine.Mathematics;
    using System;
    using System.Collections.Generic;
    using System.Numerics;

    public interface IInstanceType : IDisposable, IEquatable<IInstanceType>
    {
        IBuffer ArgBuffer { get; }

        uint ArgBufferOffset { get; }

        int Count { get; }

        int IndexCount { get; }

        IReadOnlyList<IInstance> Instances { get; }

        bool IsEmpty { get; }

        int VertexCount { get; }

        int Visible { get; }

        bool Forward { get; }

        event Action<IInstanceType, IInstance> Updated;

        bool BeginDraw(IGraphicsContext context);

        bool BeginDrawForward(IGraphicsContext context);

        bool BeginDrawNoOcculusion(IGraphicsContext context);

        bool BeginDrawNoCulling(IGraphicsContext context);

        IInstance CreateInstance(GameObject parent);

        void DestroyInstance(IInstance instance);

        void Draw(IGraphicsContext context, IBuffer camera);

        void DrawShadow(IGraphicsContext context, IBuffer light, ShadowType type);

        void DrawNoOcclusion(IGraphicsContext context);

        void DrawDepth(IGraphicsContext context, IBuffer camera);

        string ToString();

        int UpdateFrustumInstanceBuffer(BoundingBox viewBox);

        int UpdateFrustumInstanceBuffer(BoundingFrustum frustum);

        int UpdateFrustumInstanceBuffer(BoundingFrustum[] frusta);

        int UpdateInstanceBuffer(uint id, StructuredBuffer<Matrix4x4> noCullBuffer, StructuredBuffer<InstanceData> buffer, StructuredUavBuffer<DrawIndexedInstancedIndirectArgs> drawArgs, BoundingFrustum frustum, bool doCulling);
    }
}