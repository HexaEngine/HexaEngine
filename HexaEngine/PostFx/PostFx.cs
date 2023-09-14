namespace HexaEngine.PostFx
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Mathematics;
    using HexaEngine.Rendering.Graph;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    public enum FxPrimitive
    {
        Undefined,
        Quad,
        Sphere,
        Cube
    }

    public class PostFx : IPostFx
    {
        public FxPrimitive Primitive { get; set; }

        public PrimitiveTopology Topology { get; set; }

        public string Name { get; set; }

        public PostFxFlags Flags { get; set; }

        [XmlIgnore]
        public bool Enabled { get; set; }

        public PostFxPassCollection Passes { get; set; } = new();

        public bool Initialized { get; }

        public event Action<IPostFx, bool>? OnEnabledChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public event Action<IPostFx>? OnReload;

        public void Save(string path)
        {
            XmlSerializer serializer = new(typeof(PostFx));
            var fs = File.Create(path);
            serializer.Serialize(fs, this);
            fs.Close();
        }

        public static PostFx Load(string path)
        {
            XmlSerializer serializer = new(typeof(PostFx));
            var fs = File.OpenRead(path);
            var result = serializer.Deserialize(fs);
            fs.Close();
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
            return (PostFx)result;
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Draw(IGraphicsContext context, GraphResourceBuilder creator)
        {
            throw new NotImplementedException();
        }

        public Task InitializeAsync(IGraphicsDevice device, PostFxDependencyBuilder builder, int width, int height, ShaderMacro[] macros)
        {
            throw new NotImplementedException();
        }

        public void Initialize(IGraphicsDevice device, PostFxDependencyBuilder builder, GraphResourceBuilder creator, int width, int height, ShaderMacro[] macros)
        {
            throw new NotImplementedException();
        }

        public void Resize(int width, int height)
        {
            throw new NotImplementedException();
        }

        public void SetInput(IShaderResourceView view, ITexture2D resource)
        {
            throw new NotImplementedException();
        }

        public void SetOutput(IRenderTargetView view, ITexture2D resource, Viewport viewport)
        {
            throw new NotImplementedException();
        }

        public void Update(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }
    }

    public class PostFxPassCollection : List<IPostFxPass>, IXmlSerializable
    {
        private static readonly Dictionary<Type, XmlSerializer> _cache = new();

        XmlSchema IXmlSerializable.GetSchema()
        {
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("Passes");

            bool iter = true;
            while (iter)
            {
                switch (reader.Name)
                {
                    case nameof(GraphicsPass):
                        {
                            XmlSerializer xmlSerializer = new(typeof(GraphicsPass));
#pragma warning disable CS8604 // Possible null reference argument for parameter 'item' in 'void List<IPostFxPass>.ObjectAdded(IPostFxPass item)'.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                            Add((IPostFxPass)xmlSerializer.Deserialize(reader));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8604 // Possible null reference argument for parameter 'item' in 'void List<IPostFxPass>.ObjectAdded(IPostFxPass item)'.
                        }
                        break;

                    case nameof(ComputePass):
                        {
                            XmlSerializer xmlSerializer = new(typeof(ComputePass));
#pragma warning disable CS8604 // Possible null reference argument for parameter 'item' in 'void List<IPostFxPass>.ObjectAdded(IPostFxPass item)'.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                            Add((IPostFxPass)xmlSerializer.Deserialize(reader));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8604 // Possible null reference argument for parameter 'item' in 'void List<IPostFxPass>.ObjectAdded(IPostFxPass item)'.
                        }
                        break;

                    default:
                        iter = false;
                        break;
                }
            }

            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (IPostFxPass dispatcher in this)
            {
                var type = dispatcher.GetType();

                if (!_cache.TryGetValue(type, out var xmlSerializer))
                {
                    xmlSerializer = new(dispatcher.GetType());
                    _cache.Add(type, xmlSerializer);
                }

                xmlSerializer.Serialize(writer, dispatcher);
            }
        }
    }

    public interface IPostFxPass : IDisposable
    {
        public List<string> SamplerStates { get; set; }

        public List<string> ConstantBufferViews { get; set; }

        public List<string> ShaderResourceViews { get; set; }

        public List<string> UnorderedAccessViews { get; set; }

        public string? OutputBuffer { get; set; }

        public Task Initialize(IGraphicsDevice device);

        public void Update(IGraphicsContext context);

        public void Render(IGraphicsContext context);
    }

    public class GraphicsPass : IPostFxPass
    {
#pragma warning disable CS0169 // The field 'GraphicsPass.cbvs' is never used
        private unsafe void** cbvs;
#pragma warning restore CS0169 // The field 'GraphicsPass.cbvs' is never used
#pragma warning disable CS0169 // The field 'GraphicsPass.srvs' is never used
        private unsafe void** srvs;
#pragma warning restore CS0169 // The field 'GraphicsPass.srvs' is never used

        public string? OutputBuffer { get; set; }

        public List<string> SamplerStates { get; set; } = new();

        public List<string> ConstantBufferViews { get; set; } = new();

        public List<string> ShaderResourceViews { get; set; } = new();

        public List<string> UnorderedAccessViews { get; set; } = new();

        public GraphicsPipelineDesc Description { get; set; } = default;

        public GraphicsPipelineState State { get; set; } = GraphicsPipelineState.Default;

        public Task Initialize(IGraphicsDevice device)
        {
            throw new NotImplementedException();
        }

        public void Render(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void Update(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class ComputePass : IPostFxPass
    {
        public List<string> SamplerStates { get; set; } = new();

        public List<string> ConstantBufferViews { get; set; } = new();

        public List<string> ShaderResourceViews { get; set; } = new();

        public List<string> UnorderedAccessViews { get; set; } = new();

        public string? OutputBuffer { get; set; }

        public ComputePipelineDesc Description { get; set; } = default;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task Initialize(IGraphicsDevice device)
        {
            throw new NotImplementedException();
        }

        public void Render(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }

        public void Update(IGraphicsContext context)
        {
            throw new NotImplementedException();
        }
    }
}