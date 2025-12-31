namespace HexaEngine.Editor
{
    using Hexa.NET.Mathematics;
    using HexaEngine.Core;
    using HexaEngine.Core.Editor;
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Graphics.Buffers;
    using HexaEngine.Graphics.Overlays;
    using HexaEngine.Scenes;
    using System.Numerics;

    public class EditorSelectionOverlay : IOverlay
    {
        private MaskContext maskContext = null!;
        private Texture2D? maskBuffer;
        private IGraphicsPipelineState psoEdge = null!;
        private ConstantBuffer<OutlineParams> outlineParamsBuffer = null!;

        public struct OutlineParams
        {
            public Vector2 TexelSize;
            public float EdgeScale;
            public float Padding;
            public Vector4 OutlineColor;
            public Vector4 FillColor;

            public OutlineParams(Vector2 texelSize, float edgeScale, Vector4 outlineColor, Vector4 fillColor)
            {
                TexelSize = texelSize;
                EdgeScale = edgeScale;
                OutlineColor = outlineColor;
                FillColor = fillColor;
            }
        }

        public int ZIndex { get; } = 0;

        public void Draw(IGraphicsContext context, Viewport viewport, Texture2D target, DepthStencil depthStencil)
        {
            if (maskBuffer == null)
            {
                maskBuffer = new(Format.R8UInt, target.Width, target.Height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.All);
            }
            else if (maskBuffer.Width != target.Width || maskBuffer.Height != target.Height)
            {
                maskBuffer.Resize(Format.R8UInt, target.Width, target.Height, 1, 1, CpuAccessFlags.None, GpuAccessFlags.All);
            }

            using var scope = context.BeginEventScope("EditorSelectionHighlight");
            context.ClearRenderTargetView(maskBuffer, default);
            context.SetViewport(viewport);
            context.SetRenderTarget(maskBuffer, depthStencil);

            bool hasDrawn = false;
            maskContext.BeginFrame(context);
            foreach (var item in SelectionCollection.Global)
            {
                if (item is GameObject obj)
                {
                    foreach (var component in obj.Components)
                    {
                        if (component is IEditorHighlightable highlightable)
                        {
                            highlightable.DrawHighlight(maskContext);
                            hasDrawn = true;
                        }
                    }
                }
            }
            maskContext.EndFrame();
            if (!hasDrawn)
            {
                return;
            }

            OutlineParams outlineParams = new(viewport.Size, 10, Colors.Orange, Colors.Orange);
            outlineParams.FillColor.W = 0.4f;
            outlineParamsBuffer.Update(context, outlineParams);

            context.SetRenderTarget(target, null);

            psoEdge.Bindings.SetSRV("inputTex", maskBuffer);
            context.SetGraphicsPipelineState(psoEdge);
            context.DrawInstanced(4, 1, 0, 0);
        }

        public void Init()
        {
            var device = Application.GraphicsDevice;
            maskContext = new();

            outlineParamsBuffer = new(CpuAccessFlags.Write);
            psoEdge = device.CreateGraphicsPipelineState(new()
            {
                Pipeline = new()
                {
                    VertexShader = "HexaEngine.Core:shaders/quad.hlsl",
                    PixelShader = "HexaEngine.Editor:shaders/outline/draw.hlsl",
                },
                State = GraphicsPipelineStateDesc.DefaultAlphaBlendFullscreen
            });

            psoEdge.Bindings.SetCBV("OutlineParams", outlineParamsBuffer);
        }

        public void Release()
        {
            maskBuffer?.Dispose();
            maskBuffer = null;
            outlineParamsBuffer?.Dispose();
            outlineParamsBuffer = null!;
            psoEdge?.Dispose();
            psoEdge = null!;
            maskContext.Dispose();
            maskContext = null!;
        }

        public class MaskContext : EditorHighlightContext, IDisposable
        {
            public readonly IGraphicsDevice device;
            public readonly IGraphicsPipeline pipeline;
            private IGraphicsContext context = null!;
            private readonly ConstantBuffer<Matrix4x4> paramsBuffer;
            private readonly Dictionary<PSOCacheKey, IGraphicsPipelineState> psoCache = [];
            private readonly HashSet<PSOCacheKey> toRemovePsos = [];

            public struct PSOCacheKey : IEquatable<PSOCacheKey>
            {
                public InputElementDescription[] InputElements;
                public PrimitiveTopology Topology;

                public PSOCacheKey(InputElementDescription[] inputElements, PrimitiveTopology topology)
                {
                    InputElements = inputElements;
                    Topology = topology;
                }

                public readonly override bool Equals(object? obj)
                {
                    return obj is PSOCacheKey key && Equals(key);
                }

                public readonly bool Equals(PSOCacheKey other)
                {
                    return InputElements.SequenceEqual(other.InputElements) && Topology == other.Topology;
                }

                public readonly override int GetHashCode()
                {
                    HashCode hash = new();
                    foreach (var element in InputElements)
                    {
                        hash.Add(element);
                    }
                    hash.Add(Topology);
                    return hash.ToHashCode();
                }

                public static bool operator ==(PSOCacheKey left, PSOCacheKey right)
                {
                    return left.Equals(right);
                }

                public static bool operator !=(PSOCacheKey left, PSOCacheKey right)
                {
                    return !(left == right);
                }
            }

            public MaskContext()
            {
                device = Application.GraphicsDevice;
                pipeline = device.CreateGraphicsPipeline(new GraphicsPipelineDesc()
                {
                    VertexShader = "HexaEngine.Editor:shaders/outline/vs.hlsl",
                    PixelShader = "HexaEngine.Editor:shaders/outline/mask.hlsl",
                });
                paramsBuffer = new(CpuAccessFlags.Write);
            }

            public void BeginFrame(IGraphicsContext context)
            {
                this.context = context;
                foreach (var kv in psoCache)
                {
                    toRemovePsos.Add(kv.Key);
                }
            }

            public void EndFrame()
            {
                foreach (var kv in toRemovePsos)
                {
                    psoCache.Remove(kv, out var pso);
                    pso?.Dispose();
                }
            }

            public override IGraphicsContext GraphicsContext => context;

            public override void Begin(InputElementDescription[] inputElements, PrimitiveTopology topology)
            {
                bool hasPosition = false;
                foreach (var element in inputElements)
                {
                    if (element.SemanticName == "POSITION" && element.SemanticIndex == 0)
                    {
                        hasPosition = true;
                    }
                }

                if (!hasPosition)
                {
                    throw new InvalidOperationException("Editor selection highlight requires vertex positions.");
                }
                PSOCacheKey key = new(inputElements, topology);

                if (!psoCache.TryGetValue(key, out var pso))
                {
                    pso = device.CreateGraphicsPipelineState(pipeline, new()
                    {
                        InputElements = inputElements,
                        Blend = BlendDescription.Opaque,
                        BlendFactor = Vector4.One,
                        DepthStencil = DepthStencilDescription.DepthReadEquals,
                        Rasterizer = RasterizerDescription.CullNone,
                        Topology = topology,
                    });

                    pso.Bindings.SetCBV("WorldBuffer", paramsBuffer);
                    psoCache[key] = pso;
                    context.SetGraphicsPipelineState(pso);
                    return;
                }

                toRemovePsos.Remove(key);
                context.SetGraphicsPipelineState(pso);
            }

            public override void End()
            {
            }

            public override void SetTransform(Matrix4x4 transform, bool transpose)
            {
                if (transpose)
                {
                    transform = Matrix4x4.Transpose(transform);
                }
                paramsBuffer.Update(context, transform);
            }

            public void Dispose()
            {
                foreach (var kv in psoCache)
                {
                    kv.Value.Dispose();
                }
                psoCache.Clear();
                pipeline.Dispose();
                paramsBuffer.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}