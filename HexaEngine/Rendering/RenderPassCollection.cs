namespace HexaEngine.Rendering
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Core.Unsafes;
    using HexaEngine.Mathematics;
    using HexaEngine.Objects;
    using HexaEngine.Scenes;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public class RenderPassCollection : IList<IRenderPass>
    {
        private readonly List<IRenderPass> passes = new();
        private readonly Dictionary<string, IBuffer> sharedBuffers = new();
        private readonly Dictionary<string, IShaderResourceView> sharedShaderViews = new();
        private readonly Dictionary<string, IRenderTargetView> sharedTargetViews = new();
        private readonly Dictionary<string, IDepthStencilView> sharedDepthViews = new();
        private readonly List<IPrimitive> sharedPrimitives = new();
        public readonly Dictionary<string, Pointer> States = new();
        private IRenderPass[]? orderedPasses;

        public RenderPassCollection()
        {
        }

        public IRenderPass this[int index] { get => ((IList<IRenderPass>)passes)[index]; set => ((IList<IRenderPass>)passes)[index] = value; }

        public int Count => ((ICollection<IRenderPass>)passes).Count;

        public bool IsReadOnly => ((ICollection<IRenderPass>)passes).IsReadOnly;

        public void Initialize(IGraphicsDevice device, int width, int height)
        {
            orderedPasses = passes.OrderBy(x => x.Priority).ToArray();
            for (int i = 0; i < orderedPasses.Length; i++)
            {
                orderedPasses[i].Initialize(device, width, height, this);
            }
        }

        public void Draw(IGraphicsContext context, Scene scene, Viewport viewport)
        {
            orderedPasses = passes.OrderBy(x => x.Priority).ToArray();
            for (int i = 0; i < orderedPasses.Length; i++)
            {
                orderedPasses[i].Draw(context, scene, viewport);
            }
        }

        public void Update(IGraphicsContext context, Scene scene)
        {
            orderedPasses = passes.OrderBy(x => x.Priority).ToArray();
            for (int i = 0; i < orderedPasses.Length; i++)
            {
                orderedPasses[i].Update(context, scene);
            }
        }

        public void ResizeBegin()
        {
            orderedPasses = passes.OrderBy(x => x.Priority).ToArray();
            for (int i = 0; i < orderedPasses.Length; i++)
            {
                orderedPasses[i].ResizeBegin();
            }
        }

        public void ResizeEnd(IGraphicsDevice device, int width, int height)
        {
            orderedPasses = passes.OrderBy(x => x.Priority).ToArray();
            for (int i = 0; i < orderedPasses.Length; i++)
            {
                orderedPasses[i].ResizeEnd(device, width, height, this);
            }
        }

        public void Add(IRenderPass item)
        {
            ((ICollection<IRenderPass>)passes).Add(item);
        }

        public void Clear()
        {
            ((ICollection<IRenderPass>)passes).Clear();
        }

        public bool Contains(IRenderPass item)
        {
            return ((ICollection<IRenderPass>)passes).Contains(item);
        }

        public void CopyTo(IRenderPass[] array, int arrayIndex)
        {
            ((ICollection<IRenderPass>)passes).CopyTo(array, arrayIndex);
        }

        public IEnumerator<IRenderPass> GetEnumerator()
        {
            return ((IEnumerable<IRenderPass>)passes).GetEnumerator();
        }

        public int IndexOf(IRenderPass item)
        {
            return ((IList<IRenderPass>)passes).IndexOf(item);
        }

        public void Insert(int index, IRenderPass item)
        {
            ((IList<IRenderPass>)passes).Insert(index, item);
        }

        public bool Remove(IRenderPass item)
        {
            return ((ICollection<IRenderPass>)passes).Remove(item);
        }

        public void RemoveAt(int index)
        {
            ((IList<IRenderPass>)passes).RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)passes).GetEnumerator();
        }

        public T GetSharedPrimitive<T>() where T : class, IPrimitive
        {
            for (int i = 0; i < sharedPrimitives.Count; i++)
            {
                if (sharedPrimitives[i] is T t)
                    return t;
            }
            throw new KeyNotFoundException();
        }

        public bool TryGetSharedPrimitive<T>([NotNullWhen(true)] out T? tOut) where T : class, IPrimitive
        {
            tOut = null;
            for (int i = 0; i < sharedPrimitives.Count; i++)
            {
                if (sharedPrimitives[i] is T t)
                {
                    tOut = t;
                    return true;
                }
            }
            return false;
        }

        public void AddSharedPrimitive<T>(T t) where T : class, IPrimitive
        {
            sharedPrimitives.Add(t);
        }

        public T? GetByType<T>() where T : class, IRenderPass
        {
            for (int i = 0; i < passes.Count; i++)
            {
                if (passes[i] is T t)
                    return t;
            }
            return null;
        }

        public IBuffer GetSharedBuffer(string name)
        {
            return sharedBuffers[name];
        }

        public IBuffer GetSharedBuffer<T>() where T : struct
        {
            return sharedBuffers[typeof(T).Name];
        }

        public IBuffer GetOrCreateSharedBuffer(string name, IGraphicsDevice device, BufferDescription desc)
        {
            if (sharedBuffers.TryGetValue(name, out var buffer))
            {
                return buffer;
            }
            buffer = device.CreateBuffer(desc);
            sharedBuffers.Add(name, buffer);
            return buffer;
        }

        public IBuffer GetOrCreateSharedBuffer<T>(IGraphicsDevice device, BufferDescription desc) where T : struct
        {
            if (sharedBuffers.TryGetValue(typeof(T).Name, out var buffer))
            {
                return buffer;
            }
            buffer = device.CreateBuffer(desc);
            sharedBuffers.Add(typeof(T).Name, buffer);
            return buffer;
        }

        public void AddSharedBuffer(string name, IBuffer buffer)
        {
            sharedBuffers.Add(name, buffer);
        }

        public void AddSharedBuffer<T>(IBuffer buffer) where T : struct
        {
            sharedBuffers.Add(typeof(T).Name, buffer);
        }

        public IShaderResourceView GetSharedShaderView(string name)
        {
            return sharedShaderViews[name];
        }

        public void AddSharedShaderView(string name, IShaderResourceView srv)
        {
            sharedShaderViews.Add(name, srv);
        }

        public void AddSharedShaderViews(string name, IShaderResourceView[] srvs)
        {
            for (int i = 0; i < srvs.Length; i++)
            {
                sharedShaderViews.Add(name + i, srvs[i]);
            }
        }

        public void SetSharedShaderView(string name, IShaderResourceView srv)
        {
            sharedShaderViews[name] = srv;
        }

        public void SetSharedShaderViews(string name, IShaderResourceView[] srvs)
        {
            for (int i = 0; i < srvs.Length; i++)
            {
                sharedShaderViews[name + i] = srvs[i];
            }
        }

        public IRenderTargetView GetSharedRenderTarget(string name)
        {
            return sharedTargetViews[name];
        }

        public void AddSharedRenderTarget(string name, IRenderTargetView rtv)
        {
            sharedTargetViews.Add(name, rtv);
        }

        public void SetSharedRenderTarget(string name, IRenderTargetView rtv)
        {
            sharedTargetViews[name] = rtv;
        }

        public IDepthStencilView GetSharedDepthView(string name)
        {
            return sharedDepthViews[name];
        }

        public void AddSharedDepthView(string name, IDepthStencilView dsv)
        {
            sharedDepthViews.Add(name, dsv);
        }

        public void SetSharedDepthView(string name, IDepthStencilView dsv)
        {
            sharedDepthViews[name] = dsv;
        }
    }
}