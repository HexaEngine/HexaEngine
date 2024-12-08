namespace HexaEngine.Graphics
{
    using HexaEngine.Core;
    using HexaEngine.Core.Graphics;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public class RenderQueueGroup : IDisposable
    {
        private readonly ICommandBuffer commandBuffer = null!;
        private readonly RenderQueueIndex index;
        private QueueGroupFlags flags;
        private readonly List<IDrawable> renderers = [];
        private readonly string pass;
        private uint baseIndex;
        private uint lastIndex;
        private bool disposedValue;

        private bool isDirty = true;

        public RenderQueueGroup(RenderQueueIndex index, QueueGroupFlags flags, string pass)
        {
            this.index = index;
            this.flags = flags;
            this.pass = pass;

            if (!IsDynamic)
            {
                commandBuffer = Application.GraphicsDevice.CreateCommandBuffer();
            }
        }

        public RenderQueueIndex Index => index;

        public uint BaseIndex => baseIndex;

        public uint LastIndex => lastIndex;

        public bool IsDynamic
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (flags & QueueGroupFlags.Dynamic) != 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (value)
                {
                    flags |= QueueGroupFlags.Dynamic;
                }
                else
                {
                    flags &= ~QueueGroupFlags.Dynamic;
                }
            }
        }

        public int Count => renderers.Count;

        public void Add(IDrawable renderer)
        {
            isDirty = true;
            renderer.DrawableInvalidated += DrawableInvalidated;
            renderers.Add(renderer);
            renderers.Sort(SortRendererAscending.Instance);
            lastIndex = renderers[0].QueueIndex;
            baseIndex = renderers[^1].QueueIndex;
        }

        private void DrawableInvalidated(IDrawable sender)
        {
            isDirty = true;
        }

        public bool Remove(IDrawable renderer)
        {
            if (renderers.Remove(renderer))
            {
                isDirty = true;
                renderer.DrawableInvalidated -= DrawableInvalidated;
                if (renderers.Count > 0)
                {
                    lastIndex = renderers[0].QueueIndex;
                    baseIndex = renderers[^1].QueueIndex;
                }
                else
                {
                    lastIndex = (uint)index;
                }
                return true;
            }
            return false;
        }

        public void Clear()
        {
            renderers.Clear();
            lastIndex = (uint)index;
        }

        public void Invalidate()
        {
            isDirty = true;
        }

        public void ExecuteGroup(IGraphicsContext context)
        {
            if (IsDynamic)
            {
                Draw(context);
            }

            RecordList();
            context.ExecuteCommandBuffer(commandBuffer);
        }

        private void RecordList()
        {
            if (IsDynamic)
            {
                throw new InvalidOperationException($"Cannot record a dynamic group, that contains conditional render code.");
            }

            if (!isDirty)
            {
                return;
            }

            commandBuffer.Begin();

            Draw(commandBuffer);

            commandBuffer.End();
            isDirty = false;
        }

        private void Draw(IGraphicsContext context)
        {
            for (var i = 0; i < renderers.Count; i++)
            {
                renderers[i].Draw(context, pass);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                renderers.Clear();
                commandBuffer.Dispose();
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